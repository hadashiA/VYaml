#nullable enable
namespace VYaml.Emitter
{
    public enum ScalarStyle
    {
        Any,
        Plain,
        SingleQuoted,
        DoubleQuoted,
        Literal,
        Folded,
    }

    public enum SequenceStyle
    {
        Block,
        Flow,
    }

    public enum MappingStyle
    {
        Block,
        Flow,
    }

    public class YamlEmitOptions
    {
        public static readonly YamlEmitOptions Default = new();

        public int IndentWidth { get; set; } = 2;
        
        /// <summary>
        /// Gets or sets whether to preserve comments when emitting YAML.
        /// When enabled, the emitter will write Comment events.
        /// Default is false for backward compatibility.
        /// </summary>
        public bool PreserveComments { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to add a leading space after the # character in comments.
        /// When enabled, a space is automatically added after # for non-empty comments.
        /// This provides standard YAML formatting while allowing precise control when disabled.
        /// Default is true for user convenience and standard formatting.
        /// </summary>
        public bool AddLeadingSpace { get; set; } = true;

        private ScalarStyle stringQuoteStyle = ScalarStyle.DoubleQuoted;

        public ScalarStyle StringQuoteStyle
        {
            get => stringQuoteStyle;
            set
            {
                if (value != ScalarStyle.SingleQuoted &&
                    value != ScalarStyle.DoubleQuoted)
                {
                    throw new System.InvalidOperationException("Invalid scalar style");
                }

                stringQuoteStyle = value;
            }
        }
    }
}
