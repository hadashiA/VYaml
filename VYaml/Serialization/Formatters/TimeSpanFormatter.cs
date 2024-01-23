#nullable enable
using System;
using System.Buffers.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class TimeSpanFormatter : IYamlFormatter<TimeSpan>
    {
        public static readonly TimeSpanFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, TimeSpan value, YamlSerializationContext context)
        {
            var buf = context.GetBuffer64();
            if (Utf8Formatter.TryFormat(value, buf, out var bytesWritten))
            {
                emitter.WriteScalar(buf[..bytesWritten]);
            }
            else
            {
                throw new YamlSerializerException($"Cannot serialize a value: {value}");
            }
        }

        public TimeSpan Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.TryGetScalarAsSpan(out var span) &&
                Utf8Parser.TryParse(span, out TimeSpan timeSpan, out var bytesConsumed) &&
                bytesConsumed == span.Length)
            {
                parser.Read();
                return timeSpan;
            }
            throw new YamlSerializerException($"Cannot detect a scalar value of TimeSpan : {parser.CurrentEventType} {parser.GetScalarAsString()}");
        }
    }
}
