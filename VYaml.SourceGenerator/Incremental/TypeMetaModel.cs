using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

namespace VYaml.SourceGenerator;

// Value-equatable projection of one [YamlObject] type. This is the incremental cache key:
// it carries only primitives / strings / EquatableArrays extracted from symbols, so an edit
// that does not change a type's relevant shape produces an equal model, letting Roslyn skip
// BOTH the emit (.Select) and the source-output stage downstream.
sealed class TypeMetaModel : IEquatable<TypeMetaModel>
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
    public NamingConvention NamingConventionByType { get; }
    public bool HasConstructor { get; }

    public EquatableArray<MemberMetaModel> Members { get; }
    public EquatableArray<string> ConstructorParameterNames { get; }
    public EquatableArray<string> SetterMemberNames { get; }
    public EquatableArray<UnionMetaModel> Unions { get; }

    TypeMetaModel(
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
        NamingConvention namingConventionByType,
        bool hasConstructor,
        EquatableArray<MemberMetaModel> members,
        EquatableArray<string> constructorParameterNames,
        EquatableArray<string> setterMemberNames,
        EquatableArray<UnionMetaModel> unions)
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
        NamingConventionByType = namingConventionByType;
        HasConstructor = hasConstructor;
        Members = members;
        ConstructorParameterNames = constructorParameterNames;
        SetterMemberNames = setterMemberNames;
        Unions = unions;
    }

    public static TypeMetaModel? Analyze(GeneratorAttributeSyntaxContext context, System.Threading.CancellationToken cancellation)
    {
        var references = ReferenceSymbols.Create(context.SemanticModel.Compilation);
        if (references is null)
        {
            return null;
        }

        var typeMeta = new TypeMeta(
            (TypeDeclarationSyntax)context.TargetNode,
            (INamedTypeSymbol)context.TargetSymbol,
            context.Attributes.First(),
            references);

        var diagnostics = new List<DiagnosticInfo>();
        var valid = Validate(typeMeta, diagnostics);

        var constructorParameterNames = EquatableArray<string>.Empty;
        var setterMemberNames = EquatableArray<string>.Empty;
        var hasConstructor = false;

        if (valid && !typeMeta.IsUnion)
        {
            if (TryGetConstructor(typeMeta, references, diagnostics, out var selectedConstructor, out var constructedMembers))
            {
                hasConstructor = selectedConstructor != null;
                constructorParameterNames = constructedMembers.Select(x => x.Name).ToEquatableArray();

                var setterMembers = typeMeta.MemberMetas
                    .Where(x => constructedMembers.All(c => !SymbolEqualityComparer.Default.Equals(x.Symbol, c.Symbol)))
                    .ToArray();

                foreach (var setterMember in setterMembers)
                {
                    switch (setterMember)
                    {
                        case { IsProperty: true, IsSettable: false }:
                            diagnostics.Add(DiagnosticInfo.Create(
                                DiagnosticDescriptors.YamlMemberPropertyMustHaveSetter,
                                setterMember.GetLocation(typeMeta.Syntax),
                                typeMeta.TypeName,
                                setterMember.Name));
                            valid = false;
                            break;
                        case { IsField: true, IsSettable: false }:
                            diagnostics.Add(DiagnosticInfo.Create(
                                DiagnosticDescriptors.YamlMemberFieldCannotBeReadonly,
                                setterMember.GetLocation(typeMeta.Syntax),
                                typeMeta.TypeName,
                                setterMember.Name));
                            valid = false;
                            break;
                    }
                }

                setterMemberNames = setterMembers.Select(x => x.Name).ToEquatableArray();
            }
            else
            {
                valid = false;
            }
        }

        // Project members only after constructor analysis, because EmitDefaultValue() depends on
        // the explicit-default-from-constructor flags set during TryGetConstructor.
        var members = typeMeta.MemberMetas.Select(MemberMetaModel.From).ToEquatableArray();
        var unions = typeMeta.UnionMetas
            .Select(x => new UnionMetaModel(x.SubTypeTag, x.FullTypeName))
            .ToEquatableArray();

        var sanitizedTypeName = Sanitize(typeMeta.TypeName);
        var fullType = typeMeta.FullTypeName
            .Replace("global::", "")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace(",", "_")
            .Replace(" ", "");

        var ns = typeMeta.Symbol.ContainingNamespace;
        var hasBaseYamlObject = typeMeta.Symbol.BaseType != null &&
                                typeMeta.Symbol.BaseType.GetAttributes().Any(a =>
                                    a.AttributeClass != null &&
                                    a.AttributeClass.ToDisplayString() == "VYaml.Annotations.YamlObjectAttribute");

        return new TypeMetaModel(
            hintName: $"{fullType}.YamlFormatter.g.cs",
            isValid: valid,
            diagnostics: diagnostics.ToEquatableArray(),
            typeName: typeMeta.TypeName,
            sanitizedTypeName: sanitizedTypeName,
            fullTypeName: typeMeta.FullTypeName,
            @namespace: ns.IsGlobalNamespace ? "" : ns.ToDisplayString(),
            hasNamespace: !ns.IsGlobalNamespace,
            isValueType: typeMeta.Symbol.IsValueType,
            isInterface: typeMeta.Symbol.TypeKind == TypeKind.Interface,
            hasBaseYamlObject: hasBaseYamlObject,
            typeDeclarationKeyword: GetTypeDeclarationKeyword(typeMeta),
            isUnion: typeMeta.IsUnion,
            namingConventionByType: typeMeta.NamingConventionByType,
            hasConstructor: hasConstructor,
            members: members,
            constructorParameterNames: constructorParameterNames,
            setterMemberNames: setterMemberNames,
            unions: unions);
    }

    internal static string Sanitize(string typeName) =>
        typeName.Replace("<", "_").Replace(">", "_").Replace(",", "_").Replace(" ", "");

    static string GetTypeDeclarationKeyword(TypeMeta typeMeta)
    {
        if (typeMeta.IsUnion)
        {
            return typeMeta.Symbol.IsRecord
                ? "record"
                : typeMeta.Symbol.TypeKind == TypeKind.Interface ? "interface" : "class";
        }
        return (typeMeta.Symbol.IsRecord, typeMeta.Symbol.IsValueType) switch
        {
            (true, true) => "record struct",
            (true, false) => "record",
            (false, true) => "struct",
            (false, false) => "class",
        };
    }

    // Ported from the previous Emitter.TryEmit validation block (symbol-dependent checks).
    static bool Validate(TypeMeta typeMeta, List<DiagnosticInfo> diagnostics)
    {
        var error = false;

        if (!typeMeta.IsPartial())
        {
            diagnostics.Add(DiagnosticInfo.Create(
                DiagnosticDescriptors.MustBePartial, typeMeta.Syntax.Identifier.GetLocation(), typeMeta.Symbol.Name));
            error = true;
        }

        if (typeMeta.IsNested())
        {
            diagnostics.Add(DiagnosticInfo.Create(
                DiagnosticDescriptors.NestedNotAllow, typeMeta.Syntax.Identifier.GetLocation(), typeMeta.Symbol.Name));
            error = true;
        }

        if (typeMeta.Symbol.IsAbstract && !typeMeta.IsUnion)
        {
            diagnostics.Add(DiagnosticInfo.Create(
                DiagnosticDescriptors.AbstractMustUnion, typeMeta.Syntax.Identifier.GetLocation(), typeMeta.TypeName));
            error = true;
        }

        if (typeMeta.IsUnion)
        {
            if (!typeMeta.Symbol.IsAbstract)
            {
                diagnostics.Add(DiagnosticInfo.Create(
                    DiagnosticDescriptors.ConcreteTypeCantBeUnion, typeMeta.Syntax.Identifier.GetLocation(), typeMeta.TypeName));
                error = true;
            }

            foreach (var tagGroup in typeMeta.UnionMetas.GroupBy(x => x.SubTypeTag))
            {
                if (tagGroup.Count() > 1)
                {
                    diagnostics.Add(DiagnosticInfo.Create(
                        DiagnosticDescriptors.UnionTagDuplicate, typeMeta.Syntax.Identifier.GetLocation(), tagGroup.Key));
                    error = true;
                }
            }

            if (typeMeta.Symbol.TypeKind == TypeKind.Interface)
            {
                foreach (var unionMeta in typeMeta.UnionMetas)
                {
                    var check = unionMeta.SubTypeSymbol.IsGenericType
                        ? unionMeta.SubTypeSymbol.OriginalDefinition.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(typeMeta.Symbol))
                        : unionMeta.SubTypeSymbol.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, typeMeta.Symbol));
                    if (!check)
                    {
                        diagnostics.Add(DiagnosticInfo.Create(
                            DiagnosticDescriptors.UnionMemberTypeNotImplementBaseType,
                            typeMeta.Syntax.Identifier.GetLocation(), typeMeta.TypeName, unionMeta.SubTypeSymbol.Name));
                        error = true;
                    }
                }
            }
            else
            {
                foreach (var unionMeta in typeMeta.UnionMetas)
                {
                    var check = unionMeta.SubTypeSymbol.IsGenericType
                        ? unionMeta.SubTypeSymbol.OriginalDefinition.GetAllBaseTypes().Any(x => x.EqualsUnconstructedGenericType(typeMeta.Symbol))
                        : unionMeta.SubTypeSymbol.GetAllBaseTypes().Any(x => SymbolEqualityComparer.Default.Equals(x, typeMeta.Symbol));
                    if (!check)
                    {
                        diagnostics.Add(DiagnosticInfo.Create(
                            DiagnosticDescriptors.UnionMemberTypeNotDerivedBaseType,
                            typeMeta.Syntax.Identifier.GetLocation(), typeMeta.TypeName, unionMeta.SubTypeSymbol.Name));
                        error = true;
                    }
                }
            }
        }

        return !error;
    }

    // Ported from the previous Emitter.TryGetConstructor.
    static bool TryGetConstructor(
        TypeMeta typeMeta,
        ReferenceSymbols reference,
        List<DiagnosticInfo> diagnostics,
        out IMethodSymbol? selectedConstructor,
        out IReadOnlyList<MemberMeta> constructedMembers)
    {
        if (typeMeta.Constructors.Count <= 0)
        {
            selectedConstructor = null;
            constructedMembers = Array.Empty<MemberMeta>();
            return true;
        }

        if (typeMeta.Constructors.Count == 1)
        {
            selectedConstructor = typeMeta.Constructors[0];
        }
        else
        {
            var ctorWithAttrs = typeMeta.Constructors
                .Where(x => x.ContainsAttribute(reference.YamlConstructorAttribute))
                .ToArray();

            switch (ctorWithAttrs.Length)
            {
                case 1:
                    selectedConstructor = ctorWithAttrs[0];
                    break;
                case > 1:
                    diagnostics.Add(DiagnosticInfo.Create(
                        DiagnosticDescriptors.MultipleConstructorAttribute,
                        typeMeta.Syntax.Identifier.GetLocation(), typeMeta.Symbol.Name));
                    selectedConstructor = null;
                    constructedMembers = Array.Empty<MemberMeta>();
                    return false;
                default:
                    diagnostics.Add(DiagnosticInfo.Create(
                        DiagnosticDescriptors.MultipleConstructorWithoutAttribute,
                        typeMeta.Syntax.Identifier.GetLocation(), typeMeta.Symbol.Name));
                    selectedConstructor = null;
                    constructedMembers = Array.Empty<MemberMeta>();
                    return false;
            }
        }

        var parameterMembers = new List<MemberMeta>();
        var error = false;
        foreach (var parameter in selectedConstructor.Parameters)
        {
            var matchedMember = typeMeta.MemberMetas
                .FirstOrDefault(member =>
                    parameter.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase) ||
                    parameter.Name.Equals(member.KeyName, StringComparison.OrdinalIgnoreCase));
            if (matchedMember != null)
            {
                matchedMember.IsConstructorParameter = true;
                if (parameter.HasExplicitDefaultValue)
                {
                    matchedMember.HasExplicitDefaultValueFromConstructor = true;
                    matchedMember.ExplicitDefaultValueFromConstructor = parameter.ExplicitDefaultValue;
                }
                parameterMembers.Add(matchedMember);
            }
            else
            {
                var location = selectedConstructor.Locations.FirstOrDefault() ??
                               typeMeta.Syntax.Identifier.GetLocation();
                diagnostics.Add(DiagnosticInfo.Create(
                    DiagnosticDescriptors.ConstructorHasNoMatchedParameter,
                    location, typeMeta.Symbol.Name, parameter.Name));
                constructedMembers = Array.Empty<MemberMeta>();
                error = true;
            }
        }
        constructedMembers = parameterMembers;
        return !error;
    }

    public bool Equals(TypeMetaModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return HintName == other.HintName &&
               IsValid == other.IsValid &&
               TypeName == other.TypeName &&
               SanitizedTypeName == other.SanitizedTypeName &&
               FullTypeName == other.FullTypeName &&
               Namespace == other.Namespace &&
               HasNamespace == other.HasNamespace &&
               IsValueType == other.IsValueType &&
               IsInterface == other.IsInterface &&
               HasBaseYamlObject == other.HasBaseYamlObject &&
               TypeDeclarationKeyword == other.TypeDeclarationKeyword &&
               IsUnion == other.IsUnion &&
               NamingConventionByType == other.NamingConventionByType &&
               HasConstructor == other.HasConstructor &&
               Members.Equals(other.Members) &&
               ConstructorParameterNames.Equals(other.ConstructorParameterNames) &&
               SetterMemberNames.Equals(other.SetterMemberNames) &&
               Unions.Equals(other.Unions) &&
               Diagnostics.Equals(other.Diagnostics);
    }

    public override bool Equals(object? obj) => obj is TypeMetaModel other && Equals(other);

    public override int GetHashCode()
    {
        var hash = HintName.GetHashCode();
        hash = unchecked(hash * 397) ^ FullTypeName.GetHashCode();
        hash = unchecked(hash * 397) ^ IsUnion.GetHashCode();
        hash = unchecked(hash * 397) ^ NamingConventionByType.GetHashCode();
        hash = unchecked(hash * 397) ^ Members.GetHashCode();
        hash = unchecked(hash * 397) ^ Unions.GetHashCode();
        hash = unchecked(hash * 397) ^ Diagnostics.GetHashCode();
        return hash;
    }
}

