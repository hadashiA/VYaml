using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using VYaml.Annotations;

namespace VYaml.Serialization
{
    /// <summary>
    /// Generates a JSON Schema (draft-07) that describes the YAML representation of a type,
    /// e.g. for yaml-language-server (VS Code YAML extension) or JetBrains IDEs.
    /// </summary>
    /// <remarks>
    /// [YamlObject] types are described from the metadata emitted by the source generator, so the keys
    /// match the generated formatter (YamlMember aliases, naming conventions, ignored members, etc.).
    /// </remarks>
    public static class YamlSchemaGenerator
    {
        public const string SchemaUri = "http://json-schema.org/draft-07/schema#";

        public static string Generate<T>(YamlSerializerOptions? options = null, YamlSchemaGeneratorOptions? schemaOptions = null) =>
            Generate(typeof(T), options, schemaOptions);

        public static string Generate(Type type, YamlSerializerOptions? options = null, YamlSchemaGeneratorOptions? schemaOptions = null)
        {
            var builder = new SchemaBuilder(type, options ?? YamlSerializerOptions.Standard, schemaOptions ?? new YamlSchemaGeneratorOptions());
            var schema = builder.Build();
            var sb = new StringBuilder();
            JsonWriter.Write(sb, schema, 0);
            sb.Append('\n');
            return sb.ToString();
        }

        sealed class SchemaBuilder
        {
            readonly Type rootType;
            readonly YamlSerializerOptions options;
            readonly YamlSchemaGeneratorOptions schemaOptions;
            readonly bool additionalProperties;
            readonly Dictionary<Type, string> definitionNames = new();
            readonly HashSet<string> usedDefinitionNames = new();
            readonly List<KeyValuePair<string, object?>> definitions = new();
            // GetSchemaInfo() allocates, and the same type is looked up several times
            readonly Dictionary<Type, YamlObjectSchemaInfo?> schemaInfos = new();

            public SchemaBuilder(Type rootType, YamlSerializerOptions options, YamlSchemaGeneratorOptions schemaOptions)
            {
                this.rootType = rootType;
                this.options = options;
                this.schemaOptions = schemaOptions;
                additionalProperties = schemaOptions.AdditionalProperties ??
                                       options.UnmappedMemberHandling != YamlUnmappedMemberHandling.Disallow;
            }

            bool TryGetSchemaInfo(Type type, [NotNullWhen(true)] out YamlObjectSchemaInfo? info)
            {
                if (!schemaInfos.TryGetValue(type, out info))
                {
                    YamlSchemaInfoRegistry.TryGetSchemaInfo(type, out info);
                    schemaInfos.Add(type, info);
                }
                return info != null;
            }

            public JsonObject Build()
            {
                var root = new JsonObject { { "$schema", SchemaUri } };

                var schema = !schemaOptions.TryGetTypeSchema(rootType, out _) && TryGetSchemaInfo(rootType, out var info)
                    ? ObjectSchema(info)
                    : SchemaOf(rootType);
                foreach (var entry in schema)
                {
                    root.Add(entry.Key, entry.Value);
                }

                if (definitions.Count > 0)
                {
                    var defs = new JsonObject();
                    foreach (var definition in definitions.OrderBy(x => x.Key, StringComparer.Ordinal))
                    {
                        defs.Add(definition.Key, definition.Value);
                    }
                    root.Add("definitions", defs);
                }
                return root;
            }

