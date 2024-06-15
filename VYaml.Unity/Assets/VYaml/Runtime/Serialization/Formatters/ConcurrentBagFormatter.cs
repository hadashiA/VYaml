using System.Collections.Concurrent;

namespace VYaml.Serialization
{
    public class ConcurrentBagFormatter<T> : CollectionFormatterBase<T, ConcurrentBag<T>, ConcurrentBag<T>>
    {
        protected override ConcurrentBag<T> Create(YamlSerializerOptions options)
        {
            return new ConcurrentBag<T>();
        }

        protected override void Add(ConcurrentBag<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override ConcurrentBag<T> Complete(ConcurrentBag<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}