sealed class MemberMetaModel : IEquatable<MemberMetaModel>
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

    // Derived purely from KeyName (its UTF8 encoding); excluded from equality/hash.
    public byte[] KeyNameUtf8Bytes { get; }

    MemberMetaModel(
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
        byte[] keyNameUtf8Bytes)
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
        KeyNameUtf8Bytes = keyNameUtf8Bytes;
    }

    public static MemberMetaModel From(MemberMeta member)
    {
        var namedType = member.MemberType as INamedTypeSymbol;
        var isNullableValueType = namedType is { IsGenericType: true } &&
                                  namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;

        return new MemberMetaModel(
            name: member.Name,
            keyName: member.KeyName,
            fullTypeName: member.FullTypeName,
            hasKeyNameAlias: member.HasKeyNameAlias,
            namingConventionByType: member.NamingConventionByType,
            isReferenceType: member.MemberType.IsReferenceType,
            isNullableValueType: isNullableValueType,
            isValueType: member.MemberType.IsValueType,
            defaultValueComparison: GetDefaultValueComparison(member.MemberType, member.Name),
            defaultValueExpression: member.EmitDefaultValue(),
            keyNameUtf8Bytes: member.KeyNameUtf8Bytes);
    }

    // Ported from the previous Emitter.GetDefaultValueComparison.
    static string GetDefaultValueComparison(ITypeSymbol type, string memberName)
    {
        var typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return typeName switch
        {
            "bool" or "global::System.Boolean" => $"value.{memberName} != false",
            "byte" or "global::System.Byte" => $"value.{memberName} != 0",
            "sbyte" or "global::System.SByte" => $"value.{memberName} != 0",
            "short" or "global::System.Int16" => $"value.{memberName} != 0",
            "ushort" or "global::System.UInt16" => $"value.{memberName} != 0",
            "int" or "global::System.Int32" => $"value.{memberName} != 0",
            "uint" or "global::System.UInt32" => $"value.{memberName} != 0u",
            "long" or "global::System.Int64" => $"value.{memberName} != 0L",
            "ulong" or "global::System.UInt64" => $"value.{memberName} != 0UL",
            "float" or "global::System.Single" => $"value.{memberName} != 0f",
            "double" or "global::System.Double" => $"value.{memberName} != 0d",
            "decimal" or "global::System.Decimal" => $"value.{memberName} != 0m",
            "char" or "global::System.Char" => $"value.{memberName} != '\\0'",
            _ => $"!value.{memberName}.Equals(default({typeName}))"
        };
    }

    public bool Equals(MemberMetaModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name == other.Name &&
               KeyName == other.KeyName &&
               FullTypeName == other.FullTypeName &&
               HasKeyNameAlias == other.HasKeyNameAlias &&
               NamingConventionByType == other.NamingConventionByType &&
               IsReferenceType == other.IsReferenceType &&
               IsNullableValueType == other.IsNullableValueType &&
               IsValueType == other.IsValueType &&
               DefaultValueComparison == other.DefaultValueComparison &&
               DefaultValueExpression == other.DefaultValueExpression;
    }

    public override bool Equals(object? obj) => obj is MemberMetaModel other && Equals(other);

    public override int GetHashCode()
    {
        var hash = Name.GetHashCode();
        hash = unchecked(hash * 397) ^ KeyName.GetHashCode();
        hash = unchecked(hash * 397) ^ FullTypeName.GetHashCode();
        hash = unchecked(hash * 397) ^ HasKeyNameAlias.GetHashCode();
        hash = unchecked(hash * 397) ^ NamingConventionByType.GetHashCode();
        hash = unchecked(hash * 397) ^ DefaultValueExpression.GetHashCode();
        return hash;
    }
}

sealed class UnionMetaModel : IEquatable<UnionMetaModel>
{
    public string SubTypeTag { get; }
    public string FullTypeName { get; }

    public UnionMetaModel(string subTypeTag, string fullTypeName)
    {
        SubTypeTag = subTypeTag;
        FullTypeName = fullTypeName;
    }

    public bool Equals(UnionMetaModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return SubTypeTag == other.SubTypeTag && FullTypeName == other.FullTypeName;
    }

    public override bool Equals(object? obj) => obj is UnionMetaModel other && Equals(other);

    public override int GetHashCode() => unchecked(SubTypeTag.GetHashCode() * 397) ^ FullTypeName.GetHashCode();
}