            JsonObject SchemaOf(Type type)
            {
                if (Nullable.GetUnderlyingType(type) is { } underlyingType)
                {
                    return WithNull(SchemaOf(underlyingType));
                }

                if (schemaOptions.TryGetTypeSchema(type, out var customSchema))
                {
                    return CustomSchema(customSchema);
                }

                if (PrimitiveSchemaOf(type) is { } primitive)
                {
                    return primitive;
                }

                if (type.IsEnum)
                {
                    return EnumSchema(type);
                }

                if (type.IsArray)
                {
                    var elementSchema = SchemaOf(type.GetElementType()!);
                    var schema = new JsonObject { { "type", "array" } };
                    // multi-dimensional arrays are written as nested sequences
                    for (var rank = type.GetArrayRank(); rank > 1; rank--)
                    {
                        elementSchema = new JsonObject { { "type", "array" }, { "items", elementSchema } };
                    }
                    schema.Add("items", elementSchema);
                    return schema;
                }

                if (type.IsGenericType && GenericSchemaOf(type) is { } generic)
                {
                    return generic;
                }

                if (definitionNames.TryGetValue(type, out var definitionName))
                {
                    return Ref(definitionName);
                }

                if (type == rootType && TryGetSchemaInfo(type, out _))
                {
                    return new JsonObject { { "$ref", "#" } };
                }

                if (TryGetSchemaInfo(type, out var info))
                {
                    var name = DefinitionNameOf(type);
                    definitionNames.Add(type, name);
                    var index = definitions.Count;
                    definitions.Add(new KeyValuePair<string, object?>(name, null)); // reserve for recursive references
                    definitions[index] = new KeyValuePair<string, object?>(name, ObjectSchema(info));
                    return Ref(name);
                }

                // object, dynamic, or types handled by custom formatters: allow anything
                return new JsonObject();
            }

            JsonObject ObjectSchema(YamlObjectSchemaInfo info)
            {
                var schema = new JsonObject();

                if (info.IsUnion && info.UnionDiscriminatorKeyName is { } discriminatorKeyName)
                {
                    var description = $"Specify the concrete type with the '{discriminatorKeyName}' key: " +
                                      string.Join(", ", info.Unions.Select(x => $"{x.Tag} ({TypeNameOf(x.SubType)})"));
                    schema.Add("type", "object");
                    schema.Add("description", info.Description is { } d ? $"{d}\n{description}" : description);
                    schema.Add("required", new JsonArray { discriminatorKeyName });
                    schema.Add("properties", new JsonObject
                    {
                        { discriminatorKeyName, new JsonObject { { "enum", new JsonArray(info.Unions.Select(x => (object?)x.Tag)) } } }
                    });
                    // The subtypes are expanded instead of $ref, so that the discriminator key is one of their properties.
                    // (otherwise "additionalProperties": false of a subtype rejects it; draft-07 has no unevaluatedProperties)
                    schema.Add("anyOf", new JsonArray(info.Unions.Select(x => (object?)DiscriminatedSubTypeSchema(x, discriminatorKeyName))));
                    return schema;
                }

                if (info.IsUnion && info.Unions[0].RequiredKey != null)
                {
                    var keys = info.Unions.Select(x => RequiredKeyOf(x)).ToArray();
                    var description = "The concrete type is identified by the key it contains: " +
                                      string.Join(", ", info.Unions.Select((x, i) => $"{keys[i]} ({TypeNameOf(x.SubType)})"));
                    schema.Add("type", "object");
                    schema.Add("description", info.Description is { } d ? $"{d}\n{description}" : description);
                    // Lists the keys so that editors can complete them (each value is validated by the subtype schema below)
                    var keyProperties = new JsonObject();
                    for (var i = 0; i < keys.Length; i++)
                    {
                        keyProperties[keys[i]] = new JsonObject { { "description", TypeNameOf(info.Unions[i].SubType) } };
                    }
                    schema.Add("properties", keyProperties);

                    // if/then/else chain: when several keys are contained, the one declared first wins (same as the deserializer).
                    // Unlike anyOf, only the selected subtype reports errors.
                    JsonObject chain = new()
                    {
                        { "anyOf", new JsonArray(keys.Select(x => (object?)new JsonObject { { "required", new JsonArray { x } } })) },
                    };
                    for (var i = keys.Length - 1; i >= 0; i--)
                    {
                        chain = new JsonObject
                        {
                            { "if", new JsonObject { { "required", new JsonArray { keys[i] } } } },
                            { "then", SchemaOf(info.Unions[i].SubType) },
                            { "else", chain },
                        };
                    }
                    foreach (var entry in chain)
                    {
                        schema.Add(entry.Key, entry.Value);
                    }
                    return schema;
                }

                if (info.IsUnion)
                {
                    var description = "Specify the concrete type with a YAML tag: " +
                                      string.Join(", ", info.Unions.Select(x => $"{x.Tag} ({TypeNameOf(x.SubType)})"));
                    schema.Add("description", info.Description is { } d ? $"{d}\n{description}" : description);
                    schema.Add("anyOf", new JsonArray(info.Unions.Select(x => (object?)SchemaOf(x.SubType))));
                    return schema;
                }

                schema.Add("type", "object");
                if (info.Description is { } typeDescription)
                {
                    schema.Add("description", typeDescription);
                }

                var properties = new JsonObject();
                var required = new JsonArray();
                foreach (var member in info.Members)
                {
                    var keyName = KeyNameOf(member);
                    if (member.IsRequired)
                    {
                        required.Add(keyName);
                    }

                    JsonObject memberSchema;
                    if (schemaOptions.TryGetMemberSchema(info.Type, member.Name, out var customMemberSchema))
                    {
                        memberSchema = CustomSchema(customMemberSchema);
                    }
                    else
                    {
                        memberSchema = SchemaOf(member.Type);
                        if (member.IsNullable && Nullable.GetUnderlyingType(member.Type) == null)
                        {
                            memberSchema = WithNull(memberSchema);
                        }
                    }

                    var annotations = new JsonObject();
                    if (member.Description is { } memberDescription)
                    {
                        annotations.Add("description", memberDescription);
                    }
                    if (member.HasDefaultValue)
                    {
                        annotations.Add("default", DefaultValueOf(member.DefaultValue));
                    }
                    properties[keyName] = WithAnnotations(memberSchema, annotations);
                }
                schema.Add("properties", properties);
                if (required.Count > 0)
                {
                    schema.Add("required", required);
                }
                if (!additionalProperties)
                {
                    schema.Add("additionalProperties", false);
                }
                return schema;
            }

