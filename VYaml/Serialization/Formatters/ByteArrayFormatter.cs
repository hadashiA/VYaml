using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    internal static class SpanExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64String(this ReadOnlySpan<byte> bytes,
            Base64FormattingOptions options = Base64FormattingOptions.None)
        {
#if NETSTANDARD2_0
            return Convert.ToBase64String(bytes.ToArray(), options);
#else
            return Convert.ToBase64String(bytes, options);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64String(this Span<byte> bytes,
            Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            ReadOnlySpan<byte> robytes = bytes;
            return robytes.ToBase64String(options);
        }
    }

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
                value.AsSpan().ToBase64String(),
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
                value.Span.ToBase64String(),
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
                value.Span.ToBase64String(),
                ScalarStyle.Plain
            );
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
                builder.Append(segment.Span.ToBase64String());
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
                Convert.ToBase64String(value.Array, value.Offset, value.Count, Base64FormattingOptions.None),
                ScalarStyle.Plain);
        }

        public ArraySegment<byte> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var str = parser.ReadScalarAsString();
            return new (Convert.FromBase64String(str!));
        }
    }
}

