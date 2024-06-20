using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class ConcurrentQueueFormatter<T> : CollectionFormatterBase<T, Queue<T>, ConcurrentQueue<T>>
    {
        protected override Queue<T> Create(YamlSerializerOptions options)
        {
            return new Queue<T>();
        }

        protected override void Add(Queue<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Enqueue(value);
        }

        protected override ConcurrentQueue<T> Complete(Queue<T> intermediateCollection)
        {
            return new ConcurrentQueue<T>(intermediateCollection);
        }
    }
}