            JsonObject DiscriminatedSubTypeSchema(YamlUnionSchemaInfo union, string discriminatorKeyName)
            {
                var discriminator = new JsonObject { { "const", union.Tag } };
                if (schemaOptions.TryGetTypeSchema(union.SubType, out _) || !TryGetSchemaInfo(union.SubType, out var subTypeInfo))
                {
                    return new JsonObject
                    {
                        { "properties", new JsonObject { { discriminatorKeyName, discriminator } } },
                        { "required", new JsonArray { discriminatorKeyName } },
                        { "allOf", new JsonArray { SchemaOf(union.SubType) } },
                    };
                }

                var schema = ObjectSchema(subTypeInfo);
                var result = new JsonObject();
                foreach (var entry in schema)
                {
                    switch (entry.Key)
                    {
                        case "properties" when entry.Value is JsonObject subTypeProperties:
                            var properties = new JsonObject { { discriminatorKeyName, discriminator } };
                            // (the source generator rejects a subtype member with the discriminator key: VYAML018)
                            foreach (var property in subTypeProperties)
                            {
                                properties[property.Key] = property.Value;
                            }
                            result.Add("properties", properties);
                            if (!schema.ContainsKey("required"))
                            {
                                result.Add("required", new JsonArray { discriminatorKeyName });
                            }
                            break;
                        case "required" when entry.Value is JsonArray subTypeRequired:
                            result.Add("required", new JsonArray(new object?[] { discriminatorKeyName }.Concat(subTypeRequired)));
                            break;
                        default:
                            result.Add(entry.Key, entry.Value);
                            break;
                    }
                }
                return result;
            }

