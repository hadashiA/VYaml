using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace VYaml.SourceGenerator;

// Walks the closed type graph reachable from a serialized member (or a declared
// [assembly: YamlSerializable] root) and produces the registration statements the
// module initializer emits. Statically constructing the closed generic formatters
// (EnumAsStringFormatter<TEnum>, ListFormatter<T>, DictionaryFormatter<K, V>, closed
// user generics, ...) forces their instantiations into the binary, which is what makes
// them resolvable on Native AOT where BuiltinResolver's MakeGenericType path cannot
// create them at runtime.
// Value-equatable result of analyzing one [assembly: YamlSerializable] declaration.
sealed record AotRootModel
{
    public EquatableArray<string> Statements { get; }
    public EquatableArray<DiagnosticInfo> Diagnostics { get; }

    public AotRootModel(EquatableArray<string> statements, EquatableArray<DiagnosticInfo> diagnostics)
    {
        Statements = statements;
        Diagnostics = diagnostics;
    }
}

static class RegistrationHarvester
{
    const int MaxDepth = 16;

    // metadata name of the open generic definition -> VYaml formatter (open, fully qualified without arity)
    static readonly Dictionary<string, string> KnownGenericFormatters = new()
    {
        { "System.Nullable`1", "global::VYaml.Serialization.NullableFormatter" },
        { "System.Collections.Generic.KeyValuePair`2", "global::VYaml.Serialization.KeyValuePairFormatter" },

        { "System.Tuple`1", "global::VYaml.Serialization.TupleFormatter" },
        { "System.Tuple`2", "global::VYaml.Serialization.TupleFormatter" },
        { "System.Tuple`3", "global::VYaml.Serialization.TupleFormatter" },
        { "System.Tuple`4", "global::VYaml.Serialization.TupleFormatter" },
        { "System.Tuple`5", "global::VYaml.Serialization.TupleFormatter" },
        { "System.Tuple`6", "global::VYaml.Serialization.TupleFormatter" },
        { "System.Tuple`7", "global::VYaml.Serialization.TupleFormatter" },
        { "System.Tuple`8", "global::VYaml.Serialization.TupleFormatter" },
        { "System.ValueTuple`1", "global::VYaml.Serialization.ValueTupleFormatter" },
        { "System.ValueTuple`2", "global::VYaml.Serialization.ValueTupleFormatter" },
        { "System.ValueTuple`3", "global::VYaml.Serialization.ValueTupleFormatter" },
        { "System.ValueTuple`4", "global::VYaml.Serialization.ValueTupleFormatter" },
        { "System.ValueTuple`5", "global::VYaml.Serialization.ValueTupleFormatter" },
        { "System.ValueTuple`6", "global::VYaml.Serialization.ValueTupleFormatter" },
        { "System.ValueTuple`7", "global::VYaml.Serialization.ValueTupleFormatter" },
        { "System.ValueTuple`8", "global::VYaml.Serialization.ValueTupleFormatter" },

        { "System.Collections.Generic.List`1", "global::VYaml.Serialization.ListFormatter" },
        { "System.Collections.Generic.Stack`1", "global::VYaml.Serialization.StackFormatter" },
        { "System.Collections.Generic.Queue`1", "global::VYaml.Serialization.QueueFormatter" },
        { "System.Collections.Generic.LinkedList`1", "global::VYaml.Serialization.LinkedListFormatter" },
        { "System.Collections.Generic.HashSet`1", "global::VYaml.Serialization.HashSetFormatter" },
        { "System.Collections.Generic.SortedSet`1", "global::VYaml.Serialization.SortedSetFormatter" },
        { "System.Collections.Generic.Dictionary`2", "global::VYaml.Serialization.DictionaryFormatter" },

        { "System.Collections.Concurrent.BlockingCollection`1", "global::VYaml.Serialization.BlockingCollectionFormatter" },
        { "System.Collections.Concurrent.ConcurrentQueue`1", "global::VYaml.Serialization.ConcurrentQueueFormatter" },
        { "System.Collections.Concurrent.ConcurrentStack`1", "global::VYaml.Serialization.ConcurrentStackFormatter" },
        { "System.Collections.Concurrent.ConcurrentBag`1", "global::VYaml.Serialization.ConcurrentBagFormatter" },

        { "System.Collections.Generic.IEnumerable`1", "global::VYaml.Serialization.InterfaceEnumerableFormatter" },
        { "System.Collections.Generic.ICollection`1", "global::VYaml.Serialization.InterfaceCollectionFormatter" },
        { "System.Collections.Generic.IReadOnlyCollection`1", "global::VYaml.Serialization.InterfaceReadOnlyCollectionFormatter" },
        { "System.Collections.Generic.IList`1", "global::VYaml.Serialization.InterfaceListFormatter" },
        { "System.Collections.Generic.IReadOnlyList`1", "global::VYaml.Serialization.InterfaceReadOnlyListFormatter" },
        { "System.Collections.Generic.IDictionary`2", "global::VYaml.Serialization.InterfaceDictionaryFormatter" },
        { "System.Collections.Generic.IReadOnlyDictionary`2", "global::VYaml.Serialization.InterfaceReadOnlyDictionaryFormatter" },
        { "System.Collections.Generic.ISet`1", "global::VYaml.Serialization.InterfaceSetFormatter" },
    };

