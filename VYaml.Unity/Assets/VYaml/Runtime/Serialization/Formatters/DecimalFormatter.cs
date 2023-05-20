#nullable enable
using System.Buffers.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class DecimalFormatter : IYamlFormatter<decimal>
    {
        public static readonly DecimalFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, decimal value, YamlSerializationContext context)
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
