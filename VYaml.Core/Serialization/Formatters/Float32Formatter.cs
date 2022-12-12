using VYaml.Parser;

namespace VYaml.Serialization
{
    public class Float32Formatter : IYamlFormatter<float>
    {
        public static readonly Float32Formatter Instance = new();

        public float Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsFloat();
            parser.Read();
            return result;
        }
    }

    public class NullableFloat32Formatter : IYamlFormatter<float?>
    {
        public static readonly NullableFloat32Formatter Instance = new();

        public float? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsFloat();
            parser.Read();
            return result;
        }
    }
}