    public static void Harvest(
        ITypeSymbol type,
        ReferenceSymbols references,
        IAssemblySymbol currentAssembly,
        HashSet<string> visited,
        List<string> statements,
        int depth = 0)
    {
        if (depth > MaxDepth) return;
        if (type.TypeKind == TypeKind.Error) return;
        if (ContainsTypeParameter(type)) return;
        if (!IsAccessible(type, currentAssembly)) return;

        var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (!visited.Add(fullName)) return;

        switch (type)
        {
            case IArrayTypeSymbol array:
            {
                var formatter = array.Rank switch
                {
                    1 => "global::VYaml.Serialization.ArrayFormatter",
                    2 => "global::VYaml.Serialization.TwoDimensionalArrayFormatter",
                    3 => "global::VYaml.Serialization.ThreeDimensionalArrayFormatter",
                    4 => "global::VYaml.Serialization.FourDimensionalArrayFormatter",
                    _ => null,
                };
                // byte[] has a dedicated non-generic formatter in BuiltinResolver's static map
                if (formatter != null && !(array.Rank == 1 && array.ElementType.SpecialType == SpecialType.System_Byte))
                {
                    var element = array.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    statements.Add($"global::VYaml.Serialization.GeneratedResolver.Register(new {formatter}<{element}>());");
                }
                Harvest(array.ElementType, references, currentAssembly, visited, statements, depth + 1);
                return;
            }
            case INamedTypeSymbol named:
            {
                if (named.TypeKind == TypeKind.Enum)
                {
                    statements.Add($"global::VYaml.Serialization.GeneratedResolver.Register(new global::VYaml.Serialization.EnumAsStringFormatter<{fullName}>());");
                    return;
                }

                if (!named.IsGenericType)
                {
                    // A non-generic [YamlObject] registers itself from its own assembly's
                    // module initializer; primitives and other well-known types live in
                    // BuiltinResolver's static map. Nothing to do either way.
                    return;
                }

                if (named.OriginalDefinition.ContainsAttribute(references.YamlObjectAttribute))
                {
                    // Closed instantiation of a generic [YamlObject]: the generated
                    // __RegisterVYamlFormatter/formatter class is generic over the same type
                    // parameters, so referencing the closed form statically compiles the
                    // instantiation into this binary.
                    if (named.TypeKind == TypeKind.Interface)
                    {
                        var openTypeName = named.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                        var formatterName = $"{TypeMetaAnalyzer.Sanitize(openTypeName)}GeneratedFormatter";
                        statements.Add($"global::VYaml.Serialization.GeneratedResolver.Register(new {fullName}.{formatterName}());");
                    }
                    else
                    {
                        statements.Add($"{fullName}.__RegisterVYamlFormatter();");
                    }

                    foreach (var typeArgument in named.TypeArguments)
                    {
                        Harvest(typeArgument, references, currentAssembly, visited, statements, depth + 1);
                    }

                    // Member types of the closed instantiation are substituted (List<T> in the
                    // open declaration becomes List<int> here), so recurse to pick them up.
                    foreach (var member in named.GetAllMembers())
                    {
                        if (member is not (IFieldSymbol or IPropertySymbol)) continue;
                        if (member.IsStatic || member.IsImplicitlyDeclared) continue;
                        if (member.ContainsAttribute(references.YamlIgnoreAttribute)) continue;
                        if (member.DeclaredAccessibility != Accessibility.Public &&
                            !member.ContainsAttribute(references.YamlMemberAttribute))
                        {
                            continue;
                        }

                        var memberType = member switch
                        {
                            IFieldSymbol f => f.Type,
                            IPropertySymbol p => p.Type,
                            _ => null,
                        };
                        if (memberType != null)
                        {
                            Harvest(memberType, references, currentAssembly, visited, statements, depth + 1);
                        }
                    }
                    return;
                }

                var metadataKey = named.ContainingNamespace is { IsGlobalNamespace: false } ns
                    ? $"{ns.ToDisplayString()}.{named.MetadataName}"
                    : named.MetadataName;
                if (KnownGenericFormatters.TryGetValue(metadataKey, out var openFormatter))
                {
                    var args = new string[named.TypeArguments.Length];
                    for (var i = 0; i < named.TypeArguments.Length; i++)
                    {
                        args[i] = named.TypeArguments[i].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    }
                    statements.Add($"global::VYaml.Serialization.GeneratedResolver.Register(new {openFormatter}<{string.Join(", ", args)}>());");

                    foreach (var typeArgument in named.TypeArguments)
                    {
                        Harvest(typeArgument, references, currentAssembly, visited, statements, depth + 1);
                    }
                }
                return;
            }
            default:
                return;
        }
    }

