using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class QueueFormatter<T> : IYamlFormatter<Queue<T>?>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, Queue<T>? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
            }
            else
            {
                emitter.BeginSequence();
                if (value.Count > 0)
                {
                    var elementFormatter = context.Resolver.GetFormatterWithVerify<T>();
                    foreach (var x in value)
                    {
                        elementFormatter.Serialize(ref emitter, x, context);
                    }
                }
                emitter.EndSequence();
            }
        }

        public Queue<T>? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);

            var queue = new Queue<T>();
            var elementFormatter = context.Resolver.GetFormatterWithVerify<T>();
            while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
            {
                var value = context.DeserializeWithAlias(elementFormatter, ref parser);
                queue.Enqueue(value);
            }
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return queue;
        }
    }
}
