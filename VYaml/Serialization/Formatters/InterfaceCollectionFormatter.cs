using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class InterfaceCollectionFormatter<T> : CollectionFormatterBase<T, List<T>, ICollection<T>>
    {
        protected override List<T> Create(YamlSerializerOptions options)
        {
            return new List<T>();
        }

        protected override void Add(List<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override ICollection<T> Complete(List<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}
