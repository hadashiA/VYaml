using VYaml.Parser;

namespace VYaml.Serialization
{
    public class NullableStringFormatter : IYamlFormatter<string>
    {
        public static readonly NullableStringFormatter Instance = new();

        public string Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsString();
            parser.Read();
            return result;
        }
    }
}
