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

    public class YamlEmitOptions
    {
        public static readonly YamlEmitOptions Default = new();

        public int IndentWidth { get; set; } = 2;
    }
}
