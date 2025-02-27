//#if NETSTANDARD2_0
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace System
{
    public static class NetStandardCompat
    {
        public static int GetBytes(this Encoding encoding, string chars, Span<byte> bytes)
        {
            return encoding.GetBytes(chars.AsSpan(), bytes);
        }

        public static int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            bytes = new byte[encoding.GetMaxByteCount(chars.Length)];
            unsafe
            {
                fixed (char* srcPtr = &chars[0])
                {
                    fixed (byte* targetPtr = &bytes[0])
                    {
                        var byteCount = encoding.GetBytes(srcPtr, chars.Length, targetPtr, bytes.Length);
                        bytes = bytes.Slice(0, byteCount);
                        return byteCount;
                    }
                }
            }
        }

        public static string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            unsafe
            {
                fixed(byte* srcPtr = &bytes[0])
                {
                    
                    return encoding.GetString(srcPtr, bytes.Length);
                }
            }
        }

        public static int GetByteCount(this Encoding encoding, ReadOnlySpan<char> chars)
        {
            unsafe
            {
                fixed (char* srcPtr = &chars[0])
                {

                    return encoding.GetByteCount(srcPtr, chars.Length);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>? =>
            span.Length != 0 && (span[^1]?.Equals(value) ?? (object?)value is null);

        public static void GetFirstSpan<T>(this ReadOnlySequence<T> sequence, 
            out ReadOnlySpan<T> first, out SequencePosition _nextPosition)
        {
            var firstMem = sequence.First;
            first = firstMem.Span;

            _nextPosition = sequence.GetPosition(first.Length);
        }

        public static void Append(this StringBuilder stringBuilder, ReadOnlySpan<char> chars)
        {
            foreach (var c in chars)
            {
                stringBuilder.Append(c);
            }
        }

        public static bool SequenceEqual(this ReadOnlySpan<char> span, string s)
        {
            return s.AsSpan().SequenceEqual(span);
        }

        public static Value? GetOrAdd<Key, Value, Param>(this ConcurrentDictionary<Key, Value> dict,
            Key key, Func<Key, Param, Value> fkt, Param param)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }
            value = fkt(key, param);

            while (true)
            {   
                if (dict.TryAdd(key, value))
                {
                    return value;
                }
                else
                {
                    if (dict.TryGetValue(key, out value))
                    {
                        return value;
                    }
                    //could be removed already...
                }
            }
        }

        public static System.Threading.Tasks.ValueTask<int> ReadAsync(this Stream stream, Memory<byte> chars,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static int CopyTo(this StringBuilder stream, int startIndex, ReadOnlySpan<char> chars, int cnt)
        {
            throw new NotImplementedException();
        }
    }
}
//#endif