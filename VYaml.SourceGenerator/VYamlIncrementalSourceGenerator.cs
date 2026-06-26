using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

namespace VYaml.SourceGenerator;

static class TrackingNames
{
    public const string Model = "VYaml.Model";
    public const string Emit = "VYaml.Emit";
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
                static (context, cancellation) => TypeMetaModel.Analyze(context, cancellation))
            .Where(static x => x is not null)
            .WithTrackingName(TrackingNames.Model);

        // 2. emit: model -> source string. Cached on the model, so the string is only
        //    rebuilt when a type's relevant shape actually changes.
        var emitted = models.Select(static (model, cancellation) =>
        {
            var source = model!.IsValid ? IncrementalEmitter.Emit(model) : null;
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
    }
}

// Value-equatable emit output. Equality lets Roslyn skip RegisterSourceOutput (the AddSource call)
// for types whose generated output is unchanged.
sealed class EmitResult : IEquatable<EmitResult>
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

    public bool Equals(EmitResult? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return HintName == other.HintName &&
               Source == other.Source &&
               Diagnostics.Equals(other.Diagnostics);
    }

    public override bool Equals(object? obj) => obj is EmitResult other && Equals(other);

    public override int GetHashCode()
    {
        var hash = HintName.GetHashCode();
        hash = unchecked(hash * 397) ^ (Source?.GetHashCode() ?? 0);
        hash = unchecked(hash * 397) ^ Diagnostics.GetHashCode();
        return hash;
    }
}
