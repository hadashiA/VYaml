#nullable enable
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class ByteFormatter : IYamlFormatter<byte>
    {
        public static readonly ByteFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, byte value, YamlSerializationContext context)
        {
            emitter.WriteInt32(value);
        }

        public byte Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsUInt32();
            parser.Read();
            return checked((byte)result);
        }
    }

    public class NullableByteFormatter : IYamlFormatter<byte?>
    {
        public static readonly NullableByteFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, byte? value, YamlSerializationContext context)
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

        public byte? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsUInt32();
            parser.Read();
            return checked((byte)result);
        }
    }
}
