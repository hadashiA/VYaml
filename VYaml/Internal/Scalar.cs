using System;
using System.Buffers.Text;
using System.Collections.Concurrent;

namespace VYaml.Internal
{
    class ScalarPool
    {
        public static readonly ScalarPool Shared = new();

        readonly ConcurrentQueue<Scalar> queue = new();

        public Scalar Rent()
        {
            return queue.TryDequeue(out var scalar)
                ? scalar
                : new Scalar(2048);
        }

        public void Return(Scalar scalar)
        {
            scalar.Clear();
            queue.Enqueue(scalar);
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

        public ReadOnlySpan<byte> AsSpan() => buffer.AsSpan();

        public void Write(byte code)
        {
            buffer.Add(code);
        }

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

        public void Write(ReadOnlySpan<byte> codes)
        {
            var sizeBefore = buffer.Length;
            buffer.Grow(sizeBefore + codes.Length);
            codes.CopyTo(buffer.AsSpan(sizeBefore, codes.Length));
            buffer.Length += codes.Length;
        }

        public void WriteUnicodeCodepoint(int codepoint)
        {
            Span<char> chars = stackalloc char[] { (char)codepoint };
            var utf8ByteCount = StringEncoding.Utf8.GetByteCount(chars);
            Span<byte> utf8Bytes = stackalloc byte[utf8ByteCount];
            StringEncoding.Utf8.GetBytes(chars, utf8Bytes);
            Write(utf8Bytes);
        }

        public void Clear()
        {
            buffer.Clear();
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// null | Null | NULL | ~
        /// </remarks>
        /// <returns></returns>
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
