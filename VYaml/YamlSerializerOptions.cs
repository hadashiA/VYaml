using VYaml.Resolvers;

namespace VYaml
{
    public class YamlSerializerOptions
    {
        public static YamlSerializerOptions Standard => new(StandardResolver.Instance);

        public IYamlFormatterResolver Resolver { get; }
        public bool SupportAliasForDeserialization { get; set; } = true;

        public YamlSerializerOptions(IYamlFormatterResolver resolver)
        {
            Resolver = resolver;
        }
    }
}
