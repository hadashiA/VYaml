#nullable enable
using System;
using VYaml.Internal;
using VYaml.Serialization;

namespace VYaml.Parser
{
    public enum YamlNodeType
    {
        Scalar,
        Mapping,
        Sequence,
    }

    /// <summary>
    /// A mutable, in-memory tree representation of a single YAML node.
    /// </summary>
    /// <remarks>
    /// This is the DOM-like counterpart to the event-driven <see cref="YamlParser"/>,
    /// comparable to <c>System.Text.Json</c>'s <c>JsonNode</c>.
    /// Anchors and aliases are represented as shared references: an alias (<c>*name</c>)
    /// resolves to the very same <see cref="YamlNode"/> instance that the anchor (<c>&amp;name</c>) was declared on.
    /// </remarks>
    public abstract class YamlNode
    {
        /// <summary>
        /// The explicit tag (<c>!!str</c>, <c>!Foo</c>, ...) attached to this node, if any.
        /// </summary>
        public Tag? Tag { get; set; }

        /// <summary>
        /// The anchor (<c>&amp;name</c>) declared on this node, if any.
        /// </summary>
        public Anchor? Anchor { get; set; }

        public abstract YamlNodeType NodeType { get; }

        public bool IsScalar => NodeType == YamlNodeType.Scalar;
        public bool IsMapping => NodeType == YamlNodeType.Mapping;
        public bool IsSequence => NodeType == YamlNodeType.Sequence;

        public YamlScalarNode AsScalar() => this as YamlScalarNode ??
            throw new InvalidOperationException($"The node is not a scalar. ({NodeType})");

        public YamlMappingNode AsMapping() => this as YamlMappingNode ??
            throw new InvalidOperationException($"The node is not a mapping. ({NodeType})");

        public YamlSequenceNode AsSequence() => this as YamlSequenceNode ??
            throw new InvalidOperationException($"The node is not a sequence. ({NodeType})");

        /// <summary>
        /// Convenience accessor for a child of a mapping node by string key.
        /// </summary>
        public YamlNode this[string key]
        {
            get => AsMapping()[key];
            set => AsMapping()[key] = value;
        }

        /// <summary>
        /// Convenience accessor for a child of a mapping node by node key.
        /// </summary>
        public YamlNode this[YamlNode key]
        {
            get => AsMapping()[key];
            set => AsMapping()[key] = value;
        }

        /// <summary>
        /// Convenience accessor for an element of a sequence node by index.
        /// </summary>
        public YamlNode this[int index]
        {
            get => AsSequence()[index];
            set => AsSequence()[index] = value;
        }

        public static implicit operator YamlNode(string value) => new YamlScalarNode(value);
        public static implicit operator YamlNode(bool value) => new YamlScalarNode(value);
        public static implicit operator YamlNode(int value) => new YamlScalarNode(value);
        public static implicit operator YamlNode(long value) => new YamlScalarNode(value);
        public static implicit operator YamlNode(uint value) => new YamlScalarNode(value);
        public static implicit operator YamlNode(ulong value) => new YamlScalarNode(value);
        public static implicit operator YamlNode(float value) => new YamlScalarNode(value);
        public static implicit operator YamlNode(double value) => new YamlScalarNode(value);

        /// <summary>
        /// Parses the first YAML document of <paramref name="yaml"/> into a node tree.
        /// </summary>
        public static YamlNode Parse(string yaml, YamlSerializerOptions? options = null)
        {
            return Parse(StringEncoding.Utf8.GetBytes(yaml), options);
        }

        /// <summary>
        /// Parses the first YAML document of <paramref name="utf8"/> into a node tree.
        /// </summary>
        public static YamlNode Parse(ReadOnlyMemory<byte> utf8, YamlSerializerOptions? options = null)
        {
            return YamlSerializer.Deserialize<YamlNode>(utf8, options);
        }

        /// <summary>
        /// Parses the current node out of an already-positioned <see cref="YamlParser"/>.
        /// </summary>
        public static YamlNode Parse(ref YamlParser parser, YamlSerializerOptions? options = null)
        {
            return YamlSerializer.Deserialize<YamlNode>(ref parser, options);
        }
    }
}
