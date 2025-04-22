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
    internal static class NetStandardCompat
    {
        public static int GetBytes(this Encoding encoding, string chars, Span<byte> bytes)
        {
            return GetBytes(encoding, chars.AsSpan(), bytes);
        }

        public static int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
        {
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

        public static async System.Threading.Tasks.ValueTask<int> ReadAsync(this Stream stream, Memory<byte> chars,
            CancellationToken cancellationToken)
        {
            var read = 0;
            var array = ArrayPool<byte>.Shared.Rent(512);
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var remainingSpaceInTarget = chars.Length - read;
                    var maxPossible = Math.Min(remainingSpaceInTarget, array.Length);

                    var curRead = await stream.ReadAsync(array, 0, maxPossible);
                    if (curRead == 0)
                    {
                        return read;
                    }
                    array.AsSpan()[0..curRead].CopyTo(chars.Span.Slice(read));
                    read += curRead;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        public static int CopyTo(this StringBuilder stringBuilder, int sourceIndex, Span<char> destination, int count)
        {
            var arrSize = Math.Min(destination.Length, count);
            var arr = new char[arrSize];

            stringBuilder.CopyTo(sourceIndex, arr, 0, arrSize);

            //todo: avoid copy...
            arr.AsSpan().CopyTo(destination);
            return arrSize;
        }
    }
}