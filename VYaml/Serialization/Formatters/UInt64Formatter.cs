using VYaml.Parser;

namespace VYaml.Serialization
{
    public class UInt64Formatter : IYamlFormatter<ulong>
    {
        public static readonly UInt64Formatter Instance = new();

        public ulong Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsUInt64();
            parser.Read();
            return result;
        }
    }

    public class NullableUInt64Formatter : IYamlFormatter<ulong?>
    {
        public static readonly NullableUInt64Formatter Instance = new();

        public ulong? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsUInt64();
            parser.Read();
            return result;
        }
    }
}