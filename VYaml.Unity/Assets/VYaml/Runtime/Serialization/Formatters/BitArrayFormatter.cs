using System;
using System.Buffers;
using System.Collections;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class BitArrayFormatter : IYamlFormatter<BitArray?>
    {
        public static readonly BitArrayFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, BitArray? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
                return;
            }

            var buffer = ArrayPool<byte>.Shared.Rent(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                var x = value.Get(i);
                buffer[i] = x ? (byte)'1' : (byte)'0';
            }
            emitter.WriteScalar(buffer.AsSpan(0, value.Length));
            ArrayPool<byte>.Shared.Return(buffer);
        }

        public BitArray? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var value = parser.GetScalarAsUtf8();
            var bitArray = new BitArray(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                bitArray.Set(i, value[i] == '1');
            }

            parser.Read();
            return bitArray;
        }
    }
}