            // A user-provided schema is embedded as is; allOf lets WithNull / WithAnnotations add keywords next to it.
            static JsonObject CustomSchema(string jsonSchema) => new()
            {
                { "allOf", new JsonArray { new RawJson(jsonSchema) } },
            };

            // The key in YAML, as the generated formatter writes it
            string KeyNameOf(YamlMemberSchemaInfo member) => member.IsKeyNameFixed
                ? member.KeyName
                : NamingConventionMutator.Mutate(member.KeyName, options.NamingConvention);

            string RequiredKeyOf(YamlUnionSchemaInfo union)
            {
                if (TryGetSchemaInfo(union.SubType, out var subTypeInfo) &&
                    subTypeInfo.Members.FirstOrDefault(x => x.KeyName == union.RequiredKey) is { } member)
                {
                    return KeyNameOf(member);
                }
                return union.RequiredKey!;
            }

            JsonObject? GenericSchemaOf(Type type)
            {
                var definition = type.GetGenericTypeDefinition();
                var arguments = type.GetGenericArguments();

                if (definition == typeof(List<>) ||
                    definition == typeof(IList<>) ||
                    definition == typeof(IReadOnlyList<>) ||
                    definition == typeof(ICollection<>) ||
                    definition == typeof(IReadOnlyCollection<>) ||
                    definition == typeof(IEnumerable<>) ||
                    definition == typeof(Stack<>) ||
                    definition == typeof(Queue<>) ||
                    definition == typeof(LinkedList<>) ||
                    definition == typeof(System.Collections.Concurrent.BlockingCollection<>) ||
                    definition == typeof(ConcurrentQueue<>) ||
                    definition == typeof(ConcurrentStack<>) ||
                    definition == typeof(ConcurrentBag<>))
                {
                    return new JsonObject { { "type", "array" }, { "items", SchemaOf(arguments[0]) } };
                }

                if (definition == typeof(HashSet<>) ||
                    definition == typeof(SortedSet<>) ||
                    definition == typeof(ISet<>))
                {
                    return new JsonObject
                    {
                        { "type", "array" },
                        { "items", SchemaOf(arguments[0]) },
                        { "uniqueItems", true },
                    };
                }

                if (definition == typeof(Dictionary<,>) ||
                    definition == typeof(IDictionary<,>) ||
                    definition == typeof(IReadOnlyDictionary<,>))
                {
                    var schema = new JsonObject { { "type", "object" } };
                    if (arguments[0].IsEnum && EnumSchema(arguments[0]).TryGetValue("enum", out var keys))
                    {
                        schema.Add("propertyNames", new JsonObject { { "enum", keys } });
                    }
                    schema.Add("additionalProperties", SchemaOf(arguments[1]));
                    return schema;
                }

                if (definition == typeof(KeyValuePair<,>) || IsTupleDefinition(definition))
                {
                    return new JsonObject
                    {
                        { "type", "array" },
                        { "items", new JsonArray(arguments.Select(x => (object?)SchemaOf(x))) },
                        { "minItems", arguments.Length },
                        { "maxItems", arguments.Length },
                    };
                }

                return null;
            }

            JsonObject EnumSchema(Type type)
            {
                if (type.IsDefined(typeof(FlagsAttribute), false))
                {
                    // composite flags are written as a comma separated list of names
                    return new JsonObject { { "type", "string" } };
                }

                var namingConventionByType = type.GetCustomAttribute<YamlObjectAttribute>()?.NamingConvention;
                var values = new JsonArray();
                foreach (var field in GetEnumFields(type))
                {
                    var attributes = field.GetCustomAttributes(inherit: true);
                    if (attributes.OfType<EnumMemberAttribute>().FirstOrDefault() is { Value: { } enumMemberValue })
                    {
                        values.Add(enumMemberValue);
                    }
                    else if (attributes.OfType<DataMemberAttribute>().FirstOrDefault() is { Name: { } dataMemberName })
                    {
                        values.Add(dataMemberName);
                    }
                    else
                    {
                        // same as EnumAsStringFormatter
                        var name = NamingConventionMutator.Mutate(field.Name, namingConventionByType ?? YamlSerializerOptions.DefaultNamingConvention);
                        if (namingConventionByType == null && options.NamingConvention != YamlSerializerOptions.DefaultNamingConvention)
                        {
                            name = NamingConventionMutator.Mutate(name, options.NamingConvention);
                        }
                        values.Add(name);
                    }
                }
                return new JsonObject { { "type", "string" }, { "enum", values } };
            }

