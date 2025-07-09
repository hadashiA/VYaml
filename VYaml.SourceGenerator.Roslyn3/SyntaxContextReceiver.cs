using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VYaml.SourceGenerator;

class SyntaxContextReceiver : ISyntaxContextReceiver
{
    readonly HashSet<TypeDeclarationSyntax> classDeclarations = new();
    readonly HashSet<TypeDeclarationSyntax> unionMemberDeclarations = new();

    public IReadOnlyList<WorkItem> GetWorkItems()
    {
        return classDeclarations.Select(x => new WorkItem(x)).ToArray();
    }

    public IReadOnlyList<TypeDeclarationSyntax> GetUnionMemberDeclarations()
    {
        return unionMemberDeclarations.ToArray();
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
                foreach (var attr in typeSyntax.AttributeLists.SelectMany(x => x.Attributes))
                {
                    var name = attr.Name.ToString();
                    if (name is "YamlObject" or "YamlObjectAttribute" or
                        "VYaml.Annotations.YamlObject" or "VYaml.Annotations.YamlObjectAttribute")
                    {
                        classDeclarations.Add(typeSyntax);
                    }
                    else if (name is "YamlUnionMember" or "YamlUnionMemberAttribute" or
                        "VYaml.Annotations.YamlUnionMember" or "VYaml.Annotations.YamlUnionMemberAttribute")
                    {
                        unionMemberDeclarations.Add(typeSyntax);
                    }
                }
            }
        } }
}
