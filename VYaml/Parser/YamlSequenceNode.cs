#nullable enable
using System.Collections;
using System.Collections.Generic;

namespace VYaml.Parser
{
    /// <summary>
    /// A YAML sequence node.
    /// </summary>
    public sealed class YamlSequenceNode : YamlNode, IList<YamlNode>
    {
        readonly List<YamlNode> items = new();

        public YamlSequenceNode()
        {
        }

        public YamlSequenceNode(IEnumerable<YamlNode> items)
        {
            this.items.AddRange(items);
        }

        public override YamlNodeType NodeType => YamlNodeType.Sequence;

        public int Count => items.Count;
        public bool IsReadOnly => false;

        public new YamlNode this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }

        public void Add(YamlNode item) => items.Add(item);
        public void Insert(int index, YamlNode item) => items.Insert(index, item);
        public bool Remove(YamlNode item) => items.Remove(item);
        public void RemoveAt(int index) => items.RemoveAt(index);
        public void Clear() => items.Clear();
        public bool Contains(YamlNode item) => items.Contains(item);
        public int IndexOf(YamlNode item) => items.IndexOf(item);
        public void CopyTo(YamlNode[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

        public List<YamlNode>.Enumerator GetEnumerator() => items.GetEnumerator();
        IEnumerator<YamlNode> IEnumerable<YamlNode>.GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }
}
