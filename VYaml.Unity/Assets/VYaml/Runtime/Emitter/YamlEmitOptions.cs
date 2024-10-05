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
