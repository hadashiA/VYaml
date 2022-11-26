using System;

namespace VYaml
{
    public readonly ref struct Tag
    {
        public readonly ReadOnlySpan<byte> Handle;
        public readonly ReadOnlySpan<byte> Suffix;

        public Tag(ReadOnlySpan<byte> handle, ReadOnlySpan<byte> suffix)
        {
            Handle = handle;
            Suffix = suffix;
        }


    }
}
