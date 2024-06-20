using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class ConcurrentStackFormatter<T> : CollectionFormatterBase<T, List<T>, ConcurrentStack<T>>
    {
        protected override List<T> Create(YamlSerializerOptions options)
        {
            return new List<T>();
        }

        protected override void Add(List<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override ConcurrentStack<T> Complete(List<T> intermediateCollection)
        {
            var stack = new ConcurrentStack<T>();
            for (var i = intermediateCollection.Count - 1; i >= 0; i--)
            {
                stack.Push(intermediateCollection[i]);
            }
            return stack;
        }
    }
}