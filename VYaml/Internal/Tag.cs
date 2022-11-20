namespace VYaml.Internal
{
    class Tag : ITokenContent
    {
        public Scalar Handle { get; }
        public Scalar Suffix { get; }

        public Tag(Scalar handle, Scalar suffix)
        {
            Handle = handle;
            Suffix = suffix;
        }
    }
}
