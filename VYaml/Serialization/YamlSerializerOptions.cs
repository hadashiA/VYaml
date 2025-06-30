using VYaml.Annotations;
using VYaml.Emitter;

namespace VYaml.Serialization
{
    /// <summary>
    /// Options for controlling YAML serialization behavior.
    /// </summary>
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
        
        /// <summary>
        /// Gets or sets the default ignore condition for properties during serialization.
        /// </summary>
        public YamlIgnoreCondition DefaultIgnoreCondition { get; set; } = YamlIgnoreCondition.Never;
    }
}
