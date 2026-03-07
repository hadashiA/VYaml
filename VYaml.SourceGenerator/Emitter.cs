using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
#if ROSLYN3
using SourceProductionContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#endif

namespace VYaml.SourceGenerator;

static class Emitter
{
    public static bool TryEmit(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        ReferenceSymbols references,
        in SourceProductionContext context)
    {
        try
        {
            var error = false;

            // verify is partial
            if (!typeMeta.IsPartial())
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.MustBePartial,
                    typeMeta.Syntax.Identifier.GetLocation(),
                    typeMeta.Symbol.Name));
                error = true;
            }

            // nested is not allowed
            if (typeMeta.IsNested())
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.NestedNotAllow,
                    typeMeta.Syntax.Identifier.GetLocation(),
                    typeMeta.Symbol.Name));
                error = true;
            }

            // verify abstract/interface
            if (typeMeta.Symbol.IsAbstract)
            {
                if (!typeMeta.IsUnion)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.AbstractMustUnion,
                        typeMeta.Syntax.Identifier.GetLocation(),
                        typeMeta.TypeName));
                    error = true;
                }
            }

            // verify union
            if (typeMeta.IsUnion)
            {
                if (!typeMeta.Symbol.IsAbstract)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.ConcreteTypeCantBeUnion,
                        typeMeta.Syntax.Identifier.GetLocation(),
                        typeMeta.TypeName));
                    error = true;
                }

                // verify tag duplication
                foreach (var tagGroup in typeMeta.UnionMetas.GroupBy(x => x.SubTypeTag))
                {
                    if (tagGroup.Count() > 1)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.UnionTagDuplicate,
                            typeMeta.Syntax.Identifier.GetLocation(),
                            tagGroup.Key));
                        error = true;
                    }
                }

                // verify interface impl
                if (typeMeta.Symbol.TypeKind == TypeKind.Interface)
                {
                    foreach (var unionMeta in typeMeta.UnionMetas)
                    {
                        // interface, check interfaces.
                        var check = unionMeta.SubTypeSymbol.IsGenericType
                            ? unionMeta.SubTypeSymbol.OriginalDefinition.AllInterfaces.Any(x => x.EqualsUnconstructedGenericType(typeMeta.Symbol))
                            : unionMeta.SubTypeSymbol.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, typeMeta.Symbol));

                        if (!check)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptors.UnionMemberTypeNotImplementBaseType,
                                typeMeta.Syntax.Identifier.GetLocation(),
                                typeMeta.TypeName,
                                unionMeta.SubTypeSymbol.Name));
                            error = true;
                        }
                    }
                }
                // verify abstract inherit
                else
                {
                    foreach (var unionMeta in typeMeta.UnionMetas)
                    {
                        // abstract type, check base.
                        var check = unionMeta.SubTypeSymbol.IsGenericType
                            ? unionMeta.SubTypeSymbol.OriginalDefinition.GetAllBaseTypes().Any(x => x.EqualsUnconstructedGenericType(typeMeta.Symbol))
                            : unionMeta.SubTypeSymbol.GetAllBaseTypes().Any(x => SymbolEqualityComparer.Default.Equals(x, typeMeta.Symbol));

                        if (!check)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptors.UnionMemberTypeNotDerivedBaseType,
                                typeMeta.Syntax.Identifier.GetLocation(),
                                typeMeta.TypeName,
                                unionMeta.SubTypeSymbol.Name));
                            error = true;
                        }
                    }
                }
            }

            if (error)
            {
                return false;
            }

            codeWriter.AppendLine("// <auto-generated />");
            codeWriter.AppendLine("#nullable enable");
            codeWriter.AppendLine("#pragma warning disable CS0162 // Unreachable code");
            codeWriter.AppendLine("#pragma warning disable CS0219 // Variable assigned but never used");
            codeWriter.AppendLine("#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.");
            codeWriter.AppendLine("#pragma warning disable CS8601 // Possible null reference assignment");
            codeWriter.AppendLine("#pragma warning disable CS8602 // Possible null return");
            codeWriter.AppendLine("#pragma warning disable CS8604 // Possible null reference argument for parameter");
            codeWriter.AppendLine("#pragma warning disable CS8619 // Possible null reference assignment fix");
            codeWriter.AppendLine("#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method");
            codeWriter.AppendLine();
            codeWriter.AppendLine("using System;");
            codeWriter.AppendLine("using VYaml.Annotations;");
            codeWriter.AppendLine("using VYaml.Parser;");
            codeWriter.AppendLine("using VYaml.Emitter;");
            codeWriter.AppendLine("using VYaml.Serialization;");
            codeWriter.AppendLine();

            var ns = typeMeta.Symbol.ContainingNamespace;
            if (!ns.IsGlobalNamespace)
            {
                codeWriter.AppendLine($"namespace {ns}");
                codeWriter.BeginBlock();
            }

            var typeDeclarationKeyword = (typeMeta.Symbol.IsRecord, typeMeta.Symbol.IsValueType) switch
            {
                (true, true) => "record struct",
                (true, false) => "record",
                (false, true) => "struct",
                (false, false) => "class",
            };
            if (typeMeta.IsUnion)
            {
                typeDeclarationKeyword = typeMeta.Symbol.IsRecord
                    ? "record"
                    : typeMeta.Symbol.TypeKind == TypeKind.Interface ? "interface" : "class";
            }

            using (codeWriter.BeginBlockScope($"partial {typeDeclarationKeyword} {typeMeta.TypeName}"))
            {
                // EmitCCtor(typeMeta, codeWriter, in context);
                if (typeMeta.Symbol.TypeKind != TypeKind.Interface)
                {
                    if (!TryEmitRegisterMethod(typeMeta, codeWriter, in context))
                    {
                        return false;
                    }
                }
                if (!TryEmitFormatter(typeMeta, codeWriter, references, in context))
                {
                    return false;
                }
            }

            if (!ns.IsGlobalNamespace)
            {
                codeWriter.EndBlock();
            }

            codeWriter.AppendLine("#pragma warning restore CS0162 // Unreachable code");
            codeWriter.AppendLine("#pragma warning restore CS0219 // Variable assigned but never used");
            codeWriter.AppendLine("#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.");
            codeWriter.AppendLine("#pragma warning restore CS8601 // Possible null reference assignment");
            codeWriter.AppendLine("#pragma warning restore CS8602 // Possible null return");
            codeWriter.AppendLine("#pragma warning restore CS8604 // Possible null reference argument for parameter");
            codeWriter.AppendLine("#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method");
            return true;
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.UnexpectedErrorDescriptor,
                Location.None,
                ex.ToString()));
            return false;
        }
    }

    static void EmitCCtor(TypeMeta typeMeta, CodeWriter codeWriter)
    {
        using var _ = codeWriter.BeginBlockScope($"static {typeMeta.TypeName}()");
        codeWriter.AppendLine($"__RegisterVYamlFormatter();");
    }

    static bool TryEmitRegisterMethod(TypeMeta typeMeta, CodeWriter codeWriter, in SourceProductionContext context)
    {
        codeWriter.AppendLine("[VYaml.Annotations.Preserve]");
        using var _ = codeWriter.BeginBlockScope("public static void __RegisterVYamlFormatter()");

        var typeName = typeMeta.TypeName.Replace("<", "_").Replace(">", "_").Replace(",", "_").Replace(" ", "");
        codeWriter.AppendLine($"global::VYaml.Serialization.GeneratedResolver.Register(new {typeName}GeneratedFormatter());");
        return true;
    }

    static bool TryEmitFormatter(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        ReferenceSymbols references,
        in SourceProductionContext context)
    {
        var returnType = typeMeta.Symbol.IsValueType
            ? typeMeta.FullTypeName
            : $"{typeMeta.FullTypeName}?";

        codeWriter.AppendLine("[VYaml.Annotations.Preserve]");

        var typeName = typeMeta.TypeName.Replace("<", "_").Replace(">", "_").Replace(",", "_").Replace(" ", "");
        using var _ = codeWriter.BeginBlockScope($"public class {typeName}GeneratedFormatter : IYamlFormatter<{returnType}>");

        // Union
        if (typeMeta.IsUnion)
        {
            return TryEmitSerializeMethodUnion(typeMeta, codeWriter, in context) &&
                   TryEmitDeserializeMethodUnion(typeMeta, codeWriter, in context);
        }

        // Default
        foreach (var memberMeta in typeMeta.MemberMetas)
        {
            codeWriter.Append($"static readonly byte[] {memberMeta.Name}KeyUtf8Bytes = ");
            codeWriter.AppendByteArrayString(memberMeta.KeyNameUtf8Bytes);
            codeWriter.AppendLine($"; // {memberMeta.KeyName}", false);
            codeWriter.AppendLine();
        }

        return TryEmitSerializeMethod(typeMeta, codeWriter, in context) &&
               TryEmitDeserializeMethod(typeMeta, codeWriter, references, in context);
    }

    static bool TryEmitSerializeMethod(TypeMeta typeMeta, CodeWriter codeWriter, in SourceProductionContext context)
    {
        var memberMetas = typeMeta.MemberMetas;
        var returnType = typeMeta.Symbol.IsValueType
            ? typeMeta.FullTypeName
            : $"{typeMeta.FullTypeName}?";

        codeWriter.AppendLine("[VYaml.Annotations.Preserve]");
        using var methodScope = codeWriter.BeginBlockScope(
            $"public void Serialize(ref Utf8YamlEmitter emitter, {returnType} value, YamlSerializationContext context)");

        if (!typeMeta.Symbol.IsValueType)
        {
            using (codeWriter.BeginBlockScope("if (value is null)"))
            {
                codeWriter.AppendLine("emitter.WriteNull();");
                codeWriter.AppendLine("return;");
            }
        }

        codeWriter.AppendLine("emitter.BeginMapping();");
        foreach (var memberMeta in memberMetas)
        {
            if (memberMeta.HasKeyNameAlias || typeMeta.NamingConventionByType != NamingConvention.LowerCamelCase)
            {
                codeWriter.AppendLine($"emitter.WriteString(\"{memberMeta.KeyName}\");");
            }
            else
            {
                using (codeWriter.BeginBlockScope($"if (context.Options.NamingConvention == global::VYaml.Annotations.NamingConvention.{memberMeta.NamingConventionByType})"))
                {
                    codeWriter.AppendLine($"emitter.WriteScalar({memberMeta.Name}KeyUtf8Bytes);");
                }
                using (codeWriter.BeginBlockScope("else"))
                {
                    codeWriter.AppendLine($"global::VYaml.Serialization.NamingConventionMutator.MutateToThreadStaticBufferUtf8({memberMeta.Name}KeyUtf8Bytes, context.Options.NamingConvention, out var mutated, out var written);");
                    codeWriter.AppendLine("emitter.WriteScalar(mutated.AsSpan(0, written));");
                }
            }
            codeWriter.AppendLine($"context.Serialize(ref emitter, value.{memberMeta.Name});");
        }
        codeWriter.AppendLine("emitter.EndMapping();");

        return true;
    }

    static bool TryEmitSerializeMethodUnion(TypeMeta typeMeta, CodeWriter codeWriter, in SourceProductionContext context)
    {
        var returnType = typeMeta.Symbol.IsValueType
            ? typeMeta.FullTypeName
            : $"{typeMeta.FullTypeName}?";

        codeWriter.AppendLine("[VYaml.Annotations.Preserve]");
        using var methodScope = codeWriter.BeginBlockScope(
            $"public void Serialize(ref Utf8YamlEmitter emitter, {returnType} value, YamlSerializationContext context)");

        if (!typeMeta.Symbol.IsValueType)
        {
            using (codeWriter.BeginBlockScope("if (value is null)"))
            {
                codeWriter.AppendLine("emitter.WriteNull();");
                codeWriter.AppendLine("return;");
            }
        }

        using (codeWriter.BeginBlockScope("switch (value)"))
        {
            foreach (var unionMeta in typeMeta.UnionMetas)
            {
                codeWriter.AppendLine($"case {unionMeta.FullTypeName} x:");
                codeWriter.AppendLine($"    emitter.Tag(\"{unionMeta.SubTypeTag}\");");
                codeWriter.AppendLine($"    context.Serialize(ref emitter, x);");
                codeWriter.AppendLine( "    break;");
            }
        }
        return true;
    }

    static bool TryEmitDeserializeMethod(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        ReferenceSymbols references,
        in SourceProductionContext context)
    {
        if (!TryGetConstructor(typeMeta, references, in context,
                out var selectedConstructor,
                out var constructedMembers))
        {
            return false;
        }

        var setterMembers = typeMeta.MemberMetas
            .Where(x =>
            {
                return constructedMembers.All(constructedMember => !SymbolEqualityComparer.Default.Equals(x.Symbol, constructedMember.Symbol));
            })
            .ToArray();

        foreach (var setterMember in setterMembers)
        {
            switch (setterMember)
            {
                case { IsProperty: true, IsSettable: false }:
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.YamlMemberPropertyMustHaveSetter,
                        setterMember.GetLocation(typeMeta.Syntax),
                        typeMeta.TypeName,
                        setterMember.Name));
                    return false;
                }
                case { IsField: true, IsSettable: false }:
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.YamlMemberFieldCannotBeReadonly,
                        setterMember.GetLocation(typeMeta.Syntax),
                        typeMeta.TypeName,
                        setterMember.Name));
                    return false;
            }
        }

        var returnType = typeMeta.Symbol.IsValueType
            ? typeMeta.FullTypeName
            : $"{typeMeta.FullTypeName}?";
        codeWriter.AppendLine("[VYaml.Annotations.Preserve]");
        using var methodScope = codeWriter.BeginBlockScope(
            $"public {returnType} Deserialize(ref YamlParser parser, YamlDeserializationContext context)");

        using (codeWriter.BeginBlockScope("if (parser.IsNullScalar())"))
        {
            codeWriter.AppendLine("parser.Read();");
            codeWriter.AppendLine("return default;");
        }

        if (typeMeta.MemberMetas.Count <= 0)
        {
            codeWriter.AppendLine("parser.SkipCurrentNode();");
            codeWriter.AppendLine($"return new {typeMeta.TypeName}();");
            return true;
        }

        codeWriter.AppendLine("parser.ReadWithVerify(ParseEventType.MappingStart);");
        codeWriter.AppendLine();
        foreach (var memberMeta in typeMeta.MemberMetas)
        {
            codeWriter.AppendLine($"var __{memberMeta.Name}__ = {memberMeta.EmitDefaultValue()};");
        }

        using (codeWriter.BeginBlockScope("while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)"))
        {
            using (codeWriter.BeginBlockScope("if (parser.CurrentEventType != ParseEventType.Scalar)"))
            {
                codeWriter.AppendLine("throw new YamlSerializerException(parser.CurrentMark, \"Custom type deserialization supports only string key\");");
            }
            codeWriter.AppendLine();
            using (codeWriter.BeginBlockScope("if (!parser.TryGetScalarAsSpan(out var key))"))
            {
                codeWriter.AppendLine("throw new YamlSerializerException(parser.CurrentMark, \"Custom type deserialization supports only string key\");");
            }
            codeWriter.AppendLine();

            using (codeWriter.BeginBlockScope($"if (context.Options.NamingConvention != global::VYaml.Annotations.NamingConvention.{typeMeta.NamingConventionByType})"))
            {
                codeWriter.AppendLine($"global::VYaml.Serialization.NamingConventionMutator.MutateToThreadStaticBufferUtf8(key, global::VYaml.Annotations.NamingConvention.{typeMeta.NamingConventionByType}, out var mutated, out var written);");
                codeWriter.AppendLine("key = mutated.AsSpan(0, written);");
            }

            using (codeWriter.BeginBlockScope("switch (key.Length)"))
            {
                var membersByNameLength = typeMeta.MemberMetas.GroupBy(x => x.KeyNameUtf8Bytes.Length);
                foreach (var group in membersByNameLength)
                {
                    using (codeWriter.BeginIndentScope($"case {group.Key}:"))
                    {
                        var branching = "if";
                        foreach (var memberMeta in group)
                        {
                            using (codeWriter.BeginBlockScope($"{branching} (key.SequenceEqual({memberMeta.Name}KeyUtf8Bytes))"))
                            {
                                codeWriter.AppendLine("parser.Read(); // skip key");
                                codeWriter.AppendLine($"__{memberMeta.Name}__ = context.DeserializeWithAlias<{memberMeta.FullTypeName}>(ref parser);");
                                codeWriter.AppendLine("continue;");
                            }
                            branching = "else if";
                        }
                        codeWriter.AppendLine("goto default;");
                    }
                }

                using (codeWriter.BeginIndentScope("default:"))
                {
                    codeWriter.AppendLine("parser.Read(); // skip key");
                    codeWriter.AppendLine("parser.SkipCurrentNode(); // skip value");
                    codeWriter.AppendLine("continue;");
                }
            }
        }
        codeWriter.AppendLine("parser.ReadWithVerify(ParseEventType.MappingEnd);");

        codeWriter.Append("return new ");
        if (selectedConstructor != null)
        {
            var parameters = string.Join(",", constructedMembers.Select(x => $"__{x.Name}__"));
            codeWriter.Append($"{typeMeta.TypeName}({parameters})", false);
        }
        else
        {
            codeWriter.Append($"{typeMeta.TypeName}", false);
        }

        if (setterMembers.Length > 0)
        {
            using (codeWriter.BeginBlockScope())
            {
                foreach (var setterMember in setterMembers)
                {
                    if (!constructedMembers.Contains(setterMember))
                    {
                        codeWriter.AppendLine($"{setterMember.Name} = __{setterMember.Name}__,");
                    }
                }
            }
        }
        codeWriter.AppendLine(";");
        return true;
    }

    static bool TryEmitDeserializeMethodUnion(TypeMeta typeMeta, CodeWriter codeWriter, in SourceProductionContext context)
    {
        var returnType = typeMeta.Symbol.IsValueType
            ? typeMeta.FullTypeName
            : $"{typeMeta.FullTypeName}?";

        codeWriter.AppendLine("[VYaml.Annotations.Preserve]");
        using var methodScope = codeWriter.BeginBlockScope(
            $"public {returnType} Deserialize(ref YamlParser parser, YamlDeserializationContext context)");

        using (codeWriter.BeginBlockScope("if (parser.IsNullScalar())"))
        {
            codeWriter.AppendLine("parser.Read();");
            codeWriter.AppendLine("return default;");
        }

        using (codeWriter.BeginBlockScope("if (!parser.TryGetCurrentTag(out var tag))"))
        {
            codeWriter.AppendLine("throw new YamlSerializerException(parser.CurrentMark, \"Cannot find any tag for union\");");
        }

        codeWriter.AppendLine();

        var branch = "if";
        foreach (var unionMeta in typeMeta.UnionMetas)
        {
            using (codeWriter.BeginBlockScope($"{branch} (tag.Equals(\"{unionMeta.SubTypeTag}\")) "))
            {
                codeWriter.AppendLine($"return context.DeserializeWithAlias<{unionMeta.FullTypeName}>(ref parser);");
            }
            branch = "else if";
        }
        using (codeWriter.BeginBlockScope("else"))
        {
            codeWriter.AppendLine("throw new YamlSerializerException(parser.CurrentMark, \"Cannot find any subtype tag for union\");");
        }
        return true;
    }

    static bool TryGetConstructor(
        TypeMeta typeMeta,
        ReferenceSymbols reference,
        in SourceProductionContext context,
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
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.MultipleConstructorAttribute,
                        typeMeta.Syntax.Identifier.GetLocation(),
                        typeMeta.Symbol.Name));
                    selectedConstructor = null;
                    constructedMembers = Array.Empty<MemberMeta>();
                    return false;

                default:
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.MultipleConstructorWithoutAttribute,
                        typeMeta.Syntax.Identifier.GetLocation(),
                        typeMeta.Symbol.Name));
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
                .FirstOrDefault(member => parameter.Name.Equals(member.Name, StringComparison.OrdinalIgnoreCase));
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
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.ConstructorHasNoMatchedParameter,
                    location,
                    typeMeta.Symbol.Name,
                    parameter.Name));
                constructedMembers = Array.Empty<MemberMeta>();
                error = true;
            }
        }
        constructedMembers = parameterMembers;
        return !error;
    }
}