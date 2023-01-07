using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using VYaml.Emitter;
using VYaml.Internal;

namespace VYaml.Serialization
{
    public readonly struct SequenceStyleScope
    {
    }

    public readonly struct ScalarStyleScope
    {
    }

    public class YamlSerializationContext : IDisposable
    {
        public IYamlFormatterResolver Resolver { get; }
        public YamlEmitOptions EmitOptions { get; }

        readonly byte[] primitiveValueBuffer;

        public YamlSerializationContext()
        {
            primitiveValueBuffer = ArrayPool<byte>.Shared.Rent(64);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(ref Utf8YamlEmitter emitter, T value)
        {
            Resolver.GetFormatterWithVerify<T>().Serialize(ref emitter, value, this);
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(primitiveValueBuffer);
        }

        public Span<byte> GetBuffer(int length)
        {
            return primitiveValueBuffer.AsSpan(0, length);
        }

        // readonly Stack<SequenceStyle> sequenceStyleStack = new();
        // readonly Stack<ScalarStyle> sequenceStyleStack = new();

        public YamlSerializationContext(YamlSerializerOptions options)
        {
            Resolver = options.Resolver;
            EmitOptions = options.EmitOptions;
        }
    }
}