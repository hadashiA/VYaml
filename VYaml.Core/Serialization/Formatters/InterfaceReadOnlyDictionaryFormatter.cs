using System.Collections.Generic;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class InterfaceReadOnlyDictionaryFormatter<TKey, TValue> : IYamlFormatter<IReadOnlyDictionary<TKey, TValue>?>
    {
        public IReadOnlyDictionary<TKey, TValue>? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            parser.ReadWithVerify(ParseEventType.MappingStart);

            var map = new Dictionary<TKey, TValue>();
            var keyFormatter = context.Resolver.GetFormatterWithVerify<TKey>();
            var valueFormatter = context.Resolver.GetFormatterWithVerify<TValue>();

            while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
            {
                var key = context.DeserializeWithAlias(keyFormatter, ref parser);
                var value = context.DeserializeWithAlias(valueFormatter, ref parser);
                map.Add(key, value);
            }

            parser.ReadWithVerify(ParseEventType.MappingEnd);
            return map;
        }
    }
}
