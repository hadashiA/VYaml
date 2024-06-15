using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class SortedSetFormatter<T> : CollectionFormatterBase<T, SortedSet<T>, SortedSet<T>>
    {
        protected override SortedSet<T> Create(YamlSerializerOptions options)
        {
            return new SortedSet<T>();
        }

        protected override void Add(SortedSet<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override SortedSet<T> Complete(SortedSet<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}
