using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class GenericCollectionFormatterBase<TElement, TCollection>
        : CollectionFormatterBase<TElement, TCollection, TCollection>
        where TCollection : ICollection<TElement>, new()
    {

        protected override void Add(TCollection collection, TElement value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override TCollection Complete(TCollection intermediateCollection)
        {
            return intermediateCollection;
        }
    }
}