using System;
using System.Linq;

namespace VYaml.SourceGenerator;

public enum NamingConvention
{
    LowerCamelCase,
    UpperCamelCase,
    SnakeCase,
    KebabCase,
}

// mirrors VYaml.Annotations.YamlUnionStyle
public enum UnionStyle
{
    Tag,
    DiscriminatorKey,
    Wrapped,
}

// Value-equatable projection of one [YamlObject] type, produced by TypeMetaAnalyzer and consumed by
// Emitter. This is the incremental cache key: it carries only primitives / strings / EquatableArrays,
// so an edit that does not change a type's relevant shape produces an equal model, letting Roslyn skip
// both the emit and the source-output stage downstream.
sealed record TypeMetaModel
{
    public string HintName { get; }
    public bool IsValid { get; }
    public EquatableArray<DiagnosticInfo> Diagnostics { get; }

    public string TypeName { get; }
    public string SanitizedTypeName { get; }
    public string FullTypeName { get; }
    public string Namespace { get; }
    public bool HasNamespace { get; }
    public bool IsValueType { get; }
    public bool IsInterface { get; }
    public bool HasBaseYamlObject { get; }
    public string TypeDeclarationKeyword { get; }
    public bool IsUnion { get; }
    // DiscriminatorKey of [YamlObjectUnion]; null for tag-based unions
    public string? UnionDiscriminatorKeyName { get; }
    public UnionStyle UnionStyle { get; }
    public NamingConvention NamingConventionByType { get; }
    public bool HasConstructor { get; }
    // <summary> of the XML doc comment, for YamlSchemaGenerator
    public string? Description { get; }

    public EquatableArray<MemberMetaModel> Members { get; }
    public EquatableArray<string> ConstructorParameterNames { get; }
    public EquatableArray<string> SetterMemberNames { get; }
    public EquatableArray<UnionMetaModel> Unions { get; }

    // Statements for the per-assembly module-initializer registration file: this type's own
    // registration plus statically constructed closed generic formatters (enums, collections,
    // closed user generics) harvested from the member graph. See RegistrationEmitter.
    public EquatableArray<string> AotRegistrations { get; }

    public TypeMetaModel(
        string hintName,
        bool isValid,
        EquatableArray<DiagnosticInfo> diagnostics,
        string typeName,
        string sanitizedTypeName,
        string fullTypeName,
        string @namespace,
        bool hasNamespace,
        bool isValueType,
        bool isInterface,
        bool hasBaseYamlObject,
        string typeDeclarationKeyword,
        bool isUnion,
        string? unionDiscriminatorKeyName,
        UnionStyle unionStyle,
        NamingConvention namingConventionByType,
        bool hasConstructor,
        string? description,
        EquatableArray<MemberMetaModel> members,
        EquatableArray<string> constructorParameterNames,
        EquatableArray<string> setterMemberNames,
        EquatableArray<UnionMetaModel> unions,
        EquatableArray<string> aotRegistrations)
    {
        HintName = hintName;
        IsValid = isValid;
        Diagnostics = diagnostics;
        TypeName = typeName;
        SanitizedTypeName = sanitizedTypeName;
        FullTypeName = fullTypeName;
        Namespace = @namespace;
        HasNamespace = hasNamespace;
        IsValueType = isValueType;
        IsInterface = isInterface;
        HasBaseYamlObject = hasBaseYamlObject;
        TypeDeclarationKeyword = typeDeclarationKeyword;
        IsUnion = isUnion;
        UnionDiscriminatorKeyName = unionDiscriminatorKeyName;
        UnionStyle = unionStyle;
        NamingConventionByType = namingConventionByType;
        HasConstructor = hasConstructor;
        Description = description;
        Members = members;
        ConstructorParameterNames = constructorParameterNames;
        SetterMemberNames = setterMemberNames;
        Unions = unions;
        AotRegistrations = aotRegistrations;
    }
}

sealed record MemberMetaModel
{
    public string Name { get; }
    public string KeyName { get; }
    public string FullTypeName { get; }
    public bool HasKeyNameAlias { get; }
    public NamingConvention NamingConventionByType { get; }
    public bool IsReferenceType { get; }
    public bool IsNullableValueType { get; }
    public bool IsValueType { get; }
    public string DefaultValueComparison { get; }
    public string DefaultValueExpression { get; }
    // [YamlRequired] or the C# `required` modifier
    public bool IsRequired { get; }

    // For YamlSchemaGenerator
    public string TypeOfExpression { get; }
    public bool IsNullable { get; }
    public string? SchemaDefaultValueExpression { get; }
    public string? Description { get; }

    // Computed (no backing field), so it is derived on demand and excluded from record equality,
    // which is correct because it is a pure function of KeyName.
    public byte[] KeyNameUtf8Bytes => System.Text.Encoding.UTF8.GetBytes(KeyName);

    public MemberMetaModel(
        string name,
        string keyName,
        string fullTypeName,
        bool hasKeyNameAlias,
        NamingConvention namingConventionByType,
        bool isReferenceType,
        bool isNullableValueType,
        bool isValueType,
        string defaultValueComparison,
        string defaultValueExpression,
        bool isRequired,
        string typeOfExpression,
        bool isNullable,
        string? schemaDefaultValueExpression,
        string? description)
    {
        Name = name;
        KeyName = keyName;
        FullTypeName = fullTypeName;
        HasKeyNameAlias = hasKeyNameAlias;
        NamingConventionByType = namingConventionByType;
        IsReferenceType = isReferenceType;
        IsNullableValueType = isNullableValueType;
        IsValueType = isValueType;
        DefaultValueComparison = defaultValueComparison;
        DefaultValueExpression = defaultValueExpression;
        IsRequired = isRequired;
        TypeOfExpression = typeOfExpression;
        IsNullable = isNullable;
        SchemaDefaultValueExpression = schemaDefaultValueExpression;
        Description = description;
    }
}

sealed record UnionMetaModel
{
    public string SubTypeTag { get; }
    public string FullTypeName { get; }

    public UnionMetaModel(string subTypeTag, string fullTypeName)
    {
        SubTypeTag = subTypeTag;
        FullTypeName = fullTypeName;
    }
}
