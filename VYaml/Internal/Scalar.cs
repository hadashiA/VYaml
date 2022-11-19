using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace VYaml.Internal
{
    class ScalarPool
    {
        // readonly ConcurrentQueue<Scalar> queue = new();
        readonly ExpandBuffer<Scalar> queue = new(32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scalar Rent()
        {
            return queue.TryPop(out var scalar)
                ? scalar
                : new Scalar(2048);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Scalar scalar)
        {
            scalar.Clear();
            queue.Add(scalar);
        }
    }

    class Scalar
    {
        public static readonly Scalar Null = new(new []
        {
            YamlCodes.NullAlias
        });

        public int Length => buffer.Length;

        readonly ExpandBuffer<byte> buffer;

        public Scalar(int capacity)
        {
            buffer = new ExpandBuffer<byte>(capacity);
        }

        public Scalar(ReadOnlySpan<byte> content)
        {
            buffer = new ExpandBuffer<byte>(content.Length);
            Write(content);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan() => buffer.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan(int start, int length) => buffer.AsSpan(start, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte code)
        {
            buffer.Add(code);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(LineBreakState lineBreak)
        {
            switch (lineBreak)
            {
                case LineBreakState.None:
                    break;
                case LineBreakState.Lf:
                    buffer.Add(YamlCodes.Lf);
                    break;
                case LineBreakState.CrLf:
                    buffer.Add(YamlCodes.Cr);
                    buffer.Add(YamlCodes.Lf);
                    break;
                case LineBreakState.Cr:
                    buffer.Add(YamlCodes.Cr);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lineBreak), lineBreak, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> codes)
        {
            buffer.Grow(buffer.Length + codes.Length);
            codes.CopyTo(buffer.AsSpan(buffer.Length, codes.Length));
            buffer.Length += codes.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUnicodeCodepoint(int codepoint)
        {
            Span<char> chars = stackalloc char[] { (char)codepoint };
            var utf8ByteCount = StringEncoding.Utf8.GetByteCount(chars);
            Span<byte> utf8Bytes = stackalloc byte[utf8ByteCount];
            StringEncoding.Utf8.GetBytes(chars, utf8Bytes);
            Write(utf8Bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            buffer.Clear();
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// null | Null | NULL | ~
        /// </remarks>
        /// <see href="https://yaml.org/type/null.html"/>
        public bool IsNull()
        {
            var span = buffer.AsSpan();
            switch (span.Length)
            {
                case 1 when span[0] == YamlCodes.NullAlias:
                case 4 when span.SequenceEqual(YamlCodes.Null0) ||
                            span.SequenceEqual(YamlCodes.Null1) ||
                            span.SequenceEqual(YamlCodes.Null2):
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// tag:yaml.org,2002:bool
        /// true | True | TRUE | false | False | FALSE
        /// </remarks>
        /// <see href="https://yaml.org/type/bool.html" />
        public bool TryGetBool(out bool value)
        {
            var span = buffer.AsSpan();
            switch (span.Length)
            {
                case 4 when span.SequenceEqual(YamlCodes.True0) ||
                            span.SequenceEqual(YamlCodes.True1) ||
                            span.SequenceEqual(YamlCodes.True2):
                    value = true;
                    return true;
                case 5 when span.SequenceEqual(YamlCodes.False0) ||
                            span.SequenceEqual(YamlCodes.False1) ||
                            span.SequenceEqual(YamlCodes.False2):
                    value = false;
                    return true;
            }
            value = default;
            return false;
        }

        public bool TryGetInt64(out long value)
        {
            var span = buffer.AsSpan();
            return Utf8Parser.TryParse(AsSpan(), out value, out var bytesConsumed) &&
                   bytesConsumed == span.Length;
        }

        public bool TryGetInt32(out int value)
        {
            var span = buffer.AsSpan();
            return Utf8Parser.TryParse(span, out value, out var bytesConsumed) &&
                   bytesConsumed == span.Length;
        }

        public bool TryGetDouble(out double value)
        {
            var span = buffer.AsSpan();
            return Utf8Parser.TryParse(span, out value, out var bytesConsumed) &&
                   bytesConsumed == span.Length;
        }

        public override string ToString()
        {
            return StringEncoding.Utf8.GetString(buffer.AsSpan());
        }

        public bool SequenceEqual(Scalar other)
        {
            return buffer.AsSpan().SequenceEqual(other.buffer.AsSpan());
        }

        public bool SequenceEqual(ReadOnlySpan<byte> span)
        {
            return buffer.AsSpan().SequenceEqual(span);
        }
    }
}
