#nullable enable
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VYaml.Internal
{
    static class StreamHelper
    {
        public static async ValueTask<ReusableByteSequenceBuilder> ReadAsSequenceAsync(Stream stream, CancellationToken cancellation = default)
        {
            var builder = ReusableByteSequenceBuilderPool.Rent();
            try
            {
                if (stream is MemoryStream ms && ms.TryGetBuffer(out var arraySegment))
                {
                    cancellation.ThrowIfCancellationRequested();

                    // Emulate that we had actually "read" from the stream.
                    ms.Seek(arraySegment.Count, SeekOrigin.Current);

                    builder.Add(arraySegment.AsMemory(), false);
                    return builder;
                }

                var buffer = ArrayPool<byte>.Shared.Rent(65536); // initial 64K
                var offset = 0;
                do
                {
                    if (offset == buffer.Length)
                    {
                        builder.Add(buffer, returnToPool: true);
                        buffer = ArrayPool<byte>.Shared.Rent(NewArrayCapacity(buffer.Length));
                        offset = 0;
                    }

                    int bytesRead;
                    try
                    {
                        bytesRead = await stream
                            .ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellation)
                            .ConfigureAwait(false);
                    }
                    catch
                    {
                        // buffer is not added in builder, so return here.
                        ArrayPool<byte>.Shared.Return(buffer);
                        throw;
                    }

                    offset += bytesRead;

                    if (bytesRead == 0)
                    {
                        builder.Add(buffer.AsMemory(0, offset), returnToPool: true);
                        break;
                    }
                } while (true);
            }
            catch (Exception)
            {
                ReusableByteSequenceBuilderPool.Return(builder);
                throw;
            }

            return builder;
        }

        const int ArrayMexLength = 0x7FFFFFC7;

        static int NewArrayCapacity(int size)
        {
            var newSize = unchecked(size * 2);
            if ((uint)newSize > ArrayMexLength)
            {
                newSize = ArrayMexLength;
            }
            return newSize;
        }
    }
}

