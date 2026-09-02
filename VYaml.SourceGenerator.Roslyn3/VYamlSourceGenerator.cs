using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace VYaml.SourceGenerator;

// Non-incremental (Roslyn 3.x / Unity) generator. Shares the analysis (TypeMetaAnalyzer) and emit
// (Emitter) with the Roslyn4 incremental generator, so both produce identical output.
[Generator(LanguageNames.CSharp)]
public class VYamlSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            var references = ReferenceSymbols.Create(context.Compilation);
            if (references is null) return;

            if (context.SyntaxContextReceiver! is not SyntaxContextReceiver syntaxCollector) return;

            var registrationStatements = new SortedSet<string>(StringComparer.Ordinal);

            foreach (var workItem in syntaxCollector.GetWorkItems())
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var model = workItem.Analyze(in context, references);
                if (model is null) continue;

                foreach (var diagnostic in model.Diagnostics)
                {
                    context.ReportDiagnostic(diagnostic.ToDiagnostic());
                }

                if (model.IsValid)
                {
                    context.AddSource(model.HintName, Emitter.Emit(model));
                    foreach (var statement in model.AotRegistrations)
                    {
                        registrationStatements.Add(statement);
                    }
                }
            }

            // [assembly: YamlSerializable(typeof(...))] roots
            if (references.YamlSerializableAttribute is { } serializableAttribute)
            {
                foreach (var attribute in context.Compilation.Assembly.GetAttributes())
                {
                    if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, serializableAttribute)) continue;
                    if (attribute.ConstructorArguments.Length != 1) continue;
                    if (attribute.ConstructorArguments[0].Value is not ITypeSymbol type) continue;
                    if (type.TypeKind == TypeKind.Error) continue;

                    var location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation();
                    var root = RegistrationHarvester.AnalyzeRoot(type, references, context.Compilation.Assembly, location);
                    foreach (var diagnostic in root.Diagnostics)
                    {
                        context.ReportDiagnostic(diagnostic.ToDiagnostic());
                    }
                    foreach (var statement in root.Statements)
                    {
                        registrationStatements.Add(statement);
                    }
                }
            }

            // per-assembly module-initializer registration (the Native AOT / IL2CPP-safe
            // registration path); module initializers need C# 9
            var langOk = context.ParseOptions is CSharpParseOptions parseOptions &&
                         parseOptions.LanguageVersion >= LanguageVersion.CSharp9;
            if (langOk && registrationStatements.Count > 0)
            {
                context.AddSource(
                    "VYamlGeneratedRegistration.g.cs",
                    RegistrationEmitter.Emit(context.Compilation.AssemblyName ?? "Assembly", registrationStatements));

                if (context.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ModuleInitializerAttribute") is null)
                {
                    context.AddSource(
                        "ModuleInitializerAttribute.g.cs",
                        RegistrationEmitter.ModuleInitializerAttributePolyfill);
                }
            }
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.UnexpectedErrorDescriptor,
                Location.None,
                ex.ToString().Replace(Environment.NewLine, " ")));
        }
    }
}
