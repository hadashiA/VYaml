using VYaml.Parser;

namespace VYaml.Serialization
{
    public class UInt32Formatter : IYamlFormatter<uint>
    {
        public static readonly UInt32Formatter Instance = new();

        public uint Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsUInt32();
            parser.Read();
            return result;
        }
    }

    public class NullableUInt32Formatter : IYamlFormatter<uint?>
    {
        public static readonly NullableUInt32Formatter Instance = new();

        public uint? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsUInt32();
            parser.Read();
            return result;
        }
    }
}