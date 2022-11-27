using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace VYaml.Internal
{
    class ScalarPool : IDisposable
    {
        readonly ExpandBuffer<Scalar> queue = new(32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scalar Rent()
        {
            return queue.TryPop(out var scalar)
                ? scalar
                : new Scalar(256);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Scalar scalar)
        {
            scalar.Clear();
            queue.Add(scalar);
        }

        public void Dispose()
        {
            foreach (var scalar in queue.AsSpan())
            {
                scalar.Dispose();
            }
        }
    }

    class Scalar : ITokenContent, IDisposable
    {
        const int MinimumGrow = 4;
        const int GrowFactor = 200;

        public static readonly Scalar Null = new(0);

        public int Length { get; private set; }

        byte[] buffer;
        bool disposed;

        public Scalar(int capacity)
        {
            buffer = ArrayPool<byte>.Shared.Rent(capacity);
        }

        public Scalar(ReadOnlySpan<byte> content)
        {
            buffer = ArrayPool<byte>.Shared.Rent(content.Length);
            Write(content);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan() => buffer.AsSpan(0, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan(int start, int length) => buffer.AsSpan(start, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte code)
        {
            if (Length == buffer.Length)
            {
                Grow();
            }

            buffer[Length++] = code;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(LineBreakState lineBreak)
        {
            switch (lineBreak)
            {
                case LineBreakState.None:
                    break;
                case LineBreakState.Lf:
                    Write(YamlCodes.Lf);
                    break;
                case LineBreakState.CrLf:
                    Write(YamlCodes.Cr);
                    Write(YamlCodes.Lf);
                    break;
                case LineBreakState.Cr:
                    Write(YamlCodes.Cr);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lineBreak), lineBreak, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> codes)
        {
            Grow(Length + codes.Length);
            codes.CopyTo(buffer.AsSpan(Length, codes.Length));
            Length += codes.Length;
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
            Length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (disposed) return;
            ArrayPool<byte>.Shared.Return(buffer);
            disposed = true;
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// null | Null | NULL | ~
        /// </remarks>
        /// <see href="https://yaml.org/type/null.html"/>
        public bool IsNull()
        {
            var span = AsSpan();
            switch (span.Length)
            {
                case 0:
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
        /// true|True|TRUE|false|False|FALSE
        /// </remarks>
        /// <see href="https://yaml.org/type/bool.html" />
        public bool TryGetBool(out bool value)
        {
            var span = AsSpan();
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

        /// <summary>
        /// </summary>
        /// <see href="https://yaml.org/type/int.html"/>
        public bool TryGetInt64(out long value)
        {
            var span = buffer.AsSpan();
            if (Utf8Parser.TryParse(span, out value, out var bytesConsumed) &&
                bytesConsumed == span.Length)
            {
                return true;
            }

            if (span.Length > YamlCodes.HexPrefix.Length && span.StartsWith(YamlCodes.HexPrefix))
            {
                var slice = span[YamlCodes.HexPrefix.Length..];
                return Utf8Parser.TryParse(slice, out value, out var bytesConsumedHex, 'x') &&
                       bytesConsumedHex == slice.Length;
            }
            if (span.Length > YamlCodes.HexPrefixNegative.Length && span.StartsWith(YamlCodes.HexPrefixNegative))
            {
                var slice = span[YamlCodes.HexPrefixNegative.Length..];
                if (Utf8Parser.TryParse(slice, out value, out var bytesConsumedHex, 'x') && bytesConsumedHex == slice.Length)
                {
                    value = -value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// </summary>
        /// <see href="https://yaml.org/type/int.html"/>
        public bool TryGetInt32(out int value)
        {
            var span = AsSpan();

            if (Utf8Parser.TryParse(span, out value, out var bytesConsumed) &&
                bytesConsumed == span.Length)
            {
                return true;
            }

            if (span.Length > YamlCodes.HexPrefix.Length && span.StartsWith(YamlCodes.HexPrefix))
            {
                var slice = span[YamlCodes.HexPrefix.Length..];
                return Utf8Parser.TryParse(slice, out value, out var bytesConsumedHex, 'x') &&
                       bytesConsumedHex == slice.Length;
            }
            if (span.Length > YamlCodes.HexPrefixNegative.Length && span.StartsWith(YamlCodes.HexPrefixNegative))
            {
                var slice = span[YamlCodes.HexPrefixNegative.Length..];
                if (Utf8Parser.TryParse(slice, out value, out var bytesConsumedHex, 'x') && bytesConsumedHex == slice.Length)
                {
                    value = -value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// </summary>
        /// <see href="https://yaml.org/type/float.html"/>
        public bool TryGetDouble(out double value)
        {
            var span = AsSpan();
            if (Utf8Parser.TryParse(span, out value, out var bytesConsumed) &&
                bytesConsumed == span.Length)
            {
                return true;
            }

            switch (span.Length)
            {
                case 4:
                    if (span.SequenceEqual(YamlCodes.Inf0) ||
                        span.SequenceEqual(YamlCodes.Inf1) ||
                        span.SequenceEqual(YamlCodes.Inf2))
                    {
                        value = double.PositiveInfinity;
                        return true;
                    }

                    if (span.SequenceEqual(YamlCodes.Nan0) ||
                        span.SequenceEqual(YamlCodes.Nan1) ||
                        span.SequenceEqual(YamlCodes.Nan2))
                    {
                        value = double.NaN;
                        return true;
                    }
                    break;
                case 5:
                    if (span.SequenceEqual(YamlCodes.Inf3) ||
                        span.SequenceEqual(YamlCodes.Inf4) ||
                        span.SequenceEqual(YamlCodes.Inf5))
                    {
                        value = double.PositiveInfinity;
                        return true;
                    }
                    if (span.SequenceEqual(YamlCodes.NegInf0) ||
                        span.SequenceEqual(YamlCodes.NegInf1) ||
                        span.SequenceEqual(YamlCodes.NegInf2))
                    {
                        value = double.NegativeInfinity;
                        return true;
                    }
                    break;
            }
            return false;
        }

        public override string ToString()
        {
            return StringEncoding.Utf8.GetString(AsSpan());
        }

        public bool SequenceEqual(Scalar other)
        {
            return AsSpan().SequenceEqual(other.AsSpan());
        }

        public bool SequenceEqual(ReadOnlySpan<byte> span)
        {
            return AsSpan().SequenceEqual(span);
        }

        // bool IsInfinity()
        // {
        //     if (Length == 4)
        //     {
        //         if (span.SequenceEqual(YamlCodes.Inf0) ||
        //             span.SequenceEqual(YamlCodes.Inf1) ||
        //             span.SequenceEqual(YamlCodes.Inf2))
        //         {
        //             value = double.PositiveInfinity;
        //             return true;
        //         }
        //     }
        //     return false;
        // }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Grow(int sizeHint)
        {
            if (sizeHint <= buffer.Length)
            {
                return;
            }
            var newCapacity = buffer.Length * GrowFactor / 100;
            while (newCapacity < sizeHint)
            {
                newCapacity = newCapacity * GrowFactor / 100;
            }
            SetCapacity(newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Grow()
        {
            var newCapacity = buffer.Length * GrowFactor / 100;
            if (newCapacity < buffer.Length + MinimumGrow)
            {
                newCapacity = buffer.Length + MinimumGrow;
            }
            SetCapacity(newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetCapacity(int newCapacity)
        {
            if (buffer.Length >= newCapacity) return;

            var newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);
            Array.Copy(buffer, 0, newBuffer, 0, Length);
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = newBuffer;
        }
    }
}
