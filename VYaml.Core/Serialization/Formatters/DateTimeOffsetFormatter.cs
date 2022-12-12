using System;
using System.Buffers.Text;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class DateTimeOffsetFormatter : IYamlFormatter<DateTimeOffset>
    {
        public static readonly DateTimeOffsetFormatter Instance = new();

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