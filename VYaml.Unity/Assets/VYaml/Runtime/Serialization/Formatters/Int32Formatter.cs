using VYaml.Parser;

namespace VYaml.Serialization
{
    public class Int32Formatter : IYamlFormatter<int>
    {
        public static readonly Int32Formatter Instance = new();

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

        public int? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            var result = parser.GetScalarAsInt32();
            parser.Read();
            return result;
        }
    }
}