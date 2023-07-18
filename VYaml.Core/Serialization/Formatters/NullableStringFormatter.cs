#nullable enable
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class NullableStringFormatter : IYamlFormatter<string?>
    {
        public static readonly NullableStringFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, string? value, YamlSerializationContext context)
        {
            if (value == null)
            {
                emitter.WriteNull();
            }
            else
            {
                emitter.WriteString(value);
            }
        }

        public string? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return null;
            }
            return parser.ReadScalarAsString();
        }
    }
}

