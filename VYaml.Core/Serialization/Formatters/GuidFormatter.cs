using System;
using System.Buffers.Text;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class GuidFormatter : IYamlFormatter<Guid>
    {
        public static readonly GuidFormatter Instance = new();

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