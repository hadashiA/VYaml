using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class LinkedListFormatter<T> : CollectionFormatterBase<T, LinkedList<T>, LinkedList<T>>
    {
        protected override LinkedList<T> Create(YamlSerializerOptions options)
        {
            return new LinkedList<T>();
        }

        protected override void Add(LinkedList<T> collection, T value, YamlSerializerOptions options)
        {
            collection.AddLast(value);
        }

        protected override LinkedList<T> Complete(LinkedList<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}