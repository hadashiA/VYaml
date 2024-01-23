#nullable enable
namespace VYaml.Parser
{
    readonly struct Token
    {
        public readonly TokenType Type;
        public readonly ITokenContent? Content;

        public Token(TokenType type, ITokenContent? content = null)
        {
            Type = type;
            Content = content;
        }

        public override string ToString() => $"{Type} \"{Content}\"";
    }
}