            object? DefaultValueOf(object? value)
            {
                if (value is Enum e)
                {
                    var schema = EnumSchema(e.GetType());
                    if (schema.TryGetValue("enum", out var values) && values is JsonArray names)
                    {
                        var index = Array.IndexOf(GetEnumFields(e.GetType()).Select(x => x.GetValue(null)).ToArray(), value);
                        if (index >= 0) return names[index];
                    }
                    return e.ToString();
                }
                return value;
            }

            string DefinitionNameOf(Type type)
            {
                var name = TypeNameOf(type);
                if (!usedDefinitionNames.Add(name))
                {
                    name = string.IsNullOrEmpty(type.Namespace) ? name : $"{type.Namespace}.{name}";
                    var candidate = name;
                    for (var i = 2; !usedDefinitionNames.Add(candidate); i++)
                    {
                        candidate = $"{name}{i}";
                    }
                    name = candidate;
                }
                return name;
            }

            static string TypeNameOf(Type type)
            {
                if (!type.IsGenericType) return type.Name;
                var name = type.Name;
                var backtick = name.IndexOf('`');
                if (backtick >= 0) name = name.Substring(0, backtick);
                return $"{name}<{string.Join(",", type.GetGenericArguments().Select(TypeNameOf))}>";
            }

            static JsonObject Ref(string definitionName) =>
                new() { { "$ref", $"#/definitions/{EscapeJsonPointer(definitionName)}" } };

            static string EscapeJsonPointer(string value) =>
                Uri.EscapeDataString(value.Replace("~", "~0").Replace("/", "~1"));

            static JsonObject WithNull(JsonObject schema)
            {
                if (schema.Count == 0) return schema; // already allows anything
                if (schema.TryGetValue("type", out var t) && t is string typeName)
                {
                    // keywords such as "minimum" or "format" do not apply to null
                    var result = new JsonObject();
                    foreach (var entry in schema)
                    {
                        switch (entry.Key)
                        {
                            case "type":
                                result.Add("type", new JsonArray { typeName, "null" });
                                break;
                            case "enum" when entry.Value is JsonArray values:
                                result.Add("enum", new JsonArray(values) { null });
                                break;
                            default:
                                result.Add(entry.Key, entry.Value);
                                break;
                        }
                    }
                    return result;
                }
                return new JsonObject
                {
                    { "anyOf", new JsonArray { schema, new JsonObject { { "type", "null" } } } }
                };
            }

            // In draft-07 every keyword next to "$ref" is ignored, so annotations wrap a reference in allOf.
            static JsonObject WithAnnotations(JsonObject schema, JsonObject annotations)
            {
                if (annotations.Count == 0) return schema;
                JsonObject result;
                if (schema.ContainsKey("$ref"))
                {
                    result = new JsonObject { { "allOf", new JsonArray { schema } } };
                }
                else
                {
                    result = new JsonObject();
                    foreach (var entry in schema)
                    {
                        result.Add(entry.Key, entry.Value);
                    }
                }
                foreach (var entry in annotations)
                {
                    result[entry.Key] = entry.Value;
                }
                return result;
            }

