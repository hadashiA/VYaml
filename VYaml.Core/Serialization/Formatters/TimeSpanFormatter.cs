using System;
using System.Buffers.Text;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class TimeSpanFormatter : IYamlFormatter<TimeSpan>
    {
        public static readonly TimeSpanFormatter Instance = new();

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