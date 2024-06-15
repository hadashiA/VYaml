using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class InterfaceListFormatter<T> : CollectionFormatterBase<T, List<T>, IList<T>>
    {
        protected override List<T> Create(YamlSerializerOptions options)
        {
            return new List<T>();
        }

        protected override void Add(List<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override IList<T> Complete(List<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}
