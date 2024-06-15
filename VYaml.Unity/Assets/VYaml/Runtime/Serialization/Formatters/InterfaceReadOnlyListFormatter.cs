using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class InterfaceReadOnlyListFormatter<T> : CollectionFormatterBase<T, List<T>, IReadOnlyList<T>>
    {
        protected override List<T> Create(YamlSerializerOptions options)
        {
            return new List<T>();
        }

        protected override void Add(List<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override IReadOnlyList<T> Complete(List<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}
