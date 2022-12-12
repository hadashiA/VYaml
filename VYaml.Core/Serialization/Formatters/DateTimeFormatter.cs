using System;
using System.Buffers.Text;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class DateTimeFormatter : IYamlFormatter<DateTime>
    {
        public static readonly DateTimeFormatter Instance = new();

        public DateTime Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.TryGetScalarAsSpan(out var span) &&
                Utf8Parser.TryParse(span, out DateTime dateTime, out var bytesConsumed) &&
                bytesConsumed == span.Length)
            {
                parser.Read();
                return dateTime;
            }
            throw new YamlSerializerException($"Cannot detect a scalar value of DateTime : {parser.CurrentEventType} {parser.GetScalarAsString()}");
        }
    }

    public class NullableDateTimeFormatter : IYamlFormatter<DateTime?>
    {
        public static readonly NullableDateTimeFormatter Instance = new();

        public DateTime? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            if (parser.TryGetScalarAsSpan(out var span) &&
                Utf8Parser.TryParse(span, out DateTime dateTime, out var bytesConsumed) &&
                bytesConsumed == span.Length)
            {
                parser.Read();
                return dateTime;
            }
            throw new YamlSerializerException($"Cannot detect a scalar value of DateTime : {parser.CurrentEventType} {parser.GetScalarAsString()}");
        }
    }
}