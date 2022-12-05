using VYaml.Parser;

namespace VYaml.Serialization
{
    public class SByteFormatter : IYamlFormatter<sbyte>
    {
        public static readonly SByteFormatter Instance = new();

        public sbyte Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsInt32();
            parser.Read();
            return checked((sbyte)result);
        }
    }

    public class NullableSByteFormatter : IYamlFormatter<sbyte?>
    {
        public static readonly NullableSByteFormatter Instance = new();

        public sbyte? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsInt32();
            parser.Read();
            return checked((sbyte)result);
        }
    }
}