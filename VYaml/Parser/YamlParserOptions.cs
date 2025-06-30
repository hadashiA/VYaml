namespace VYaml.Parser
{
    /// <summary>
    /// Options for controlling YAML parsing behavior.
    /// </summary>
    public class YamlParserOptions
    {
        public static readonly YamlParserOptions Default = new();

        /// <summary>
        /// Gets or sets whether to preserve comments during parsing.
        /// When enabled, the parser will emit Comment events for comments in the YAML document.
        /// Default is false for backward compatibility and performance.
        /// </summary>
        public bool PreserveComments { get; set; } = false;
        
        /// <summary>
        /// Gets or sets whether to strip leading whitespace from comment content.
        /// When enabled, leading whitespace after the # character is removed from comment text.
        /// This provides user-friendly comment content while allowing round-trip fidelity when disabled.
        /// Default is true for user convenience.
        /// </summary>
        public bool StripLeadingWhitespace { get; set; } = true;
    }
}