using VYaml.Parser;

namespace VYaml.Serialization
{
    public class ByteFormatter : IYamlFormatter<byte>
    {
        public static readonly ByteFormatter Instance = new();

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