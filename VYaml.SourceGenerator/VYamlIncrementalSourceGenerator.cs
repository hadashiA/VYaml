using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

namespace VYaml.SourceGenerator;

static class TrackingNames
{
    public const string Model = "VYaml.Model";
    public const string Emit = "VYaml.Emit";
    public const string Roots = "VYaml.Roots";
    public const string RegistrationFacts = "VYaml.RegistrationFacts";
}

[Generator]
public class VYamlIncrementalSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. transform: symbols -> value-equatable TypeMetaModel (the cache key).
        var models = context.SyntaxProvider
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
                static (context, cancellation) =>
                {
                    var references = ReferenceSymbols.Create(context.SemanticModel.Compilation);
                    if (references is null)
                    {
                        return null;
                    }
                    return TypeMetaAnalyzer.Analyze(
                        (INamedTypeSymbol)context.TargetSymbol,
                        (TypeDeclarationSyntax)context.TargetNode,
                        context.Attributes.First(),
                        references);
                })
            .Where(static x => x is not null)
            .WithTrackingName(TrackingNames.Model);

        // 2. emit: model -> source string. Cached on the model, so the string is only
        //    rebuilt when a type's relevant shape actually changes.
        var emitted = models.Select(static (model, cancellation) =>
        {
            var source = model!.IsValid ? Emitter.Emit(model) : null;
            return new EmitResult(model.HintName, source, model.Diagnostics);
        }).WithTrackingName(TrackingNames.Emit);

        // 3. output: report diagnostics + AddSource. Skipped when the EmitResult is unchanged.
        context.RegisterSourceOutput(emitted, static (sourceProductionContext, result) =>
        {
            foreach (var diagnostic in result.Diagnostics)
            {
                sourceProductionContext.ReportDiagnostic(diagnostic.ToDiagnostic());
            }

            if (result.Source is { } source)
            {
                sourceProductionContext.AddSource(result.HintName, source);
            }
        });

        // 4. [assembly: YamlSerializable(typeof(...))] roots: serialization shapes that never
        //    appear as a member of any [YamlObject], declared explicitly for Native AOT.
        var roots = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, cancellation) =>
                    node is AttributeSyntax { Parent: AttributeListSyntax { Target: { } target } } attribute
                    && target.Identifier.IsKind(SyntaxKind.AssemblyKeyword)
                    && attribute.Name.ToString().Contains("YamlSerializable"),
                static (context, cancellation) => AnalyzeRootAttribute(context))
            .Where(static x => x is not null)
            .WithTrackingName(TrackingNames.Roots);

        // 5. per-assembly module-initializer registration file, aggregated over every model's
        //    registration statements plus the declared roots. See RegistrationEmitter for why
        //    this is the heart of Native AOT support.
        var registrationFacts = context.CompilationProvider
            .Select(static (compilation, cancellation) => (
                AssemblyName: compilation.AssemblyName ?? "Assembly",
                // module initializers need C# 9; older compilations keep the reflection-only path
                LangOk: compilation is CSharpCompilation csharp && csharp.LanguageVersion >= LanguageVersion.CSharp9,
                NeedsModuleInitializerPolyfill: compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ModuleInitializerAttribute") is null))
            .WithTrackingName(TrackingNames.RegistrationFacts);

        var registration = models.Collect()
            .Combine(roots.Collect())
            .Combine(registrationFacts);

        context.RegisterSourceOutput(registration, static (sourceProductionContext, source) =>
        {
            var ((models, roots), facts) = source;

            foreach (var root in roots)
            {
                foreach (var diagnostic in root!.Diagnostics)
                {
                    sourceProductionContext.ReportDiagnostic(diagnostic.ToDiagnostic());
                }
            }

            if (!facts.LangOk)
            {
                return;
            }

            var statements = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var model in models)
            {
                if (model is { IsValid: true })
                {
                    foreach (var statement in model.AotRegistrations)
                    {
                        statements.Add(statement);
                    }
                }
            }
            foreach (var root in roots)
            {
                foreach (var statement in root!.Statements)
                {
                    statements.Add(statement);
                }
            }

            if (statements.Count == 0)
            {
                return;
            }

            sourceProductionContext.AddSource(
                "VYamlGeneratedRegistration.g.cs",
                RegistrationEmitter.Emit(facts.AssemblyName, statements));

            if (facts.NeedsModuleInitializerPolyfill)
            {
                sourceProductionContext.AddSource(
                    "ModuleInitializerAttribute.g.cs",
                    RegistrationEmitter.ModuleInitializerAttributePolyfill);
            }
        });
    }

    static AotRootModel? AnalyzeRootAttribute(GeneratorSyntaxContext context)
    {
        var attribute = (AttributeSyntax)context.Node;
        var references = ReferenceSymbols.Create(context.SemanticModel.Compilation);
        if (references?.YamlSerializableAttribute is null)
        {
            return null;
        }
        if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol constructor ||
            !SymbolEqualityComparer.Default.Equals(constructor.ContainingType, references.YamlSerializableAttribute))
        {
            return null;
        }
        if (attribute.ArgumentList is not { Arguments.Count: 1 } argumentList ||
            argumentList.Arguments[0].Expression is not TypeOfExpressionSyntax typeOf)
        {
            return null;
        }
        var type = context.SemanticModel.GetTypeInfo(typeOf.Type).Type;
        if (type is null || type.TypeKind == TypeKind.Error)
        {
            return null; // unresolvable typeof already carries a compiler error
        }
        return RegistrationHarvester.AnalyzeRoot(
            type, references, context.SemanticModel.Compilation.Assembly, attribute.GetLocation());
    }
}

// Value-equatable emit output. Equality lets Roslyn skip RegisterSourceOutput (the AddSource call)
// for types whose generated output is unchanged.
sealed record EmitResult
{
    public string HintName { get; }
    public string? Source { get; }
    public EquatableArray<DiagnosticInfo> Diagnostics { get; }

    public EmitResult(string hintName, string? source, EquatableArray<DiagnosticInfo> diagnostics)
    {
        HintName = hintName;
        Source = source;
        Diagnostics = diagnostics;
    }
}
