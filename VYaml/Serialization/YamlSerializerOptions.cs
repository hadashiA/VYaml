using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;

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
        public YamlIgnoreCondition DefaultIgnoreCondition { get; set; } = YamlIgnoreCondition.Never;

        /// <summary>
        /// How keys that do not match any member of a [YamlObject] type are handled when deserializing.
        /// </summary>
        public YamlUnmappedMemberHandling UnmappedMemberHandling { get; set; } = YamlUnmappedMemberHandling.Skip;

        /// <summary>
        /// The maximum nesting depth allowed when deserializing. Applied to the parser created by <see cref="YamlSerializer"/>;
        /// a <see cref="VYaml.Parser.YamlParser"/> passed by ref keeps its own <see cref="VYaml.Parser.YamlParser.MaxDepth"/>.
        /// </summary>
        public int MaxDepth { get; set; } = YamlParser.DefaultMaxDepth;
    }
}
