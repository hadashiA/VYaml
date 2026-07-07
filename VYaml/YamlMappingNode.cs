#nullable enable
using System.Collections;
using System.Collections.Generic;

namespace VYaml
{
    /// <summary>
    /// A YAML mapping node. Insertion order is preserved.
    /// </summary>
    public sealed class YamlMappingNode : YamlNode, IDictionary<YamlNode, YamlNode>
    {
        readonly List<YamlNode> keyOrder = new();
        readonly Dictionary<YamlNode, YamlNode> entries = new();

        public override YamlNodeType NodeType => YamlNodeType.Mapping;

        public int Count => entries.Count;
        public bool IsReadOnly => false;

        public ICollection<YamlNode> Keys => keyOrder;
        public ICollection<YamlNode> Values
        {
            get
            {
                var values = new List<YamlNode>(keyOrder.Count);
                foreach (var key in keyOrder)
                {
                    values.Add(entries[key]);
                }
                return values;
            }
        }

        public new YamlNode this[YamlNode key]
        {
            get => entries[key];
            set
            {
                if (!entries.ContainsKey(key))
                {
                    keyOrder.Add(key);
                }
                entries[key] = value;
            }
        }

        public new YamlNode this[string key]
        {
            get => entries[new YamlScalarNode(key)];
            set => this[(YamlNode)new YamlScalarNode(key)] = value;
        }

        public bool ContainsKey(YamlNode key) => entries.ContainsKey(key);
        public bool ContainsKey(string key) => entries.ContainsKey(new YamlScalarNode(key));

        public bool TryGetValue(YamlNode key, out YamlNode value) => entries.TryGetValue(key, out value!);
        public bool TryGetValue(string key, out YamlNode value) => entries.TryGetValue(new YamlScalarNode(key), out value!);

        public void Add(YamlNode key, YamlNode value)
        {
            entries.Add(key, value);
            keyOrder.Add(key);
        }

        public void Add(KeyValuePair<YamlNode, YamlNode> item) => Add(item.Key, item.Value);

        public bool Remove(YamlNode key)
        {
            if (entries.Remove(key))
            {
                keyOrder.Remove(key);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<YamlNode, YamlNode> item) => Remove(item.Key);

        public void Clear()
        {
            entries.Clear();
            keyOrder.Clear();
        }

        public bool Contains(KeyValuePair<YamlNode, YamlNode> item) =>
            entries.TryGetValue(item.Key, out var value) && EqualityComparer<YamlNode>.Default.Equals(value, item.Value);

        public void CopyTo(KeyValuePair<YamlNode, YamlNode>[] array, int arrayIndex)
        {
            foreach (var pair in this)
            {
                array[arrayIndex++] = pair;
            }
        }

        public IEnumerator<KeyValuePair<YamlNode, YamlNode>> GetEnumerator()
        {
            foreach (var key in keyOrder)
            {
                yield return new KeyValuePair<YamlNode, YamlNode>(key, entries[key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
