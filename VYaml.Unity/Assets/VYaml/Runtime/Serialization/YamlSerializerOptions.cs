using VYaml.Annotations;
using VYaml.Emitter;

namespace VYaml.Serialization
{
    public class YamlSerializerOptions
    {
        public const NamingConvention DefaultNamingConvention = NamingConvention.LowerCamelCase;

        public static YamlSerializerOptions Standard => new()
        {
            Resolver = StandardResolver.Instance
        };

        public IYamlFormatterResolver Resolver { get; set; } = StandardResolver.Instance;
        public NamingConvention NamingConvention { get; set; } = DefaultNamingConvention;
        public YamlEmitOptions EmitOptions { get; set; } = new();
    }
}