            static bool IsTupleDefinition(Type definition) =>
                definition == typeof(Tuple<>) || definition == typeof(ValueTuple<>) ||
                definition == typeof(Tuple<,>) || definition == typeof(ValueTuple<,>) ||
                definition == typeof(Tuple<,,>) || definition == typeof(ValueTuple<,,>) ||
                definition == typeof(Tuple<,,,>) || definition == typeof(ValueTuple<,,,>) ||
                definition == typeof(Tuple<,,,,>) || definition == typeof(ValueTuple<,,,,>) ||
                definition == typeof(Tuple<,,,,,>) || definition == typeof(ValueTuple<,,,,,>) ||
                definition == typeof(Tuple<,,,,,,>) || definition == typeof(ValueTuple<,,,,,,>);

            [UnconditionalSuppressMessage("Trimming", "IL2070",
                Justification = "Enum field metadata is preserved through the source generator's static EnumAsStringFormatter<T> registrations.")]
            static FieldInfo[] GetEnumFields(Type type) =>
                type.GetFields(BindingFlags.Public | BindingFlags.Static);

            static JsonObject? PrimitiveSchemaOf(Type type)
            {
                if (type == typeof(string)) return new JsonObject { { "type", "string" } };
                if (type == typeof(bool)) return new JsonObject { { "type", "boolean" } };
                if (type == typeof(byte)) return Integer(byte.MinValue, byte.MaxValue);
                if (type == typeof(sbyte)) return Integer(sbyte.MinValue, sbyte.MaxValue);
                if (type == typeof(short)) return Integer(short.MinValue, short.MaxValue);
                if (type == typeof(ushort)) return Integer(ushort.MinValue, ushort.MaxValue);
                // char is written as its code point
                if (type == typeof(char)) return Integer(char.MinValue, char.MaxValue);
                if (type == typeof(uint)) return Integer(uint.MinValue, uint.MaxValue);
                if (type == typeof(int) ||
                    type == typeof(long) ||
                    type == typeof(nint) ||
                    type == typeof(System.Numerics.BigInteger))
                {
                    return new JsonObject { { "type", "integer" } };
                }
                if (type == typeof(ulong) || type == typeof(nuint))
                {
                    return new JsonObject { { "type", "integer" }, { "minimum", 0 } };
                }
                if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                {
                    return new JsonObject { { "type", "number" } };
                }
                if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
                {
                    return new JsonObject { { "type", "string" }, { "format", "date-time" } };
                }
                if (type == typeof(Guid)) return new JsonObject { { "type", "string" }, { "format", "uuid" } };
                if (type == typeof(Uri)) return new JsonObject { { "type", "string" }, { "format", "uri-reference" } };
                if (type == typeof(TimeSpan) ||
                    type == typeof(Version) ||
                    type == typeof(Type) ||
                    type == typeof(System.Numerics.Complex))
                {
                    return new JsonObject { { "type", "string" } };
                }
                // binary is written as a base64 string
                if (type == typeof(byte[]) ||
                    type == typeof(Memory<byte>) ||
                    type == typeof(ReadOnlyMemory<byte>) ||
                    type == typeof(ArraySegment<byte>) ||
                    type == typeof(System.Buffers.ReadOnlySequence<byte>))
                {
                    return new JsonObject { { "type", "string" }, { "contentEncoding", "base64" } };
                }
                if (type == typeof(BitArray))
                {
                    return new JsonObject { { "type", "string" }, { "pattern", "^[01]*$" } };
                }
                return null;
            }

            static JsonObject Integer(long min, long max) =>
                new() { { "type", "integer" }, { "minimum", min }, { "maximum", max } };
        }

        // Insertion-ordered JSON object
        sealed class JsonObject : IEnumerable<KeyValuePair<string, object?>>
        {
            readonly List<KeyValuePair<string, object?>> entries = new();

            public int Count => entries.Count;

            public object? this[string key]
            {
                set
                {
                    var index = entries.FindIndex(x => x.Key == key);
                    if (index >= 0)
                    {
                        entries[index] = new KeyValuePair<string, object?>(key, value);
                    }
                    else
                    {
                        entries.Add(new KeyValuePair<string, object?>(key, value));
                    }
                }
            }

