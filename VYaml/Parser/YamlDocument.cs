#nullable enable
using System;
using System.Collections.Generic;
using VYaml.Internal;
using VYaml.Serialization;

namespace VYaml.Parser
{
    /// <summary>
    /// A single YAML document (one <c>---</c> section), wrapping its root <see cref="YamlNode"/>.
    /// </summary>
    public sealed class YamlDocument
    {
        public YamlNode RootNode { get; }

        public YamlDocument(YamlNode rootNode)
        {
            RootNode = rootNode;
        }

        /// <summary>
        /// Loads the first document of <paramref name="yaml"/>.
        /// </summary>
        public static YamlDocument Load(string yaml, YamlSerializerOptions? options = null)
        {
            return Load(StringEncoding.Utf8.GetBytes(yaml), options);
        }

        /// <summary>
        /// Loads the first document of <paramref name="utf8"/>.
        /// </summary>
        public static YamlDocument Load(ReadOnlyMemory<byte> utf8, YamlSerializerOptions? options = null)
        {
            return new YamlDocument(YamlNode.Parse(utf8, options));
        }

        /// <summary>
        /// Loads every document in <paramref name="yaml"/>.
        /// </summary>
        public static IReadOnlyList<YamlDocument> LoadAll(string yaml, YamlSerializerOptions? options = null)
        {
            return LoadAll(StringEncoding.Utf8.GetBytes(yaml), options);
        }

        /// <summary>
        /// Loads every document in <paramref name="utf8"/>.
        /// </summary>
        public static IReadOnlyList<YamlDocument> LoadAll(ReadOnlyMemory<byte> utf8, YamlSerializerOptions? options = null)
        {
            var documents = new List<YamlDocument>();
            foreach (var node in YamlSerializer.DeserializeMultipleDocuments<YamlNode>(utf8, options))
            {
                documents.Add(new YamlDocument(node));
            }
            return documents;
        }

        public ReadOnlyMemory<byte> ToYaml(YamlSerializerOptions? options = null)
        {
            return YamlSerializer.Serialize(RootNode, options);
        }

        public string ToYamlString(YamlSerializerOptions? options = null)
        {
            return YamlSerializer.SerializeToString(RootNode, options);
        }
    }
}
