#nullable enable
using System;
using System.Buffers.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class GuidFormatter : IYamlFormatter<Guid>
    {
        public static readonly GuidFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Guid value, YamlSerializationContext context)
        {
            // nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn
            var buf = context.GetBuffer64();
            if (Utf8Formatter.TryFormat(value, buf, out var bytesWritten))
            {
                emitter.WriteScalar(buf[..bytesWritten]);
            }
            else
            {
                throw new YamlSerializerException($"Cannot serialize {value}");
            }
        }

        public Guid Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.TryGetScalarAsSpan(out var span) &&
                Utf8Parser.TryParse(span, out Guid guid, out var bytesConsumed) &&
                bytesConsumed == span.Length)
            {
                parser.Read();
                return guid;
            }
            throw new YamlSerializerException($"Cannot detect a scalar value of Guid : {parser.CurrentEventType} {parser.GetScalarAsString()}");
        }
    }
}
