namespace VYaml.Internal
{
    readonly struct Tag : ITokenContent
    {
        public readonly Scalar Handle;
        public readonly Scalar Suffix;

        public Tag(Scalar handle, Scalar suffix)
        {
            Handle = handle;
            Suffix = suffix;
        }
    }
}
