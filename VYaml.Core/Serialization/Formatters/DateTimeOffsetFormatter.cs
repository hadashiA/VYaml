#nullable enable
using System;
using System.Buffers;
using System.Buffers.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class DateTimeOffsetFormatter : IYamlFormatter<DateTimeOffset>
    {
        public static readonly DateTimeOffsetFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, DateTimeOffset value, YamlSerializationContext context)
        {
            var buf = context.GetBuffer64();
            if (Utf8Formatter.TryFormat(value, buf, out var bytesWritten, new StandardFormat('O')))
            {
                emitter.WriteScalar(buf[..bytesWritten]);
            }
            else
            {
                throw new YamlSerializerException($"Cannot format {value}");
            }
        }

        public DateTimeOffset Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.TryGetScalarAsSpan(out var span) &&
                Utf8Parser.TryParse(span, out DateTimeOffset value, out var bytesConsumed) &&
                bytesConsumed == span.Length)
            {
                parser.Read();
                return value;
            }

            throw new YamlSerializerException($"Cannot detect a scalar value of DateTimeOffset : {parser.CurrentEventType} {parser.GetScalarAsString()}");
        }
    }
}
