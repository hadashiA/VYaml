using System.Collections.Concurrent;

namespace VYaml.Serialization
{
    public class BlockingCollectionFormatter<T> : CollectionFormatterBase<T, BlockingCollection<T>, BlockingCollection<T>>
    {
        protected override BlockingCollection<T> Create(YamlSerializerOptions options)
        {
            return new BlockingCollection<T>();
        }

        protected override void Add(BlockingCollection<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override BlockingCollection<T> Complete(BlockingCollection<T> intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}
