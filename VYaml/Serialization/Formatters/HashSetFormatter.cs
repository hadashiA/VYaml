using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class HashSetFormatter<T> : CollectionFormatterBase<T, HashSet<T>, HashSet<T>>
    {
        protected override HashSet<T> Create(YamlSerializerOptions options)
        {
            return new HashSet<T>();
        }

        protected override void Add(HashSet<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override HashSet<T> Complete(HashSet<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}
