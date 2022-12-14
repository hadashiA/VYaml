namespace VYaml.Parser
{
    public class Tag : ITokenContent
    {
        public string Handle { get; }
        public string Suffix { get; }

        public Tag(string handle, string suffix)
        {
            Handle = handle;
            Suffix = suffix;
        }

        public override string ToString() => $"{Handle} {Suffix}";
    }
}
