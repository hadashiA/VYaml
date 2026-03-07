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
        var provider = context.SyntaxProvider
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

        // Generate the source code.
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider.Collect()),
            (sourceProductionContext, t) =>
            {
                var (compilation, list) = t;
                var references = ReferenceSymbols.Create(compilation);
                if (references is null)
                {
                    return;
                }

                var codeWriter = new CodeWriter();

                foreach (var (x, _) in list)
                {
                    var typeMeta = new TypeMeta(
                        (TypeDeclarationSyntax)x.TargetNode,
                        (INamedTypeSymbol)x.TargetSymbol,
                        x.Attributes.First(),
                        references);

                    if (Emitter.TryEmit(typeMeta, codeWriter, references, sourceProductionContext))
                    {
                        var fullType = typeMeta.FullTypeName
                            .Replace("global::", "")
                            .Replace("<", "_")
                            .Replace(">", "_")
                            .Replace(",", "_")
                            .Replace(" ", "");

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
