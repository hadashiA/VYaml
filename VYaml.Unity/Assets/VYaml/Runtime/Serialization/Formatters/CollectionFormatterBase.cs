using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public abstract class CollectionFormatterBase<TElement, TIntermediate, TCollection>
        : IYamlFormatter<TCollection?>
        where TCollection : IEnumerable<TElement>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, TCollection? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
            }
            else
            {
                emitter.BeginSequence();
                if (GetCount(value) > 0)
                {
                    var elementFormatter = context.Resolver.GetFormatterWithVerify<TElement>();
                    foreach (var x in value)
                    {
                        elementFormatter.Serialize(ref emitter, x, context);
                    }
                }
                emitter.EndSequence();
            }

        }

        public TCollection? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);

            var list = Create(context.Options);
            var elementFormatter = context.Resolver.GetFormatterWithVerify<TElement>();
            while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
            {
                var value = context.DeserializeWithAlias(elementFormatter, ref parser);
                Add(list, value, context.Options);
            }
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return Complete(list);
        }

        // abstraction for serialize
        protected virtual int? GetCount(TCollection sequence)
        {
            if (sequence is ICollection<TElement> collection)
            {
                return collection.Count;
            }

            if (sequence is IReadOnlyCollection<TElement> readonlyCollection)
            {
                return readonlyCollection.Count;
            }

            return null;
        }


        // abstraction for deserialize
        protected abstract TIntermediate Create(YamlSerializerOptions options);
        protected abstract void Add(TIntermediate collection, TElement value, YamlSerializerOptions options);
        protected abstract TCollection Complete(TIntermediate intermediateCollection);
    }
}