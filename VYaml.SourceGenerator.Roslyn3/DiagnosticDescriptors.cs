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

    public static readonly DiagnosticDescriptor YamlMemberPropertyMustHaveSetter = new(
        id: "VYAML004",
        title: "A yaml serializable property with must have setter",
        messageFormat: "The VYaml serializable object '{0}' property '{1}' must have setter",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor YamlMemberFieldCannotBeReadonly = new(
        id: "VYAML005",
        title: "A yaml serializable field cannot be readonly",
        messageFormat: "The VYaml serializable object '{0}' field '{1}' cannot be readonly",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AbstractMustUnion = new(
        id: "VYAML006",
        title: "abstract/interface type of `[YamlObject]` must annotate with Union",
        messageFormat: "abstract/interface type of `[YamlObject]` '{0}' must annotate with Union",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConcreteTypeCantBeUnion = new(
        id: "VYAML007",
        title: "Concrete type can't be union",
        messageFormat: "The object that has `[YamlObject]` '{0}' can be Union, only allow abstract or interface",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionTagDuplicate = new(
        id: "VYAML008",
        title: "Union tag is duplicate",
        messageFormat: "The object that has `[YamlObject]` '{0}' union tag value is duplicate",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionMemberTypeNotImplementBaseType = new(
        id: "VYAML009",
        title: "Union member not implement union interface",
        messageFormat: "The object '{0}' union member '{1}' not implment union interface",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionMemberTypeNotDerivedBaseType = new(
        id: "VYAML010",
        title: "Union member not dervided union base type",
        messageFormat: "The object '{0}' union member '{1}' not derived union type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionMemberNotAllowStruct = new(
        id: "VYAML011",
        title: "Union member can't be struct",
        messageFormat: "The object '{0}' union member '{1}' can't be member, not allows struct",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnionMemberMustBeYamlObject = new(
        id: "VYAML012",
        title: "Union member must be YamlObject",
        messageFormat: "The object '{0}' union member '{1}' must be [YamlObject]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleConstructorAttribute = new(
        id: "VYAML013",
        title: "[YamlConstructor] exists in multiple constructors",
        messageFormat: "Mupltiple [YamlConstructor] exists in '{0}' but allows only single ctor",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleConstructorWithoutAttribute = new(
        id: "VYAML014",
        title: "Require [YamlConstructor] when exists multiple constructors",
        messageFormat: "The Yaml object '{0}' must annotate with [YamlConstructor] when exists multiple constructors",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConstructorHasNoMatchedParameter = new(
        id: "VYAML0015",
        title: "VYaml's constructor has no matched parameter",
        messageFormat: "The VYaml object '{0}' constructor's parameter '{1}' must match a serialized member name(case-insensitive)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
