using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class InterfaceSetFormatter<T> : CollectionFormatterBase<T, HashSet<T>, ISet<T>>
    {
        protected override HashSet<T> Create(YamlSerializerOptions options)
        {
            return new HashSet<T>();
        }

        protected override void Add(HashSet<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override ISet<T> Complete(HashSet<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}
