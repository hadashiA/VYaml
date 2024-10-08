using System;
using VYaml.Annotations;

namespace VYaml.Serialization
{
    public interface INamingConventionMutator
    {
        bool TryMutate(ReadOnlySpan<char> source, Span<char> destination, out int written);
        bool TryMutate(ReadOnlySpan<byte> sourceUtf8, Span<byte> destinationUtf8, out int written);
    }

    public static class NamingConventionMutator
    {
        public static readonly INamingConventionMutator UpperCamelCase = new UpperCamelCaseMutator();
        public static readonly INamingConventionMutator LowerCamelCase = new LowerCamelCaseMutator();
        public static readonly INamingConventionMutator SnakeCase = new NotationCaseMutator('_');
        public static readonly INamingConventionMutator KebabCase = new NotationCaseMutator('-');

        [ThreadStatic]
        static char[]? ThreadStaticBuffer;

        [ThreadStatic]
        static byte[]? ThreadStaticBufferUtf8;

        static byte[] GetThreadStaticBufferUtf8(int sizeHint)
        {
            if (ThreadStaticBufferUtf8 == null || ThreadStaticBufferUtf8.Length < sizeHint)
            {
                ThreadStaticBufferUtf8 = new byte[Math.Max(64, sizeHint)];
            }
            return ThreadStaticBufferUtf8;
        }

        static char[] GetThreadStaticBuffer(int sizeHint)
        {
            if (ThreadStaticBuffer == null || ThreadStaticBuffer.Length < sizeHint)
            {
                ThreadStaticBuffer = new char[Math.Max(64, sizeHint)];
            }
            return ThreadStaticBuffer;
        }

        public static void MutateToThreadStaticBuffer(
            ReadOnlySpan<char> source,
            NamingConvention convention,
            out char[] threadStaticBuffer, out int written)
        {
            var mutator = Of(convention);
            threadStaticBuffer = GetThreadStaticBuffer(source.Length * 2);
            while (!mutator.TryMutate(source, threadStaticBuffer, out written))
            {
                // ReSharper disable once StackAllocInsideLoop
                threadStaticBuffer = GetThreadStaticBuffer(threadStaticBuffer.Length * 2);
            }
        }

        public static void MutateToThreadStaticBufferUtf8(
            ReadOnlySpan<byte> sourceUtf8,
            NamingConvention convention,
            out byte[] threadStaticBuffer, out int written)
        {
            var mutator = Of(convention);
            threadStaticBuffer = GetThreadStaticBufferUtf8(sourceUtf8.Length * 2);
            while (!mutator.TryMutate(sourceUtf8, threadStaticBuffer, out written))
            {
                // ReSharper disable once StackAllocInsideLoop
                threadStaticBuffer = GetThreadStaticBufferUtf8(threadStaticBuffer.Length * 2);
            }
        }

        public static string Mutate(string source, NamingConvention namingConvention)
        {
            var mutator = Of(namingConvention);
            Span<char> destination = stackalloc char[source.Length * 2];
            while (!mutator.TryMutate(source.AsSpan(), destination, out var written))
            {
                // ReSharper disable once StackAllocInsideLoop
                destination = stackalloc char[destination.Length * 2];
            }
            return destination.ToString();
        }

        public static INamingConventionMutator Of(NamingConvention namingConvention) => namingConvention switch
        {
            NamingConvention.LowerCamelCase => LowerCamelCase,
            NamingConvention.UpperCamelCase => UpperCamelCase,
            NamingConvention.SnakeCase => SnakeCase,
            NamingConvention.KebabCase => KebabCase,
            _ => throw new ArgumentOutOfRangeException(nameof(namingConvention), namingConvention, null)
        };

        internal static bool IsUpper(byte ch) => ch >= 'A' && ch <= 'Z';
        internal static bool IsLower(byte ch) => ch >= 'a' && ch <= 'z';

        internal static byte ToUpper(byte ch)
        {
            if (ch >= 'a' && ch <= 'z')
            {
                return (byte)(ch - 0x20);
            }

            return ch;
        }

        internal static byte ToLower(byte ch)
        {
            if (ch >= 'A' && ch <= 'Z')
            {
                return (byte)(ch + 0x20);
            }

            return ch;
        }
    }

