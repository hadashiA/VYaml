#nullable enable
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
        public YamlSerializerOptions Options { get; set; }
        public IYamlFormatterResolver Resolver { get; set; }
        public YamlEmitOptions EmitOptions { get; set; }

        readonly byte[] primitiveValueBuffer;
        ArrayBufferWriter<byte>? arrayBufferWriter;

        public YamlSerializationContext(YamlSerializerOptions options)
        {
            primitiveValueBuffer = ArrayPool<byte>.Shared.Rent(64);
            Options = options;
            Resolver = options.Resolver;
            EmitOptions = options.EmitOptions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(ref Utf8YamlEmitter emitter, T value)
        {
            Resolver.GetFormatterWithVerify<T>().Serialize(ref emitter, value, this);
        }

        public ArrayBufferWriter<byte> GetArrayBufferWriter()
        {
            return arrayBufferWriter ??= new ArrayBufferWriter<byte>(65536);
        }

        public void Reset()
        {
            arrayBufferWriter?.Clear();
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(primitiveValueBuffer);
        }

        public byte[] GetBuffer64() => primitiveValueBuffer;

        // readonly Stack<SequenceStyle> sequenceStyleStack = new();
        // readonly Stack<ScalarStyle> sequenceStyleStack = new();
   }
}
