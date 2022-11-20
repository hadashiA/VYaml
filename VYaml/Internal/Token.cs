namespace VYaml.Internal
{
    interface ITokenContent
    {
    }

    readonly struct Token
    {
        public readonly TokenType Type;
        public readonly Marker Start;
        public readonly Scalar? Scalar;

        public Token(
            TokenType type,
            in Marker start,
            Scalar? scalar = null)
        {
            Type = type;
            Start = start;
            Scalar = scalar;
            // Tag = tag;
        }

        public override string ToString() => $"{Type} \"{Scalar}\"";
    }
}
