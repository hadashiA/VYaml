using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

namespace VYaml.SourceGenerator;

[Generator]
public class VYamlIncrementalSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var yamlObjectProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                context,
                "VYaml.Annotations.YamlObjectAttribute",
                static (node, cancellation) =>
                {
                    return node is ClassDeclarationSyntax
                        or StructDeclarationSyntax
                        or RecordDeclarationSyntax
                        or InterfaceDeclarationSyntax;
                },
                static (context, cancellation) => context)
            .Combine(context.CompilationProvider)
            .WithComparer(Comparer.Instance);

        var yamlUnionMemberProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                context,
                "VYaml.Annotations.YamlUnionMemberAttribute",
                static (node, cancellation) =>
                {
                    return node is ClassDeclarationSyntax
                        or StructDeclarationSyntax
                        or RecordDeclarationSyntax;
                },
                static (context, cancellation) => context)
            .Combine(context.CompilationProvider)
            .WithComparer(Comparer.Instance);

        // Generate the source code.
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(yamlObjectProvider.Collect()).Combine(yamlUnionMemberProvider.Collect()),
            (sourceProductionContext, t) =>
            {
                var ((compilation, yamlObjectList), yamlUnionMemberList) = t;
                var references = ReferenceSymbols.Create(compilation);
                if (references is null)
                {
                    return;
                }

                // Collect union member information
                var unionMembersByType = new Dictionary<INamedTypeSymbol, List<(string Tag, INamedTypeSymbol SubType)>>(SymbolEqualityComparer.Default);
                foreach (var (x, _) in yamlUnionMemberList)
                {
                    var attr = x.Attributes.First();
                    if (attr.ConstructorArguments.Length == 2)
                    {
                        var tag = (string)attr.ConstructorArguments[0].Value!;
                        var unionType = (INamedTypeSymbol)attr.ConstructorArguments[1].Value!;
                        
                        if (!unionMembersByType.TryGetValue(unionType, out var list))
                        {
                            list = new List<(string, INamedTypeSymbol)>();
                            unionMembersByType[unionType] = list;
                        }
                        list.Add((tag, (INamedTypeSymbol)x.TargetSymbol));
                    }
                }

                var codeWriter = new CodeWriter();

                foreach (var (x, _) in yamlObjectList)
                {
                    var typeMeta = new TypeMeta(
                        (TypeDeclarationSyntax)x.TargetNode,
                        (INamedTypeSymbol)x.TargetSymbol,
                        x.Attributes.First(),
                        references,
                        unionMembersByType);

                    if (Emitter.TryEmit(typeMeta, codeWriter, references, sourceProductionContext))
                    {
                        var fullType = typeMeta.FullTypeName
                            .Replace("global::", "")
                            .Replace("<", "_")
                            .Replace(">", "_");

                        sourceProductionContext.AddSource($"{fullType}.YamlFormatter.g.cs", codeWriter.ToString());
                    }
                    codeWriter.Clear();
                }
            });
    }
}

class Comparer : IEqualityComparer<(GeneratorAttributeSyntaxContext, Compilation)>
{
    public static readonly Comparer Instance = new();

    public bool Equals((GeneratorAttributeSyntaxContext, Compilation) x, (GeneratorAttributeSyntaxContext, Compilation) y)
    {
        return x.Item1.TargetNode.IsEquivalentTo(y.Item1.TargetNode);
    }

    public int GetHashCode((GeneratorAttributeSyntaxContext, Compilation) obj)
    {
        return obj.Item1.TargetNode.GetHashCode();
    }
}
