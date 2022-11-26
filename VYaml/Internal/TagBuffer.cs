namespace VYaml.Internal
{
    class TagBuffer : ITokenContent
    {
        public Scalar Handle { get; }
        public Scalar Suffix { get; }

        public TagBuffer(Scalar handle, Scalar suffix)
        {
            Handle = handle;
            Suffix = suffix;
        }

        public override string ToString() => $"{Handle} {Suffix}";
    }
}
