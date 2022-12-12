using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VYaml.SourceGenerator;

class SyntaxContextReceiver : ISyntaxContextReceiver
{
    readonly HashSet<TypeDeclarationSyntax> classDeclarations = new();

    public IReadOnlyList<WorkItem> GetWorkItems()
    {
        return classDeclarations.Select(x => new WorkItem(x)).ToArray();
    }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        var node = context.Node;
        if (node is ClassDeclarationSyntax
            or StructDeclarationSyntax
            or RecordDeclarationSyntax
            or InterfaceDeclarationSyntax)
        {
            var typeSyntax = (TypeDeclarationSyntax)node;
            if (typeSyntax.AttributeLists.Count > 0)
            {
                var attr = typeSyntax.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .FirstOrDefault(x => x.Name.ToString() is
                        "YamlObject" or
                        "YamlObjectAttribute" or
                        "VYaml.Annotations.YamlObject" or
                        "VYaml.Annotations.YamlObjectAttribute");
                if (attr != null)
                {
                    classDeclarations.Add(typeSyntax);
                }
            }
        }
    }
}
