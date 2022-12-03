using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VYaml.SourceGenerator;

class WorkItem
{
    public TypeDeclarationSyntax Syntax { get; }

    public WorkItem(TypeDeclarationSyntax syntax)
    {
        Syntax = syntax;
    }

    public TypeMeta? Analyze(in GeneratorExecutionContext context, ReferenceSymbols references)
    {
        var semanticModel = context.Compilation.GetSemanticModel(Syntax.SyntaxTree);
        var symbol = semanticModel.GetDeclaredSymbol(Syntax, context.CancellationToken);
        if (symbol is INamedTypeSymbol typeSymbol)
        {
            var attributeData = symbol.GetAttributes().FirstOrDefault(x =>
            {
                var attribute = references.YamlObjectAttribute;
                return SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribute);
            });
            if (attributeData is null)
            {
                return null;
            }
            return new TypeMeta(Syntax, typeSymbol, attributeData, references);
        }
        return null;
    }
}
