namespace VYaml.Serialization
{
    public class YamlSerializerOptions
    {
        public static YamlSerializerOptions Standard => new()
        {
            Resolver = StandardResolver.Instance
        };

        public IYamlFormatterResolver Resolver { get; set; } = null!;
        public bool SupportAliasForDeserialization { get; set; } = true;
    }
}
