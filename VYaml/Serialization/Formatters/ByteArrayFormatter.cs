using System;
using System.Buffers;
using System.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public sealed class ByteArrayFormatter : IYamlFormatter<byte[]?>
    {
        public static readonly IYamlFormatter<byte[]?> Instance = new ByteArrayFormatter();

        public void Serialize(ref Utf8YamlEmitter emitter, byte[]? value, YamlSerializationContext context)
        {
            if (value == null)
            {
                emitter.WriteNull();
                return;
            }

            emitter.WriteString(
                Convert.ToBase64String(value, Base64FormattingOptions.None),
                ScalarStyle.Plain);
        }

        public byte[]? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return null;
            }

            var str = parser.ReadScalarAsString();
            return Convert.FromBase64String(str!);
        }
    }

    public class ByteMemoryFormatter : IYamlFormatter<Memory<byte>>
    {
        public static readonly ByteMemoryFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Memory<byte> value, YamlSerializationContext context)
        {
            emitter.WriteString(
                Convert.ToBase64String(value.Span, Base64FormattingOptions.None),
                ScalarStyle.Plain);
        }

        public Memory<byte> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var str = parser.ReadScalarAsString();
            return Convert.FromBase64String(str!);
        }
    }

    public class ByteReadOnlyMemoryFormatter : IYamlFormatter<ReadOnlyMemory<byte>>
    {
        public static readonly ByteReadOnlyMemoryFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, ReadOnlyMemory<byte> value, YamlSerializationContext context)
        {
            emitter.WriteString(
                Convert.ToBase64String(value.Span, Base64FormattingOptions.None),
                ScalarStyle.Plain);
        }

        public ReadOnlyMemory<byte> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var str = parser.ReadScalarAsString();
            return Convert.FromBase64String(str!);
        }
    }

    public class ByteReadOnlySequenceFormatter : IYamlFormatter<ReadOnlySequence<byte>>
    {
        public static readonly ByteReadOnlySequenceFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, ReadOnlySequence<byte> value, YamlSerializationContext context)
        {
            var builder = new StringBuilder((int)value.Length);
            foreach (var segment in value)
            {
                builder.Append(Convert.ToBase64String(segment.Span, Base64FormattingOptions.None));
            }
            emitter.WriteString(builder.ToString(), ScalarStyle.Plain);
        }

        public ReadOnlySequence<byte> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var str = parser.ReadScalarAsString();
            var bytes = Convert.FromBase64String(str!);
            return new ReadOnlySequence<byte>(bytes);
        }
    }

    public class ByteArraySegmentFormatter : IYamlFormatter<ArraySegment<byte>>
    {
        public static readonly ByteArraySegmentFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, ArraySegment<byte> value, YamlSerializationContext context)
        {
            emitter.WriteString(
                Convert.ToBase64String(value, Base64FormattingOptions.None),
                ScalarStyle.Plain);
        }

        public ArraySegment<byte> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var str = parser.ReadScalarAsString();
            return Convert.FromBase64String(str!);
        }
    }
}

