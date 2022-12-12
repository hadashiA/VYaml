using System;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class UriFormatter : IYamlFormatter<Uri>
    {
        public static readonly UriFormatter Instance = new();

        public Uri Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.TryGetScalarAsSpan(out var span))
            {
                var uri = new Uri(span.ToString(), UriKind.RelativeOrAbsolute);
                parser.Read();
                return uri;
            }
            throw new YamlSerializerException($"Cannot detect a scalar value of Uri : {parser.CurrentEventType} {parser.GetScalarAsString()}");
        }
    }
}
