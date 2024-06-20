using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class StackFormatter<T> : CollectionFormatterBase<T, List<T>, Stack<T>>
    {
        protected override List<T> Create(YamlSerializerOptions options)
        {
            return new List<T>();
        }

        protected override void Add(List<T> collection, T value, YamlSerializerOptions options)
        {
            collection.Add(value);
        }

        protected override Stack<T> Complete(List<T> intermediateCollection)
        {
            var stack = new Stack<T>();
            for (var i = intermediateCollection.Count - 1; i >= 0; i--)
            {
                stack.Push(intermediateCollection[i]);
            }
            return stack;
        }
    }
}