            public void Add(string key, object? value) => entries.Add(new KeyValuePair<string, object?>(key, value));

            public bool ContainsKey(string key) => entries.Exists(x => x.Key == key);

            public bool TryGetValue(string key, out object? value)
            {
                var index = entries.FindIndex(x => x.Key == key);
                value = index >= 0 ? entries[index].Value : null;
                return index >= 0;
            }

            public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => entries.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        sealed class RawJson
        {
            public string Json { get; }
            public RawJson(string json) => Json = json.Trim();
        }

        sealed class JsonArray : List<object?>
        {
            public JsonArray()
            {
            }

            public JsonArray(IEnumerable<object?> items) : base(items)
            {
            }
        }

        static class JsonWriter
        {
            public static void Write(StringBuilder sb, object? value, int indent)
            {
                switch (value)
                {
                    case null:
                        sb.Append("null");
                        break;
                    case bool b:
                        sb.Append(b ? "true" : "false");
                        break;
                    case string s:
                        WriteString(sb, s);
                        break;
                    case char c:
                        WriteString(sb, c.ToString());
                        break;
                    case float f:
                        WriteNumber(sb, f.ToString("R", CultureInfo.InvariantCulture), double.IsNaN(f) || double.IsInfinity(f));
                        break;
                    case double d:
                        WriteNumber(sb, d.ToString("R", CultureInfo.InvariantCulture), double.IsNaN(d) || double.IsInfinity(d));
                        break;
                    case IFormattable number and (byte or sbyte or short or ushort or int or uint or long or ulong or decimal):
                        sb.Append(number.ToString(null, CultureInfo.InvariantCulture));
                        break;
                    case JsonObject obj:
                        WriteObject(sb, obj, indent);
                        break;
                    case JsonArray array:
                        WriteArray(sb, array, indent);
                        break;
                    case RawJson raw:
                        sb.Append(raw.Json);
                        break;
                    default:
                        WriteString(sb, Convert.ToString(value, CultureInfo.InvariantCulture) ?? "");
                        break;
                }
            }

            static void WriteNumber(StringBuilder sb, string value, bool nonFinite)
            {
                if (nonFinite)
                {
                    sb.Append("null");
                }
                else
                {
                    sb.Append(value);
                }
            }

            static void WriteObject(StringBuilder sb, JsonObject obj, int indent)
            {
                if (obj.Count == 0)
                {
                    sb.Append("{}");
                    return;
                }
                sb.Append('{');
                var first = true;
                foreach (var entry in obj)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append('\n');
                    sb.Append(' ', (indent + 1) * 2);
                    WriteString(sb, entry.Key);
                    sb.Append(": ");
                    Write(sb, entry.Value, indent + 1);
                }
                sb.Append('\n');
                sb.Append(' ', indent * 2);
                sb.Append('}');
            }

            static void WriteArray(StringBuilder sb, JsonArray array, int indent)
            {
                if (array.Count == 0)
                {
                    sb.Append("[]");
                    return;
                }
                // scalar-only arrays (e.g. enum values, type unions) are written inline
                if (array.TrueForAll(x => x is not JsonObject and not JsonArray and not RawJson))
                {
                    sb.Append('[');
                    for (var i = 0; i < array.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        Write(sb, array[i], indent);
                    }
                    sb.Append(']');
                    return;
                }
                sb.Append('[');
                for (var i = 0; i < array.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append('\n');
                    sb.Append(' ', (indent + 1) * 2);
                    Write(sb, array[i], indent + 1);
                }
                sb.Append('\n');
                sb.Append(' ', indent * 2);
                sb.Append(']');
            }

            static void WriteString(StringBuilder sb, string value)
            {
                sb.Append('"');
                foreach (var c in value)
                {
                    switch (c)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        default:
                            if (c < 0x20)
                            {
                                sb.Append("\\u");
                                sb.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                sb.Append(c);
                            }
                            break;
                    }
                }
                sb.Append('"');
            }
        }
    }
}
