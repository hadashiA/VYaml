using System;
using System.Diagnostics.CodeAnalysis;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class TypeFormatter : IYamlFormatter<Type?>
    {
        public static readonly TypeFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Type? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
            }
            else
            {
                emitter.WriteString(value.AssemblyQualifiedName!);
            }
        }

        // Round-tripping System.Type by name is inherently unanalyzable: the payload decides
        // which type is loaded. On trimmed / Native AOT apps the application must ensure the
        // named types are preserved (e.g. by referencing them statically or with
        // [DynamicDependency]); an unpreserved name throws TypeLoadException here.
        [UnconditionalSuppressMessage("Trimming", "IL2057",
            Justification = "Type-by-name deserialization is inherently dynamic; the application is responsible for preserving the named types, and an unpreserved name fails loudly with TypeLoadException.")]
        public Type? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }
            return Type.GetType(parser.ReadScalarAsString()!, throwOnError: true);
        }
    }
}
