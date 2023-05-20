#nullable enable
using System;

namespace VYaml.Internal
{
    public static class ByteSequenceHash
    {
        public static int GetHashCode(ReadOnlySpan<byte> span)
        {
            uint hash = 2166136261;
            foreach (var x in span)
            {
                hash = unchecked((x ^ hash) * 16777619);
            }
            return unchecked((int)hash);
        }
    }
}
