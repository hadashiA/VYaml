using System.Collections.Generic;

namespace VYaml.Formatters
{
    public class ListFormatter<T> : IYamlFormatter<List<T>?>
    {
        public List<T>? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            if (parser.CurrentEventType != ParseEventType.SequenceStart)
            {
                throw new YamlSerializerException($"Invalid sequence : {parser.CurrentEventType}");
            }

            var list = new List<T>();
            var elementFormatter = context.Resolver.GetFormatterWithVerify<T>();
            while (parser.Read() && parser.CurrentEventType != ParseEventType.SequenceEnd)
            {
                var value = context.DeserializeWithAlias(elementFormatter, ref parser);
                list.Add(value);
            }

            parser.Read();
            return list;
        }
    }
}