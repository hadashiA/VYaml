using System;
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