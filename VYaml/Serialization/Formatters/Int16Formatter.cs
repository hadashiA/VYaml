using VYaml.Parser;

namespace VYaml.Serialization
{
    public class Int16Formatter : IYamlFormatter<short>
    {
        public static readonly Int16Formatter Instance = new();

        public short Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsInt16();
            parser.Read();
            return result;
        }
    }

    public class NullableInt16Formatter : IYamlFormatter<short?>
    {
        public static readonly NullableInt32Formatter Instance = new();

        public short? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            var result = parser.GetScalarAsInt16();
            parser.Read();
            return result;
        }
    }
}