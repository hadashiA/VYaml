using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VYaml.SourceGenerator;

// Single source of truth for analysis, shared by both the Roslyn4 (incremental) and Roslyn3
// (ISourceGenerator) generators. Resolves symbols and projects everything into the value-equatable
// TypeMetaModel. There is intentionally no symbol-bearing parallel data type: the only symbol work
// lives here, in local scratch, and never escapes into the model.
static class TypeMetaAnalyzer
{
    public static TypeMetaModel Analyze(
        INamedTypeSymbol symbol,
        TypeDeclarationSyntax syntax,
        AttributeData yamlObjectAttribute,
        ReferenceSymbols references)
    {
        var typeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var fullTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var namingConvention = NamingConvention.LowerCamelCase;
        foreach (var arg in yamlObjectAttribute.ConstructorArguments)
        {
            if (arg is { Kind: TypedConstantKind.Enum, Value: not null })
            {
                namingConvention = (NamingConvention)arg.Value;
                break;
            }
        }

        var diagnostics = new List<DiagnosticInfo>();
        var unionsValid = TryGetUnions(symbol, syntax, typeName, references, diagnostics,
            out var unions, out var unionDiscriminatorKeyName, out var isUnionByKey);
        var isUnion = unions.Length > 0;

        // invalid [YamlObjectUnion] entries are already reported; avoid cascading "abstract must be union" errors
        var valid = Validate(symbol, syntax, typeName, unions, isUnion || !unionsValid, diagnostics) && unionsValid;
        if (unionsValid && unionDiscriminatorKeyName != null)
        {
            valid &= ValidateUnionDiscriminator(syntax, typeName, unions, unionDiscriminatorKeyName, references, diagnostics);
        }
        if (unionsValid && isUnionByKey)
        {
            valid &= ResolveUnionKeys(syntax, typeName, unions, references, diagnostics);
        }

        var allMembers = SelectMembers(symbol, references, namingConvention);

        var constructorParameterNames = EquatableArray<string>.Empty;
        var setterMemberNames = EquatableArray<string>.Empty;
        var hasConstructor = false;

        if (valid && !isUnion)
        {
            if (TryGetConstructor(symbol, syntax, typeName, allMembers, references, diagnostics,
                    out var hasCtor, out var constructedMembers))
            {
                hasConstructor = hasCtor;
                constructorParameterNames = constructedMembers.Select(x => x.Name).ToEquatableArray();

                var setterMembers = allMembers
                    .Where(x => constructedMembers.All(c => !SymbolEqualityComparer.Default.Equals(x.Symbol, c.Symbol)))
                    .ToArray();

                foreach (var setterMember in setterMembers)
                {
                    switch (setterMember)
                    {
                        case { IsProperty: true, IsSettable: false }:
                            diagnostics.Add(DiagnosticInfo.Create(
                                DiagnosticDescriptors.YamlMemberPropertyMustHaveSetter,
                                setterMember.GetLocation(syntax), typeName, setterMember.Name));
                            valid = false;
                            break;
                        case { IsField: true, IsSettable: false }:
                            diagnostics.Add(DiagnosticInfo.Create(
                                DiagnosticDescriptors.YamlMemberFieldCannotBeReadonly,
                                setterMember.GetLocation(syntax), typeName, setterMember.Name));
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

        var paramDocs = DocumentationComment.GetParams(symbol);
        var members = allMembers.Select(x => x.ToModel(paramDocs)).ToEquatableArray();
        var unionModels = unions
            .Select(x => new UnionMetaModel(x.SubTypeTag, x.FullTypeName, x.KeyName, x.KeyNamingConvention))
            .ToEquatableArray();

        var ns = symbol.ContainingNamespace;
        var hasBaseYamlObject = symbol.BaseType != null &&
                                symbol.BaseType.GetAttributes().Any(a =>
                                    a.AttributeClass != null &&
                                    a.AttributeClass.ToDisplayString() == "VYaml.Annotations.YamlObjectAttribute");

        var fullType = fullTypeName
            .Replace("global::", "")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace(",", "_")
            .Replace(" ", "");

        // Module-initializer registrations: this type's own registration (only closed types
        // can self-register; closures of a generic [YamlObject] are harvested at their usage
        // sites instead) plus statically constructed formatters for the closed generic shapes
        // reachable from the member graph — what Native AOT cannot create at runtime.
        var aotRegistrations = new List<string>();
        if (valid)
        {
            var currentAssembly = symbol.ContainingAssembly;
            if (!symbol.IsGenericType)
            {
                if (symbol.TypeKind == TypeKind.Interface)
                {
                    aotRegistrations.Add(
                        $"global::VYaml.Serialization.GeneratedResolver.Register(new {fullTypeName}.{Sanitize(typeName)}GeneratedFormatter());");
                }
                else
                {
                    aotRegistrations.Add($"{fullTypeName}.__RegisterVYamlFormatter();");
                }
            }

            var visited = new HashSet<string>();
            foreach (var member in allMembers)
            {
                RegistrationHarvester.Harvest(member.MemberType, references, currentAssembly, visited, aotRegistrations);
            }
            foreach (var union in unions)
            {
                RegistrationHarvester.Harvest(union.SubTypeSymbol, references, currentAssembly, visited, aotRegistrations);
            }
        }

        return new TypeMetaModel(
            hintName: $"{fullType}.YamlFormatter.g.cs",
            isValid: valid,
            diagnostics: diagnostics.ToEquatableArray(),
            typeName: typeName,
            sanitizedTypeName: Sanitize(typeName),
            fullTypeName: fullTypeName,
            @namespace: ns.IsGlobalNamespace ? "" : ns.ToDisplayString(),
            hasNamespace: !ns.IsGlobalNamespace,
            isValueType: symbol.IsValueType,
            isInterface: symbol.TypeKind == TypeKind.Interface,
            hasBaseYamlObject: hasBaseYamlObject,
            typeDeclarationKeyword: GetTypeDeclarationKeyword(symbol, isUnion),
            isUnion: isUnion,
            unionDiscriminatorKeyName: unionDiscriminatorKeyName,
            isUnionByKey: isUnionByKey,
            namingConventionByType: namingConvention,
            hasConstructor: hasConstructor,
            description: DocumentationComment.GetSummary(symbol),
            members: members,
            constructorParameterNames: constructorParameterNames,
            setterMemberNames: setterMemberNames,
            unions: unionModels,
            aotRegistrations: aotRegistrations.ToEquatableArray());
    }

    internal static string Sanitize(string typeName) =>
        typeName.Replace("<", "_").Replace(">", "_").Replace(",", "_").Replace(" ", "");

    static string GetTypeDeclarationKeyword(INamedTypeSymbol symbol, bool isUnion)
    {
        if (isUnion)
        {
            return symbol.IsRecord
                ? "record"
                : symbol.TypeKind == TypeKind.Interface ? "interface" : "class";
        }
        return (symbol.IsRecord, symbol.IsValueType) switch
        {
            (true, true) => "record struct",
            (true, false) => "record",
            (false, true) => "struct",
            (false, false) => "class",
        };
    }

    static bool Validate(
        INamedTypeSymbol symbol,
        TypeDeclarationSyntax syntax,
        string typeName,
        IReadOnlyList<UnionAnalysis> unions,
        bool isUnion,
        List<DiagnosticInfo> diagnostics)
    {
        var error = false;
        var location = syntax.Identifier.GetLocation();

        if (!syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.MustBePartial, location, symbol.Name));
            error = true;
        }

        if (syntax.Parent is TypeDeclarationSyntax)
        {
            diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.NestedNotAllow, location, symbol.Name));
            error = true;
        }

        if (symbol.IsAbstract && !isUnion)
        {
            diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.AbstractMustUnion, location, typeName));
            error = true;
        }

        if (isUnion)
        {
            if (!symbol.IsAbstract)
            {
                diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.ConcreteTypeCantBeUnion, location, typeName));
                error = true;
            }

            foreach (var tagGroup in unions.GroupBy(x => x.SubTypeTag))
            {
                if (tagGroup.Count() > 1)
                {
                    diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.UnionTagDuplicate, location, tagGroup.Key));
                    error = true;
                }
            }

            if (symbol.TypeKind == TypeKind.Interface)
            {
                foreach (var union in unions)
                {
                    var check = union.SubTypeSymbol.IsGenericType
                        ? union.SubTypeSymbol.OriginalDefinition.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(symbol))
                        : union.SubTypeSymbol.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, symbol));
                    if (!check)
                    {
                        diagnostics.Add(DiagnosticInfo.Create(
                            DiagnosticDescriptors.UnionMemberTypeNotImplementBaseType,
                            location, typeName, union.SubTypeSymbol.Name));
                        error = true;
                    }
                }
            }
            else
            {
                foreach (var union in unions)
                {
                    var check = union.SubTypeSymbol.IsGenericType
                        ? union.SubTypeSymbol.OriginalDefinition.GetAllBaseTypes().Any(x => x.EqualsUnconstructedGenericType(symbol))
                        : union.SubTypeSymbol.GetAllBaseTypes().Any(x => SymbolEqualityComparer.Default.Equals(x, symbol));
                    if (!check)
                    {
                        diagnostics.Add(DiagnosticInfo.Create(
                            DiagnosticDescriptors.UnionMemberTypeNotDerivedBaseType,
                            location, typeName, union.SubTypeSymbol.Name));
                        error = true;
                    }
                }
            }
        }

        return !error;
    }

    // [YamlObjectUnion] entries, and how they identify the subtype: a YAML tag (default), DiscriminatorKey, or RequiredKey.
    // All entries of a type must use the same way.
    static bool TryGetUnions(
        INamedTypeSymbol symbol,
        TypeDeclarationSyntax syntax,
        string typeName,
        ReferenceSymbols references,
        List<DiagnosticInfo> diagnostics,
        out UnionAnalysis[] unions,
        out string? discriminatorKeyName,
        out bool isUnionByKey)
    {
        var location = syntax.Identifier.GetLocation();
        var valid = true;
        var result = new List<UnionAnalysis>();
        var kinds = new HashSet<(string? DiscriminatorKey, bool ByKey)>();

        foreach (var attribute in symbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, references.YamlObjectUnionAttribute)) continue;

            var arguments = attribute.ConstructorArguments;
            var tag = arguments.Length == 2 ? arguments[0].Value as string : null;
            if (arguments.Length is < 1 or > 2 || arguments[arguments.Length - 1].Value is not INamedTypeSymbol subType)
            {
                continue; // invalid attribute usage already carries a compiler error
            }

            string? discriminatorKey = null;
            string? requiredKey = null;
            foreach (var named in attribute.NamedArguments)
            {
                switch (named.Key)
                {
                    case "DiscriminatorKey":
                        discriminatorKey = named.Value.Value as string;
                        break;
                    case "RequiredKey":
                        requiredKey = named.Value.Value as string;
                        break;
                }
            }

            if ((requiredKey == null) == (tag == null) || (requiredKey != null && discriminatorKey != null))
            {
                diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.UnionTagOrRequiredKey, location, typeName,
                    subType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                valid = false;
                continue;
            }

            kinds.Add((discriminatorKey, requiredKey != null));
            result.Add(new UnionAnalysis(requiredKey ?? tag!, subType));
        }

        if (kinds.Count > 1)
        {
            diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.UnionKindInconsistent, location, typeName));
            valid = false;
        }

        unions = result.ToArray();
        var kind = kinds.FirstOrDefault();
        discriminatorKeyName = valid ? kind.DiscriminatorKey : null;
        isUnionByKey = valid && kind.ByKey;
        return valid;
    }

    static bool ValidateUnionDiscriminator(
        TypeDeclarationSyntax syntax,
        string typeName,
        IReadOnlyList<UnionAnalysis> unions,
        string discriminatorKeyName,
        ReferenceSymbols references,
        List<DiagnosticInfo> diagnostics)
    {
        var location = syntax.Identifier.GetLocation();
        var valid = true;
        foreach (var union in unions)
        {
            if (!TryGetSubTypeMembers(union, references, location, typeName, diagnostics, out _, out var subTypeMembers))
            {
                valid = false;
                continue;
            }
            foreach (var member in subTypeMembers)
            {
                if (member.KeyName == discriminatorKeyName)
                {
                    diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.UnionDiscriminatorConflictsWithMember,
                        location, discriminatorKeyName, typeName, member.Name, union.SubTypeName));
                    valid = false;
                }
            }
        }
        return valid;
    }

    // RequiredKey: resolves the key to the subtype's member, so that it follows the member's naming rules
    static bool ResolveUnionKeys(
        TypeDeclarationSyntax syntax,
        string typeName,
        IReadOnlyList<UnionAnalysis> unions,
        ReferenceSymbols references,
        List<DiagnosticInfo> diagnostics)
    {
        var location = syntax.Identifier.GetLocation();
        var valid = true;
        foreach (var union in unions)
        {
            if (!TryGetSubTypeMembers(union, references, location, typeName, diagnostics, out var namingConvention, out var subTypeMembers))
            {
                valid = false;
                continue;
            }

            var member = subTypeMembers.FirstOrDefault(x => x.KeyName == union.SubTypeTag) ??
                         subTypeMembers.FirstOrDefault(x => x.Name == union.SubTypeTag);
            if (member == null)
            {
                diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.UnionRequiredKeyNotFoundInSubType,
                    location, typeName, union.SubTypeTag, union.SubTypeName));
                valid = false;
                continue;
            }
            union.KeyName = member.KeyName;
            union.KeyNamingConvention = namingConvention;
        }
        return valid;
    }

    static bool TryGetSubTypeMembers(
        UnionAnalysis union,
        ReferenceSymbols references,
        Location location,
        string typeName,
        List<DiagnosticInfo> diagnostics,
        out NamingConvention namingConvention,
        out MemberAnalysis[] members)
    {
        var subType = union.SubTypeSymbol;
        var subTypeAttribute = subType.GetAttribute(references.YamlObjectAttribute);
        if (subTypeAttribute == null || subType.ContainsAttribute(references.YamlObjectUnionAttribute))
        {
            diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.UnionSubTypeMustBeObject, location, typeName, union.SubTypeName));
            namingConvention = default;
            members = Array.Empty<MemberAnalysis>();
            return false;
        }

        namingConvention = subTypeAttribute.ConstructorArguments.Length > 0 &&
                           subTypeAttribute.ConstructorArguments[0] is { Kind: TypedConstantKind.Enum, Value: { } value }
            ? (NamingConvention)value
            : NamingConvention.LowerCamelCase;
        members = SelectMembers(subType, references, namingConvention);
        return true;
    }

    static MemberAnalysis[] SelectMembers(INamedTypeSymbol symbol, ReferenceSymbols references, NamingConvention namingConvention)
    {
        return symbol.GetAllMembers() // iterate includes parent type
            .Where(x => x is (IFieldSymbol or IPropertySymbol) and { IsStatic: false, IsImplicitlyDeclared: false })
            .Where(x =>
            {
                if (x.ContainsAttribute(references.YamlIgnoreAttribute)) return false;

                // Allow private/internal members when explicitly marked with [YamlMember]
                if (x.DeclaredAccessibility != Accessibility.Public &&
                    !x.ContainsAttribute(references.YamlMemberAttribute))
                {
                    return false;
                }

                if (x is IPropertySymbol p)
                {
                    // set only can't be serializable member
                    if (p.GetMethod == null && p.SetMethod != null)
                    {
                        return false;
                    }
                    if (p.IsIndexer) return false;
                }
                return true;
            })
            .Select((x, i) => new MemberAnalysis(x, references, i, namingConvention))
            .OrderBy(x => x.Order)
            .ToArray();
    }

    static bool TryGetConstructor(
        INamedTypeSymbol symbol,
        TypeDeclarationSyntax syntax,
        string typeName,
        IReadOnlyList<MemberAnalysis> members,
        ReferenceSymbols references,
        List<DiagnosticInfo> diagnostics,
        out bool hasConstructor,
        out IReadOnlyList<MemberAnalysis> constructedMembers)
    {
        var constructors = symbol.InstanceConstructors
            .Where(x => !x.IsImplicitlyDeclared) // remove empty ctor(struct always generate it), record's clone ctor
            .ToArray();

        hasConstructor = false;
        if (constructors.Length <= 0)
        {
            constructedMembers = Array.Empty<MemberAnalysis>();
            return true;
        }

        IMethodSymbol selectedConstructor;
        if (constructors.Length == 1)
        {
            selectedConstructor = constructors[0];
        }
        else
        {
            var ctorWithAttrs = constructors
                .Where(x => x.ContainsAttribute(references.YamlConstructorAttribute))
                .ToArray();

            switch (ctorWithAttrs.Length)
            {
                case 1:
                    selectedConstructor = ctorWithAttrs[0];
                    break;
                case > 1:
                    diagnostics.Add(DiagnosticInfo.Create(
                        DiagnosticDescriptors.MultipleConstructorAttribute, syntax.Identifier.GetLocation(), symbol.Name));
                    constructedMembers = Array.Empty<MemberAnalysis>();
                    return false;
                default:
                    diagnostics.Add(DiagnosticInfo.Create(
                        DiagnosticDescriptors.MultipleConstructorWithoutAttribute, syntax.Identifier.GetLocation(), symbol.Name));
                    constructedMembers = Array.Empty<MemberAnalysis>();
                    return false;
            }
        }

        hasConstructor = true;
        var parameterMembers = new List<MemberAnalysis>();
        var error = false;
        foreach (var parameter in selectedConstructor.Parameters)
        {
            var matchedMember = members
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
                var location = selectedConstructor.Locations.FirstOrDefault() ?? syntax.Identifier.GetLocation();
                diagnostics.Add(DiagnosticInfo.Create(
                    DiagnosticDescriptors.ConstructorHasNoMatchedParameter, location, symbol.Name, parameter.Name));
                constructedMembers = Array.Empty<MemberAnalysis>();
                error = true;
            }
        }
        constructedMembers = parameterMembers;
        return !error;
    }

    // Mutable, analyzer-private scratch for one member. Carries symbols and the constructor flags that
    // are filled in during constructor analysis, then collapses to the immutable MemberMetaModel.
    sealed class MemberAnalysis
    {
        public ISymbol Symbol { get; }
        public string Name { get; }
        public string FullTypeName { get; }
        public ITypeSymbol MemberType { get; }
        public bool IsField { get; }
        public bool IsProperty { get; }
        public bool IsSettable { get; }
        public int Order { get; }
        public bool HasKeyNameAlias { get; }
        public bool IsRequired { get; }
        public string KeyName { get; }
        public NamingConvention NamingConventionByType { get; }

        public bool IsConstructorParameter { get; set; }
        public bool HasExplicitDefaultValueFromConstructor { get; set; }
        public object? ExplicitDefaultValueFromConstructor { get; set; }

        public MemberAnalysis(ISymbol symbol, ReferenceSymbols references, int sequentialOrder, NamingConvention namingConventionByType)
        {
            Symbol = symbol;
            Name = symbol.Name;
            Order = sequentialOrder;
            NamingConventionByType = namingConventionByType;

            // Strip leading '_' from private field names for key generation (e.g. _myField -> myField)
            var nameForKey = symbol.DeclaredAccessibility != Accessibility.Public && Name.StartsWith("_")
                ? Name.Substring(1)
                : Name;
            KeyName = NamingConventionMutator.Mutate(nameForKey, namingConventionByType);

            var memberAttribute = symbol.GetAttribute(references.YamlMemberAttribute);
            if (memberAttribute != null)
            {
                if (memberAttribute.ConstructorArguments.Length > 0 &&
                    memberAttribute.ConstructorArguments[0].Value is string aliasValue)
                {
                    HasKeyNameAlias = true;
                    KeyName = aliasValue;
                }

                var orderProp = memberAttribute.NamedArguments.FirstOrDefault(x => x.Key == "Order");
                if (orderProp is { Key: "Order", Value.Value: { } explicitOrder })
                {
                    Order = (int)explicitOrder;
                }
            }

            IsRequired = (references.YamlRequiredAttribute is { } requiredAttribute && symbol.ContainsAttribute(requiredAttribute)) ||
                         HasRequiredModifier(symbol);

            if (symbol is IFieldSymbol f)
            {
                IsProperty = false;
                IsField = true;
                IsSettable = !f.IsReadOnly; // readonly field can not set.
                MemberType = f.Type;
            }
            else if (symbol is IPropertySymbol p)
            {
                IsProperty = true;
                IsField = false;
                IsSettable = !p.IsReadOnly;
                MemberType = p.Type;
            }
            else
            {
                throw new InvalidOperationException("member is not field or property.");
            }
            FullTypeName = MemberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        public Location GetLocation(TypeDeclarationSyntax fallback)
        {
            return Symbol.Locations.FirstOrDefault() ?? fallback.Identifier.GetLocation();
        }

        public MemberMetaModel ToModel(IReadOnlyDictionary<string, string> paramDocs)
        {
            var namedType = MemberType as INamedTypeSymbol;
            var isNullableValueType = namedType is { IsGenericType: true } &&
                                      namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;

            return new MemberMetaModel(
                name: Name,
                keyName: KeyName,
                fullTypeName: FullTypeName,
                hasKeyNameAlias: HasKeyNameAlias,
                namingConventionByType: NamingConventionByType,
                isReferenceType: MemberType.IsReferenceType,
                isNullableValueType: isNullableValueType,
                isValueType: MemberType.IsValueType,
                defaultValueComparison: GetDefaultValueComparison(MemberType, Name),
                defaultValueExpression: EmitDefaultValue(),
                isRequired: IsRequired,
                typeOfExpression: MemberType.TypeKind == TypeKind.Dynamic ? "object" : FullTypeName,
                isNullable: isNullableValueType ||
                            MemberType is { IsReferenceType: true, NullableAnnotation: not NullableAnnotation.NotAnnotated },
                schemaDefaultValueExpression: EmitSchemaDefaultValue(),
                description: GetDescription(paramDocs));
        }

        string? GetDescription(IReadOnlyDictionary<string, string> paramDocs)
        {
            string? description = null;
            for (var s = Symbol; s != null && description == null; s = (s as IPropertySymbol)?.OverriddenProperty)
            {
                description = DocumentationComment.GetSummary(s);
            }
            if (description == null && paramDocs.TryGetValue(Name, out var paramDoc))
            {
                description = paramDoc; // positional record parameter
            }
            return description;
        }

        // A C# expression for the constructor parameter's default value, boxed into YamlMemberSchemaInfo.
        // null when there is no default or it cannot be represented in a schema.
        string? EmitSchemaDefaultValue()
        {
            if (!HasExplicitDefaultValueFromConstructor) return null;

            // null (including `default` of a struct) is omitted: it is not a useful default to show
            var value = ExplicitDefaultValueFromConstructor;
            if (value is null) return null;

            var type = MemberType is INamedTypeSymbol { ConstructedFrom.SpecialType: SpecialType.System_Nullable_T } nullable
                ? nullable.TypeArguments[0]
                : MemberType;
            var invariant = System.Globalization.CultureInfo.InvariantCulture;

            if (type.TypeKind == TypeKind.Enum)
            {
                var enumTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return $"({enumTypeName})({Convert.ToString(value, invariant)})";
            }

            switch (type.SpecialType)
            {
                case SpecialType.System_String:
                    return SymbolDisplay.FormatLiteral((string)value, quote: true);
                case SpecialType.System_Char:
                    // char is written as its code point (see CharFormatter)
                    return $"{(long)Convert.ToChar(value, invariant)}L";
                case SpecialType.System_Boolean:
                    return (bool)value ? "true" : "false";
                case SpecialType.System_Single:
                    var f = Convert.ToSingle(value, invariant);
                    return float.IsNaN(f) || float.IsInfinity(f) ? null : $"{f.ToString("R", invariant)}f";
                case SpecialType.System_Double:
                    var d = Convert.ToDouble(value, invariant);
                    return double.IsNaN(d) || double.IsInfinity(d) ? null : $"{d.ToString("R", invariant)}d";
                case SpecialType.System_Decimal:
                    return $"{Convert.ToDecimal(value, invariant).ToString(invariant)}m";
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                    return $"{Convert.ToInt64(value, invariant).ToString(invariant)}L";
                case SpecialType.System_Byte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                    return $"{Convert.ToUInt64(value, invariant).ToString(invariant)}UL";
                default:
                    return null;
            }
        }

        // IFieldSymbol/IPropertySymbol.IsRequired (C# 11) is only available since Roslyn 4.4, while this generator
        // is built against Roslyn 4.1. The host compiler that runs it may be newer, so read it via reflection.
        // (typeof(IPropertySymbol) is the host's Microsoft.CodeAnalysis at runtime, so it has the member if supported)
        static readonly System.Reflection.PropertyInfo? PropertyIsRequired = typeof(IPropertySymbol).GetProperty("IsRequired", typeof(bool));
        static readonly System.Reflection.PropertyInfo? FieldIsRequired = typeof(IFieldSymbol).GetProperty("IsRequired", typeof(bool));

        static bool HasRequiredModifier(ISymbol symbol)
        {
            var isRequired = symbol switch
            {
                IPropertySymbol => PropertyIsRequired,
                IFieldSymbol => FieldIsRequired,
                _ => null,
            };
            return isRequired != null && (bool)isRequired.GetValue(symbol)!;
        }

        string EmitDefaultValue()
        {
            if (!HasExplicitDefaultValueFromConstructor)
            {
                return (MemberType is { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated or NullableAnnotation.None })
                    ? $"default({FullTypeName})!"
                    : $"default({FullTypeName})";
            }

            if (ExplicitDefaultValueFromConstructor is null)
            {
                return $"default({FullTypeName})";
            }

            // Use MemberType.SpecialType instead of runtime type pattern matching,
            // because Roslyn may box numeric default values as int regardless of the parameter type.
            return MemberType.SpecialType switch
            {
                SpecialType.System_String => SymbolDisplay.FormatLiteral((string)ExplicitDefaultValueFromConstructor, quote: true),
                SpecialType.System_Single => $"{ExplicitDefaultValueFromConstructor}f",
                SpecialType.System_Double => $"{ExplicitDefaultValueFromConstructor}d",
                SpecialType.System_Decimal => $"{ExplicitDefaultValueFromConstructor}m",
                SpecialType.System_Boolean => (bool)ExplicitDefaultValueFromConstructor ? "true" : "false",
                SpecialType.System_Int32 => $"{ExplicitDefaultValueFromConstructor}",
                SpecialType.System_Int64 => $"{ExplicitDefaultValueFromConstructor}L",
                SpecialType.System_UInt32 => $"{ExplicitDefaultValueFromConstructor}u",
                SpecialType.System_UInt64 => $"{ExplicitDefaultValueFromConstructor}ul",
                SpecialType.System_Int16 => $"(short){ExplicitDefaultValueFromConstructor}",
                SpecialType.System_UInt16 => $"(ushort){ExplicitDefaultValueFromConstructor}",
                SpecialType.System_Byte => $"(byte){ExplicitDefaultValueFromConstructor}",
                SpecialType.System_SByte => $"(sbyte){ExplicitDefaultValueFromConstructor}",
                SpecialType.System_Char => SymbolDisplay.FormatLiteral(Convert.ToChar(ExplicitDefaultValueFromConstructor), quote: true),
                _ when MemberType.TypeKind == TypeKind.Enum => $"({FullTypeName}){ExplicitDefaultValueFromConstructor}",
                _ => ExplicitDefaultValueFromConstructor.ToString()
            };
        }

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
    }

    // Analyzer-private scratch for one [YamlObjectUnion] entry.
    sealed class UnionAnalysis
    {
        public string SubTypeTag { get; }
        public INamedTypeSymbol SubTypeSymbol { get; }
        public string FullTypeName { get; }
        public string SubTypeName => SubTypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        // RequiredKey: resolved by ResolveUnionKeys
        public string KeyName { get; set; } = "";
        public NamingConvention KeyNamingConvention { get; set; }

        public UnionAnalysis(string subTypeTag, INamedTypeSymbol subTypeSymbol)
        {
            SubTypeTag = subTypeTag;
            SubTypeSymbol = subTypeSymbol;
            FullTypeName = subTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
    }
}
