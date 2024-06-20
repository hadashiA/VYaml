using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class QueueFormatter<T> : CollectionFormatterBase<T, Queue<T>, Queue<T>>
    {
        protected override Queue<T> Create(YamlSerializerOptions options)
        {
            return new Queue<T>();
        }

        protected override void Add(Queue<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Enqueue(value);
        }

        protected override Queue<T> Complete(Queue<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}
