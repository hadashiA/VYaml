using Microsoft.CodeAnalysis;

namespace VYaml.SourceGenerator;

static class DiagnosticDescriptors
{
    const string Category = "VYaml.SourceGenerator";

    public static readonly DiagnosticDescriptor UnexpectedErrorDescriptor = new(
        id: "VYAML001",
        title: "Unexpected error during source code generation",
        messageFormat: "Unexpected error occurred during source code code generation: {0}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "VYAML002",
        title: "VYaml serializable type declaration must be partial",
        messageFormat: "The VYaml serializable type declaration '{0}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NestedNotAllow = new(
        id: "VYAML003",
        title: "VYaml serializable type must not be nested type",
        messageFormat: "The VYaml serializable object '{0}' must be not nested type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
