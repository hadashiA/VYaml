using VYaml.Parser;

namespace VYaml.Serialization
{
    public class CharFormatter : IYamlFormatter<char>
    {
        public static readonly CharFormatter Instance = new();

        public char Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsUInt32();
            parser.Read();
            return checked((char)result);
        }
    }

    public class NullableCharFormatter : IYamlFormatter<char?>
    {
        public static readonly NullableCharFormatter Instance = new();

        public char? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsUInt32();
            parser.Read();
            return checked((char)result);
        }
    }
}