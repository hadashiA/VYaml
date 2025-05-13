using System;
using System.Runtime.CompilerServices;

namespace VYaml.SourceGenerator
{
    public interface INamingConventionMutator
    {
        bool TryMutate(ReadOnlySpan<char> source, Span<char> destination, out int written);
        bool TryMutate(ReadOnlySpan<byte> sourceUtf8, Span<byte> destinationUtf8, out int written);
    }

    static class NamingConventionMutator
    {
        public static readonly INamingConventionMutator UpperCamelCase = new UpperCamelCaseMutator();
        public static readonly INamingConventionMutator LowerCamelCase = new LowerCamelCaseMutator();
        public static readonly INamingConventionMutator SnakeCase = new NotationCaseMutator('_');
        public static readonly INamingConventionMutator KebabCase = new NotationCaseMutator('-');

        public static string Mutate(string source, NamingConvention namingConvention)
        {
            var mutator = Of(namingConvention);
            Span<char> destination = stackalloc char[source.Length * 2];
            int written;
            while (!mutator.TryMutate(source.AsSpan(), destination, out written))
            {
                // ReSharper disable once StackAllocInsideLoop
                destination = stackalloc char[destination.Length * 2];
            }
            return destination.Slice(0, written).ToString();
        }

        public static INamingConventionMutator Of(NamingConvention namingConvention) => namingConvention switch
        {
            NamingConvention.LowerCamelCase => LowerCamelCase,
            NamingConvention.UpperCamelCase => UpperCamelCase,
            NamingConvention.SnakeCase => SnakeCase,
            NamingConvention.KebabCase => KebabCase,
            _ => throw new ArgumentOutOfRangeException(nameof(namingConvention), namingConvention, null)
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsUpper(byte c) => c - (byte)'A' < 26;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsLower(byte c) => c - (byte)'a' < 26;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte ToUpper(byte ch)
        {
            if (IsUpper(ch))
            {
                return (byte)(ch - 0x20);
            }

            return ch;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte ToLower(byte ch)
        {
            if (IsUpper(ch))
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
