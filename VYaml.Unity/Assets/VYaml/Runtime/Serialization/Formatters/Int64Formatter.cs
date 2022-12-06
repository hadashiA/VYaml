using VYaml.Parser;

namespace VYaml.Serialization
{
    public class Int64Formatter : IYamlFormatter<long>
    {
        public static readonly Int64Formatter Instance = new();

        public long Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsInt64();
            parser.Read();
            return result;
        }
    }

    public class NullableInt64Formatter : IYamlFormatter<long?>
    {
        public static readonly NullableInt64Formatter Instance = new();

        public long? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsInt64();
            parser.Read();
            return result;
        }
    }
}