    // Analyzes one [assembly: YamlSerializable(typeof(...))] root declaration: validates the
    // type is closed, then harvests registration statements for its whole reachable closure.
    public static AotRootModel AnalyzeRoot(
        ITypeSymbol type,
        ReferenceSymbols references,
        IAssemblySymbol currentAssembly,
        Location? location)
    {
        var statements = new List<string>();
        var diagnostics = new List<DiagnosticInfo>();
        if (ContainsTypeParameter(type))
        {
            diagnostics.Add(DiagnosticInfo.Create(
                DiagnosticDescriptors.YamlSerializableMustBeClosed, location, type.ToDisplayString()));
        }
        else
        {
            Harvest(type, references, currentAssembly, new HashSet<string>(), statements);
        }
        return new AotRootModel(statements.ToEquatableArray(), diagnostics.ToEquatableArray());
    }

    public static bool ContainsTypeParameter(ITypeSymbol type)
    {
        switch (type)
        {
            case ITypeParameterSymbol:
                return true;
            case IArrayTypeSymbol array:
                return ContainsTypeParameter(array.ElementType);
            case INamedTypeSymbol named:
                if (named.IsUnboundGenericType) return true;
                foreach (var argument in named.TypeArguments)
                {
                    if (ContainsTypeParameter(argument)) return true;
                }
                return false;
            default:
                return false;
        }
    }

    // The registration statements live in a generated internal class of the current
    // assembly: a referenced type must be public all the way down, or internal within
    // this same assembly.
    static bool IsAccessible(ITypeSymbol type, IAssemblySymbol currentAssembly)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                return IsAccessible(array.ElementType, currentAssembly);
            case INamedTypeSymbol named:
            {
                for (var t = (INamedTypeSymbol?)named; t != null; t = t.ContainingType)
                {
                    switch (t.DeclaredAccessibility)
                    {
                        case Accessibility.Public:
                            break;
                        case Accessibility.Internal:
                        case Accessibility.ProtectedOrInternal:
                            if (!SymbolEqualityComparer.Default.Equals(t.ContainingAssembly, currentAssembly))
                            {
                                return false;
                            }
                            break;
                        case Accessibility.NotApplicable: // e.g. error/dynamic
                            break;
                        default:
                            return false;
                    }
                }
                foreach (var argument in named.TypeArguments)
                {
                    if (!IsAccessible(argument, currentAssembly)) return false;
                }
                return true;
            }
            case ITypeParameterSymbol:
                return false;
            default:
                return true;
        }
    }
}
