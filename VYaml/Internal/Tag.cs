namespace VYaml.Internal
{
    class Tag
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
