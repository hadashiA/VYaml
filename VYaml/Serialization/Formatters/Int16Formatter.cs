using VYaml.Parser;

namespace VYaml.Serialization
{
    public class Int16Formatter : IYamlFormatter<short>
    {
        public static readonly Int16Formatter Instance = new();

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