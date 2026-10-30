using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace VYaml.SourceGenerator;

// Extracts plain text from XML documentation comments, used as "description" in the YAML schema.
// Only available when the compilation parses documentation comments (e.g. <GenerateDocumentationFile>).
static class DocumentationComment
{
    public static string? GetSummary(ISymbol symbol)
    {
        return Parse(symbol)?.Element("summary") is { } summary ? ToText(summary) : null;
    }

    // <param> of the type itself: documentation of positional record parameters
    public static IReadOnlyDictionary<string, string> GetParams(ISymbol symbol)
    {
        var result = new Dictionary<string, string>();
        if (Parse(symbol) is { } root)
        {
            foreach (var param in root.Elements("param"))
            {
                if (param.Attribute("name")?.Value is { } name && ToText(param) is { } text)
                {
                    result[name] = text;
                }
            }
        }
        return result;
    }

    static XElement? Parse(ISymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml(expandIncludes: false);
        if (string.IsNullOrWhiteSpace(xml)) return null;
        try
        {
            // fragments of the member element can be returned for some symbols
            return XElement.Parse($"<root>{xml}</root>").Element("member") ?? XElement.Parse($"<root>{xml}</root>");
        }
        catch (XmlException)
        {
            return null;
        }
    }

    static string? ToText(XElement element)
    {
        var sb = new StringBuilder();
        Append(sb, element);
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

    static void Append(StringBuilder sb, XElement element)
    {
        foreach (var node in element.Nodes())
        {
            switch (node)
            {
                case XText text:
                    sb.Append(text.Value);
                    break;
                case XElement { Name.LocalName: "para" } para:
                    sb.Append("\n\n");
                    Append(sb, para);
                    sb.Append("\n\n");
                    break;
                case XElement { Name.LocalName: "br" }:
                    sb.Append("\n\n");
                    break;
                case XElement e when e.Attribute("cref")?.Value is { } cref:
                    sb.Append(e.IsEmpty ? ShortName(cref) : e.Value);
                    break;
                case XElement e when e.Attribute("langword")?.Value is { } langword:
                    sb.Append(langword);
                    break;
                case XElement e when e.IsEmpty && e.Attribute("name")?.Value is { } name:
                    sb.Append(name);
                    break;
                case XElement e when e.IsEmpty && e.Attribute("href")?.Value is { } href:
                    sb.Append(href);
                    break;
                case XElement e:
                    Append(sb, e);
                    break;
            }
        }
    }

    // "T:Namespace.Type`1" -> "Type", "M:Namespace.Type.Method(System.Int32)" -> "Method"
    static string ShortName(string cref)
    {
        var name = cref.Length > 2 && cref[1] == ':' ? cref.Substring(2) : cref;
        var paren = name.IndexOf('(');
        if (paren >= 0) name = name.Substring(0, paren);
        var dot = name.LastIndexOf('.');
        if (dot >= 0) name = name.Substring(dot + 1);
        var backtick = name.IndexOf('`');
        if (backtick >= 0) name = name.Substring(0, backtick);
        return name;
    }
}
