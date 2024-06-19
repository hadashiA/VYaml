#nullable enable
using System;
using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class DictionaryFormatter<TKey, TValue> : 
        IYamlFormatter<IDictionary<TKey, TValue>?>
        where TKey : notnull
    {
        public void Serialize(ref Utf8YamlEmitter emitter, IDictionary<TKey, TValue>? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
                return;
            }

            emitter.BeginMapping();
            if (value.Count > 0)
            {
                var keyFormatter = context.Resolver.GetFormatterWithVerify<TKey>();
                var valueFormatter = context.Resolver.GetFormatterWithVerify<TValue>();
                foreach (var x in value)
                {
                    keyFormatter.Serialize(ref emitter, x.Key, context);
                    valueFormatter.Serialize(ref emitter, x.Value, context);
                }
            }
            emitter.EndMapping();
        }

        public IDictionary<TKey, TValue>? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
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

            Console.WriteLine("empty " + parser.End + " " + parser.CurrentEventType);
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