    class UpperCamelCaseMutator : INamingConventionMutator
    {
        public bool TryMutate(ReadOnlySpan<byte> source, Span<byte> destination, out int written)
        {
            if (source.Length > destination.Length)
            {
                written = default;
                return false;
            }

            var offset = 0;
            destination[offset++] = NamingConventionMutator.ToUpper(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var ch = source[i];
                if (i > 1 && ch is (byte)'_' or (byte)'-')
                {
                    i++; // skip separator
                    if (i <= source.Length - 1)
                    {
                        destination[offset++] = NamingConventionMutator.ToUpper(source[i]);
                    }
                }
                else
                {
                    destination[offset++] = ch;
                }
            }

            written = offset;
            return true;
        }

        public bool TryMutate(ReadOnlySpan<char> source, Span<char> destination, out int written)
        {
            if (source.Length > destination.Length)
            {
                written = default;
                return false;
            }

            var offset = 0;
            destination[offset++] = char.ToUpperInvariant(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var ch = source[i];
                if (i > 1 && ch is '_' or '-')
                {
                    i++; // skip separator
                    if (i <= source.Length - 1)
                    {
                        destination[offset++] = char.ToUpperInvariant(source[i]);
                    }
                }
                else
                {
                    destination[offset++] = ch;
                }
            }

            written = offset;
            return true;
        }
    }

    class LowerCamelCaseMutator : INamingConventionMutator
    {
        public bool TryMutate(ReadOnlySpan<byte> source, Span<byte> destination, out int written)
        {
            if (source.Length > destination.Length)
            {
                written = default;
                return false;
            }

            var offset = 0;
            destination[offset++] = NamingConventionMutator.ToLower(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var ch = source[i];
                if (i > 1 && ch is (byte)'_' or (byte)'-')
                {
                    i++; // skip separator
                    if (i <= source.Length - 1)
                    {
                        destination[offset++] = NamingConventionMutator.ToUpper(source[i]);
                    }
                }
                else
                {
                    destination[offset++] = ch;
                }
            }

            written = offset;
            return true;
        }

        public bool TryMutate(ReadOnlySpan<char> source, Span<char> destination, out int written)
        {
            if (source.Length > destination.Length)
            {
                written = default;
                return false;
            }

            var offset = 0;
            destination[offset++] = char.ToLowerInvariant(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var ch = source[i];
                if (i > 1 && ch is '_' or '-')
                {
                    i++; // skip separator
                    if (i <= source.Length - 1)
                    {
                        destination[offset++] = char.ToUpperInvariant(source[i]);
                    }
                }
                else
                {
                    destination[offset++] = ch;
                }
            }

            written = offset;
            return true;
        }
    }

    class NotationCaseMutator : INamingConventionMutator
    {
        readonly char separator;

        public NotationCaseMutator(char separator)
        {
            this.separator = separator;
        }

        public bool TryMutate(ReadOnlySpan<char> source, Span<char> destination, out int written)
        {
            if (source.Length <= 0)
            {
                written = default;
                return true;
            }

            var offset = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (offset >= destination.Length - 1)
                {
                    written = default;
                    return false;
                }

                var ch = source[i];
                if (char.IsUpper(ch))
                {
                    if (i > 0 && source[i - 1] is not ('_' or '-'))
                    {
                        destination[offset++] = separator;
                    }
                    if (offset >= destination.Length - 1)
                    {
                        written = default;
                        return false;
                    }
                    destination[offset++] = char.ToLowerInvariant(ch);
                }
                else if (ch is '_' or '-')
                {
                    destination[offset++] = separator;
                }
                else
                {
                    destination[offset++] = ch;
                }
            }

            written = offset;
            return true;
        }

        public bool TryMutate(ReadOnlySpan<byte> source, Span<byte> destination, out int written)
        {
            if (source.Length <= 0)
            {
                written = default;
                return true;
            }

            var s = (byte)separator;
            var offset = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (offset >= destination.Length - 1)
                {
                    written = default;
                    return false;
                }

                var ch = source[i];
                if (NamingConventionMutator.IsUpper(ch))
                {
                    if (i > 0 && source[i - 1] is not ((byte)'_' or (byte)'-'))
                    {
                        destination[offset++] = s;
                    }
                    if (offset >= destination.Length - 1)
                    {
                        written = default;
                        return false;
                    }
                    destination[offset++] = NamingConventionMutator.ToLower(ch);
                }
                else if (ch is (byte)'_' or (byte)'-')
                {
                    destination[offset++] = s;
                }
                else
                {
                    destination[offset++] = ch;
                }
            }

            written = offset;
            return true;
        }
    }
}
