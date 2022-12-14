using System.Collections.Generic;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class KeyValuePairFormatter<TKey, TValue> : IYamlFormatter<KeyValuePair<TKey, TValue>>
    {
        public KeyValuePair<TKey, TValue> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var key = context.DeserializeWithAlias<TKey>(ref parser);
            var value = context.DeserializeWithAlias<TValue>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}