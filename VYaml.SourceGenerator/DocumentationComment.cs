using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VYaml.SourceGenerator;

// Extracts plain text from XML documentation comments, used as "description" in the YAML schema.
//
// The comment is read from the syntax trivia of the declaration instead of ISymbol.GetDocumentationCommentXml(),
// which is empty unless the project sets <GenerateDocumentationFile>. The trivia is also dropped by the parser in
// that case (DocumentationMode.None), so the declaration is re-parsed with documentation comments enabled.
// (same approach as Cysharp/ConsoleAppFramework; see https://github.com/dotnet/roslyn/issues/58210)
static class DocumentationComment
{
    public static string? GetSummary(ISymbol symbol)
    {
        return Find(symbol)?.Content.OfType<XmlElementSyntax>().FirstOrDefault(x => NameOf(x) == "summary") is { } summary
            ? ToText(summary.Content)
            : null;
    }

    // <param> of the type itself: documentation of positional record parameters
    public static IReadOnlyDictionary<string, string> GetParams(ISymbol symbol)
    {
        var result = new Dictionary<string, string>();
        if (Find(symbol) is { } doc)
        {
            foreach (var param in doc.Content.OfType<XmlElementSyntax>().Where(x => NameOf(x) == "param"))
            {
                var name = param.StartTag.Attributes.OfType<XmlNameAttributeSyntax>().FirstOrDefault()?.Identifier.Identifier.ValueText;
                if (name != null && ToText(param.Content) is { } text)
                {
                    result[name] = text;
                }
            }
        }
        return result;
    }

    static DocumentationCommentTriviaSyntax? Find(ISymbol symbol)
    {
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            if (FindOnDeclaration(reference.GetSyntax()) is { } doc)
            {
                return doc;
            }
        }
        return null;
    }

    static DocumentationCommentTriviaSyntax? FindOnDeclaration(SyntaxNode node)
    {
        if (node.SyntaxTree.Options.DocumentationMode == DocumentationMode.None)
        {
            // The parser dropped the structured trivia; parse this declaration alone with it enabled.
            // The fragment loses its usings and namespace, which does not matter for the trivia.
            var options = ((CSharpParseOptions)node.SyntaxTree.Options).WithDocumentationMode(DocumentationMode.Parse);
            node = CSharpSyntaxTree.ParseText(node.ToFullString(), options).GetRoot();
        }

        foreach (var trivia in node.GetLeadingTrivia())
        {
            if (trivia.GetStructure() is DocumentationCommentTriviaSyntax doc)
            {
                return doc;
            }
        }
        return null;
    }

    static string NameOf(XmlElementSyntax element) => element.StartTag.Name.LocalName.ValueText;

    static string? ToText(SyntaxList<XmlNodeSyntax> content)
    {
        var sb = new StringBuilder();
        Append(sb, content);
        var lines = sb.ToString()
            .Split('\n')
            .Select(x => x.Trim())
            .ToArray();

        // join wrapped lines, keep blank lines as paragraph breaks
        var result = new StringBuilder();
        var pendingBreak = false;
        foreach (var line in lines)
        {
            if (line.Length == 0)
            {
                pendingBreak = result.Length > 0;
                continue;
            }
            if (result.Length > 0)
            {
                result.Append(pendingBreak ? "\n" : " ");
            }
            result.Append(line);
            pendingBreak = false;
        }
        return result.Length > 0 ? result.ToString() : null;
    }

    static void Append(StringBuilder sb, SyntaxList<XmlNodeSyntax> content)
    {
        foreach (var node in content)
        {
            switch (node)
            {
                case XmlTextSyntax text:
                    AppendTokens(sb, text.TextTokens);
                    break;
                case XmlCDataSectionSyntax cdata:
                    AppendTokens(sb, cdata.TextTokens);
                    break;
                case XmlElementSyntax element:
                    switch (NameOf(element))
                    {
                        case "para":
                            sb.Append("\n\n");
                            Append(sb, element.Content);
                            sb.Append("\n\n");
                            break;
                        default:
                            if (element.Content.Count > 0)
                            {
                                Append(sb, element.Content); // <see cref="X">text</see>, <c>, <b>, ...
                            }
                            else
                            {
                                AppendReference(sb, element.StartTag.Attributes);
                            }
                            break;
                    }
                    break;
                case XmlEmptyElementSyntax empty:
                    if (empty.Name.LocalName.ValueText == "br")
                    {
                        sb.Append("\n\n");
                    }
                    else
                    {
                        AppendReference(sb, empty.Attributes); // <see cref/>, <see langword/>, <paramref name/>, <see href/>
                    }
                    break;
                // XmlCommentSyntax, XmlProcessingInstructionSyntax: nothing
            }
        }
    }

    static void AppendTokens(StringBuilder sb, SyntaxTokenList tokens)
    {
        foreach (var token in tokens)
        {
            // the exterior "///" is not part of the token; newlines separate the lines for ToText
            sb.Append(token.IsKind(SyntaxKind.XmlTextLiteralNewLineToken) ? "\n" : token.ValueText);
        }
    }

    static void AppendReference(StringBuilder sb, SyntaxList<XmlAttributeSyntax> attributes)
    {
        foreach (var attribute in attributes)
        {
            switch (attribute)
            {
                case XmlCrefAttributeSyntax cref:
                    sb.Append(ShortName(cref.Cref.ToString()));
                    return;
                case XmlNameAttributeSyntax name: // paramref / typeparamref
                    sb.Append(name.Identifier.Identifier.ValueText);
                    return;
                case XmlTextAttributeSyntax text when text.Name.LocalName.ValueText is "langword" or "href":
                    foreach (var token in text.TextTokens)
                    {
                        sb.Append(token.ValueText);
                    }
                    return;
            }
        }
    }

    // "Namespace.Type{T}" -> "Type", "Type.Method(int)" -> "Method"
    static string ShortName(string cref)
    {
        var name = cref;
        var paren = name.IndexOf('(');
        if (paren >= 0) name = name.Substring(0, paren);
        var generic = name.IndexOfAny(new[] { '{', '<' });
        if (generic >= 0) name = name.Substring(0, generic);
        var dot = name.LastIndexOf('.');
        if (dot >= 0) name = name.Substring(dot + 1);
        return name.Trim();
    }
}
