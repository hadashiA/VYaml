#nullable enable
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class Int16Formatter : IYamlFormatter<short>
    {
        public static readonly Int16Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, short value, YamlSerializationContext context)
        {
            emitter.WriteInt32(value);
        }

        public short Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsInt32();
            parser.Read();
            return checked((short)result);
        }
    }

    public class NullableInt16Formatter : IYamlFormatter<short?>
    {
        public static readonly NullableInt16Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, short? value, YamlSerializationContext context)
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

        public short? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsInt32();
            parser.Read();
            return checked((short)result);
        }
    }
}
