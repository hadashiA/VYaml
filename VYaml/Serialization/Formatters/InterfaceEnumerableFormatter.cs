using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class InterfaceEnumerableFormatter<T> : CollectionFormatterBase<T, List<T>, IEnumerable<T>>
    {
        protected override List<T> Create(YamlSerializerOptions options)
        {
            return new List<T>();
        }

        protected override void Add(List<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override IEnumerable<T> Complete(List<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}
