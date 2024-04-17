using Microsoft.CodeAnalysis;

namespace VYaml.SourceGenerator;

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

            var codeWriter = new CodeWriter();
            if (context.SyntaxContextReceiver! is not SyntaxContextReceiver syntaxCollector) return;

            foreach (var workItem in syntaxCollector.GetWorkItems())
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var typeMeta = workItem.Analyze(in context, references);
                if (typeMeta is null) continue;

                if (TryEmit(typeMeta, codeWriter, references, in context))
                {
                    var fullType = typeMeta.FullTypeName
                        .Replace("global::", "")
                        .Replace("<", "_")
                        .Replace(">", "_");

                    context.AddSource($"{fullType}.YamlFormatter.g.cs", codeWriter.ToString());
                }
                codeWriter.Clear();
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

    static bool TryEmit(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        ReferenceSymbols references,
        in GeneratorExecutionContext context)
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
                if (!TryEmitRegisterMethod(typeMeta, codeWriter, in context))
                {
                    return false;
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
                ex.ToString().Replace(Environment.NewLine, " ")));
            return false;
        }
    }

    static void EmitCCtor(TypeMeta typeMeta, CodeWriter codeWriter)
    {
        using var _ = codeWriter.BeginBlockScope($"static {typeMeta.TypeName}()");
        codeWriter.AppendLine($"__RegisterVYamlFormatter();");
    }

    static bool TryEmitRegisterMethod(TypeMeta typeMeta, CodeWriter codeWriter, in GeneratorExecutionContext context)
    {
        codeWriter.AppendLine("[VYaml.Annotations.Preserve]");
        using var _ = codeWriter.BeginBlockScope("public static void __RegisterVYamlFormatter()");
        codeWriter.AppendLine($"global::VYaml.Serialization.GeneratedResolver.Register(new {typeMeta.TypeName}GeneratedFormatter());");
        return true;
    }

    static bool TryEmitFormatter(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        ReferenceSymbols references,
        in GeneratorExecutionContext context)
    {
        var returnType = typeMeta.Symbol.IsValueType
            ? typeMeta.FullTypeName
            : $"{typeMeta.FullTypeName}?";

        codeWriter.AppendLine("[VYaml.Annotations.Preserve]");
        using var _ = codeWriter.BeginBlockScope($"public class {typeMeta.TypeName}GeneratedFormatter : IYamlFormatter<{returnType}>");

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

    static bool TryEmitSerializeMethod(TypeMeta typeMeta, CodeWriter codeWriter, in GeneratorExecutionContext context)
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
            if (memberMeta.HasKeyNameAlias)
            {
                codeWriter.AppendLine($"emitter.WriteString(\"{memberMeta.KeyName}\");");
            }
            else
            {
                codeWriter.AppendLine($"emitter.WriteString(\"{memberMeta.KeyName}\", ScalarStyle.Plain);");
            }
            codeWriter.AppendLine($"context.Serialize(ref emitter, value.{memberMeta.Name});");
        }
        codeWriter.AppendLine("emitter.EndMapping();");

        return true;
    }

    static bool TryEmitSerializeMethodUnion(TypeMeta typeMeta, CodeWriter codeWriter, in GeneratorExecutionContext context)
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
        in GeneratorExecutionContext context)
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
            codeWriter.Append($"var __{memberMeta.Name}__ = {memberMeta.EmitDefaultValue()};");
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
                                codeWriter.AppendLine(
                                    $"__{memberMeta.Name}__ = context.DeserializeWithAlias<{memberMeta.FullTypeName}>(ref parser);");
                            }
                            branching = "else if";
                        }
                        using (codeWriter.BeginBlockScope("else"))
                        {
                            codeWriter.AppendLine("parser.Read(); // skip key");
                            codeWriter.AppendLine("parser.SkipCurrentNode(); // skip value");
                        }
                        codeWriter.AppendLine("continue;");
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
            var parameters = string.Join(",", constructedMembers.Select(x =>
            {
                if (x.HasExplicitDefaultValueFromConstructor)
                {
                    return $"__{x.Name}__ = {x.EmitDefaultValue()}";
                }
                return $"__{x.Name}__";
            }));
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

    static bool TryEmitDeserializeMethodUnion(TypeMeta typeMeta, CodeWriter codeWriter, in GeneratorExecutionContext context)
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
        in GeneratorExecutionContext context,
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