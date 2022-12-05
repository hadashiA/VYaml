using System;
using System.Buffers.Text;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class DecimalFormatter : IYamlFormatter<decimal>
    {
        public static readonly DecimalFormatter Instance = new();

        public decimal Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.TryGetScalarAsSpan(out var span) &&
                Utf8Parser.TryParse(span, out decimal value, out var bytesConsumed) &&
                bytesConsumed == span.Length)
            {
                parser.Read();
                return value;
            }
            throw new YamlSerializerException($"Cannot detect a scalar value of decimal : {parser.CurrentEventType} {parser.GetScalarAsString()}");
        }
    }
}