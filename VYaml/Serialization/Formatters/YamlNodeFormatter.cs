#nullable enable
using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    /// <summary>
    /// Materializes the YAML event stream into a <see cref="YamlNode"/> tree and back.
    /// </summary>
    /// <remarks>
    /// Anchors / aliases become shared node references: this leans on
    /// <see cref="YamlDeserializationContext.DeserializeWithAlias{T}(IYamlFormatter{T}, ref YamlParser)"/>,
    /// which registers each anchored node and resolves aliases to the same instance.
    /// </remarks>
    public sealed class YamlNodeFormatter : IYamlFormatter<YamlNode>
    {
        public static readonly YamlNodeFormatter Instance = new();

        public YamlNode Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            switch (parser.CurrentEventType)
            {
                case ParseEventType.Scalar:
                {
                    parser.TryGetCurrentTag(out var tag);
                    parser.TryGetCurrentAnchor(out var anchor);

                    var scalar = parser.GetCurrentScalar();
                    var node = scalar is null
                        ? new YamlScalarNode((string?)null, ScalarStyle.Plain)
                        : new YamlScalarNode(scalar.AsUtf8(), scalar.Type);
                    node.Tag = tag;
                    node.Anchor = anchor;
                    parser.Read();
                    return node;
                }
                case ParseEventType.MappingStart:
                {
                    parser.TryGetCurrentTag(out var tag);
                    var hasAnchor = parser.TryGetCurrentAnchor(out var anchor);

                    var node = new YamlMappingNode { Tag = tag, Anchor = hasAnchor ? anchor : null };
                    // Register before reading children so self-referential aliases resolve to this instance.
                    if (hasAnchor)
                    {
                        context.RegisterAnchor(anchor, node);
                    }
                    parser.Read();
                    while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
                    {
                        var key = context.DeserializeWithAlias(this, ref parser);
                        var value = context.DeserializeWithAlias(this, ref parser);
                        node[key] = value;
                    }
                    parser.ReadWithVerify(ParseEventType.MappingEnd);
                    return node;
                }
                case ParseEventType.SequenceStart:
                {
                    parser.TryGetCurrentTag(out var tag);
                    var hasAnchor = parser.TryGetCurrentAnchor(out var anchor);

                    var node = new YamlSequenceNode { Tag = tag, Anchor = hasAnchor ? anchor : null };
                    // Register before reading children so self-referential aliases resolve to this instance.
                    if (hasAnchor)
                    {
                        context.RegisterAnchor(anchor, node);
                    }
                    parser.Read();
                    while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
                    {
                        node.Add(context.DeserializeWithAlias(this, ref parser));
                    }
                    parser.ReadWithVerify(ParseEventType.SequenceEnd);
                    return node;
                }
                default:
                    throw new YamlSerializerException($"Cannot build a YamlNode from event: {parser.CurrentEventType}");
            }
        }

        public void Serialize(ref Utf8YamlEmitter emitter, YamlNode value, YamlSerializationContext context)
        {
            // Nodes shared by more than one reference (or carrying an explicit anchor) are emitted
            // once with an anchor (&name) and as aliases (*name) on every subsequent encounter.
            var refCounts = new Dictionary<YamlNode, int>(ReferenceComparer.Instance);
            CountReferences(value, refCounts);

            var state = new EmitState(refCounts);
            Write(ref emitter, value, state);
        }

        static void CountReferences(YamlNode node, Dictionary<YamlNode, int> refCounts)
        {
            if (refCounts.TryGetValue(node, out var count))
            {
                refCounts[node] = count + 1;
                return; // Already visited; don't recurse again (also breaks cycles).
            }
            refCounts[node] = 1;

            switch (node)
            {
                case YamlMappingNode mapping:
                    foreach (var pair in mapping)
                    {
                        CountReferences(pair.Key, refCounts);
                        CountReferences(pair.Value, refCounts);
                    }
                    break;
                case YamlSequenceNode sequence:
                    foreach (var item in sequence)
                    {
                        CountReferences(item, refCounts);
                    }
                    break;
            }
        }

        static void Write(ref Utf8YamlEmitter emitter, YamlNode node, EmitState state)
        {
            if (state.NeedsAnchor(node))
            {
                if (state.TryGetEmittedName(node, out var aliasName))
                {
                    emitter.WriteAlias(aliasName);
                    return;
                }
                emitter.Anchor(state.RegisterAnchor(node));
            }

            if (node.Tag is { } tag)
            {
                emitter.Tag(tag.ToString());
            }

            switch (node)
            {
                case YamlScalarNode scalar:
                    if (scalar.IsNull)
                    {
                        emitter.WriteNull();
                    }
                    else
                    {
                        emitter.WriteString(scalar.Value, scalar.Style);
                    }
                    break;

                case YamlMappingNode mapping:
                    emitter.BeginMapping();
                    foreach (var pair in mapping)
                    {
                        Write(ref emitter, pair.Key, state);
                        Write(ref emitter, pair.Value, state);
                    }
                    emitter.EndMapping();
                    break;

                case YamlSequenceNode sequence:
                    emitter.BeginSequence();
                    foreach (var item in sequence)
                    {
                        Write(ref emitter, item, state);
                    }
                    emitter.EndSequence();
                    break;

                default:
                    throw new YamlSerializerException($"Unknown YamlNode type: {node.NodeType}");
            }
        }

        sealed class EmitState
        {
            readonly Dictionary<YamlNode, int> refCounts;
            readonly Dictionary<YamlNode, string> emittedNames = new(ReferenceComparer.Instance);
            readonly HashSet<string> usedNames = new();
            int generated;

            public EmitState(Dictionary<YamlNode, int> refCounts)
            {
                this.refCounts = refCounts;
            }

            public bool NeedsAnchor(YamlNode node) =>
                node.Anchor != null || (refCounts.TryGetValue(node, out var count) && count >= 2);

            public bool TryGetEmittedName(YamlNode node, out string name) =>
                emittedNames.TryGetValue(node, out name!);

            public string RegisterAnchor(YamlNode node)
            {
                var name = node.Anchor?.Name;
                if (name is null || usedNames.Contains(name))
                {
                    do
                    {
                        name = $"id{++generated:D3}";
                    } while (usedNames.Contains(name));
                }
                usedNames.Add(name);
                emittedNames[node] = name;
                return name;
            }
        }

        /// <summary>
        /// Allows <c>Deserialize&lt;YamlMappingNode&gt;</c> etc. by delegating to the base formatter and casting.
        /// </summary>
        public sealed class SubtypeFormatter<T> : IYamlFormatter<T> where T : YamlNode
        {
            public T Deserialize(ref YamlParser parser, YamlDeserializationContext context)
            {
                var node = context.DeserializeWithAlias(Instance, ref parser);
                if (node is T typed)
                {
                    return typed;
                }
                throw new YamlSerializerException($"Expected a {typeof(T).Name} but got {node.NodeType}.");
            }

            public void Serialize(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
            {
                Instance.Serialize(ref emitter, value, context);
            }
        }

        sealed class ReferenceComparer : IEqualityComparer<YamlNode>
        {
            public static readonly ReferenceComparer Instance = new();
            public bool Equals(YamlNode? x, YamlNode? y) => ReferenceEquals(x, y);
            public int GetHashCode(YamlNode obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
