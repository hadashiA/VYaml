#nullable enable
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class Int32Formatter : IYamlFormatter<int>
    {
        public static readonly Int32Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int value, YamlSerializationContext context)
        {
            emitter.WriteInt32(value);
        }

        public int Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsInt32();
            parser.Read();
            return result;
        }
    }

    public class NullableInt32Formatter : IYamlFormatter<int?>
    {
        public static readonly NullableInt32Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int? value, YamlSerializationContext context)
        {
            if (value.HasValue)
            {
                emitter.WriteInt32(value.Value);
            }
            else
            {
                emitter.WriteNull();
            }
        }

        public int? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsInt32();
            parser.Read();
            return result;
        }
    }
}
