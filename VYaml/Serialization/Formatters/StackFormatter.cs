using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class StackFormatter<T> : IYamlFormatter<Stack<T>?>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, Stack<T>? value, YamlSerializationContext context)
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

        public Stack<T>? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);

            var list = new List<T>();
            var elementFormatter = context.Resolver.GetFormatterWithVerify<T>();
            while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
            {
                var value = context.DeserializeWithAlias(elementFormatter, ref parser);
                list.Add(value);
            }

            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            var stack = new Stack<T>(list.Count);
            for (var i = list.Count - 1; i >= 0; i--)
            {
                stack.Push(list[i]);
            }
            return stack;
        }
    }
}