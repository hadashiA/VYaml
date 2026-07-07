using System.Linq;
using NUnit.Framework;
using VYaml;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;

namespace VYaml.Tests.Parser
{
    [TestFixture]
    public class YamlNodeTest
    {
        [Test]
        public void Parse_Scalar_TypedValues()
        {
            Assert.That(((YamlScalarNode)YamlNode.Parse("123")).GetInt32(), Is.EqualTo(123));
            Assert.That(((YamlScalarNode)YamlNode.Parse("true")).GetBool(), Is.True);
            Assert.That(((YamlScalarNode)YamlNode.Parse("1.5")).GetDouble(), Is.EqualTo(1.5));
            Assert.That(((YamlScalarNode)YamlNode.Parse("hello")).Value, Is.EqualTo("hello"));
        }

        [Test]
        public void Parse_QuotedNumber_IsString()
        {
            var node = (YamlScalarNode)YamlNode.Parse("\"123\"");
            Assert.That(node.Style, Is.EqualTo(ScalarStyle.DoubleQuoted));
            Assert.That(node.TryGetValue(out int _), Is.False);
            Assert.That(node.Value, Is.EqualTo("123"));
        }

        [Test]
        public void Parse_Null()
        {
            Assert.That(((YamlScalarNode)YamlNode.Parse("~")).IsNull, Is.True);
            Assert.That(((YamlScalarNode)YamlNode.Parse("null")).IsNull, Is.True);
        }

        [Test]
        public void Parse_Mapping()
        {
            var node = YamlNode.Parse("a: 1\nb: hello\n").AsMapping();
            Assert.That(node.Count, Is.EqualTo(2));
            Assert.That(node["a"].AsScalar().GetInt32(), Is.EqualTo(1));
            Assert.That(node["b"].AsScalar().Value, Is.EqualTo("hello"));
            Assert.That(node.ContainsKey("a"), Is.True);
            Assert.That(node.ContainsKey("zzz"), Is.False);
        }

        [Test]
        public void Parse_Mapping_PreservesOrder()
        {
            var node = YamlNode.Parse("z: 1\na: 2\nm: 3\n").AsMapping();
            var keys = node.Keys.Cast<YamlScalarNode>().Select(k => k.Value).ToArray();
            Assert.That(keys, Is.EqualTo(new[] { "z", "a", "m" }));
        }

        [Test]
        public void Parse_Sequence()
        {
            var node = YamlNode.Parse("- 1\n- 2\n- 3\n").AsSequence();
            Assert.That(node.Count, Is.EqualTo(3));
            Assert.That(node[0].AsScalar().GetInt32(), Is.EqualTo(1));
            Assert.That(node[2].AsScalar().GetInt32(), Is.EqualTo(3));
        }

        [Test]
        public void Parse_Nested()
        {
            const string yaml = "name: root\nitems:\n  - id: 1\n  - id: 2\n";
            var node = YamlNode.Parse(yaml).AsMapping();
            Assert.That(node["name"].AsScalar().Value, Is.EqualTo("root"));
            var items = node["items"].AsSequence();
            Assert.That(items[0]["id"].AsScalar().GetInt32(), Is.EqualTo(1));
            Assert.That(items[1]["id"].AsScalar().GetInt32(), Is.EqualTo(2));
        }

        [Test]
        public void Parse_Alias_IsSharedReference()
        {
            const string yaml = "a: &anchor\n  x: 1\nb: *anchor\n";
            var node = YamlNode.Parse(yaml).AsMapping();
            Assert.That(ReferenceEquals(node["a"], node["b"]), Is.True);
            Assert.That(node["a"].Anchor!.Name, Is.EqualTo("anchor"));
        }

        [Test]
        public void Parse_ScalarAlias_IsSharedReference()
        {
            const string yaml = "a: &v 42\nb: *v\n";
            var node = YamlNode.Parse(yaml).AsMapping();
            Assert.That(ReferenceEquals(node["a"], node["b"]), Is.True);
            Assert.That(node["b"].AsScalar().GetInt32(), Is.EqualTo(42));
        }

        [Test]
        public void Parse_Tag_IsPreserved()
        {
            var node = (YamlScalarNode)YamlNode.Parse("!!str 123");
            Assert.That(node.Tag, Is.Not.Null);
            Assert.That(node.Tag!.ToString(), Is.EqualTo("!!str"));
        }

        [Test]
        public void LoadAll_MultipleDocuments()
        {
            const string yaml = "a: 1\n---\nb: 2\n";
            var documents = YamlDocument.LoadAll(yaml);
            Assert.That(documents.Count, Is.EqualTo(2));
            Assert.That(documents[0].RootNode["a"].AsScalar().GetInt32(), Is.EqualTo(1));
            Assert.That(documents[1].RootNode["b"].AsScalar().GetInt32(), Is.EqualTo(2));
        }

        [Test]
        public void RoundTrip_Mapping()
        {
            const string yaml = "a: 1\nb: hello\nc:\n  - 1\n  - 2\n";
            var original = YamlNode.Parse(yaml);
            var roundTripped = YamlNode.Parse(YamlSerializer.Serialize(original));

            var map = roundTripped.AsMapping();
            Assert.That(map["a"].AsScalar().GetInt32(), Is.EqualTo(1));
            Assert.That(map["b"].AsScalar().Value, Is.EqualTo("hello"));
            Assert.That(map["c"].AsSequence().Count, Is.EqualTo(2));
        }

        [Test]
        public void Build_AndSerialize()
        {
            var node = new YamlMappingNode
            {
                ["name"] = "vyaml",
                ["count"] = 3,
                ["tags"] = new YamlSequenceNode { "a", "b" },
            };

            var reparsed = YamlNode.Parse(YamlSerializer.Serialize(node)).AsMapping();
            Assert.That(reparsed["name"].AsScalar().Value, Is.EqualTo("vyaml"));
            Assert.That(reparsed["count"].AsScalar().GetInt32(), Is.EqualTo(3));
            Assert.That(reparsed["tags"].AsSequence()[1].AsScalar().Value, Is.EqualTo("b"));
        }

        [Test]
        public void Mutate_AndSerialize()
        {
            var node = YamlNode.Parse("a: 1\n").AsMapping();
            node["a"] = 2;
            node["b"] = "added";

            var reparsed = YamlNode.Parse(YamlSerializer.Serialize(node)).AsMapping();
            Assert.That(reparsed["a"].AsScalar().GetInt32(), Is.EqualTo(2));
            Assert.That(reparsed["b"].AsScalar().Value, Is.EqualTo("added"));
        }

        [Test]
        public void Deserialize_AsSubtype()
        {
            var mapping = YamlSerializer.Deserialize<YamlMappingNode>(
                StringEncodingBytes("a: 1\n"));
            Assert.That(mapping["a"].AsScalar().GetInt32(), Is.EqualTo(1));
        }

        [Test]
        public void RoundTrip_Alias_PreservesSharing()
        {
            const string yaml = "a: &anchor\n  x: 1\nb: *anchor\n";
            var original = YamlNode.Parse(yaml);

            var yamlOut = YamlSerializer.SerializeToString(original);
            var reparsed = YamlNode.Parse(yamlOut).AsMapping();

            Assert.That(ReferenceEquals(reparsed["a"], reparsed["b"]), Is.True,
                $"Sharing was lost. Emitted:\n{yamlOut}");
            Assert.That(reparsed["a"]["x"].AsScalar().GetInt32(), Is.EqualTo(1));
        }

        [Test]
        public void Serialize_SharedInstance_EmitsAnchorAndAlias()
        {
            var shared = new YamlMappingNode { ["x"] = 1 };
            var root = new YamlMappingNode { ["a"] = shared, ["b"] = shared };

            var yamlOut = YamlSerializer.SerializeToString(root);
            StringAssert.Contains("&", yamlOut);
            StringAssert.Contains("*", yamlOut);

            var reparsed = YamlNode.Parse(yamlOut).AsMapping();
            Assert.That(ReferenceEquals(reparsed["a"], reparsed["b"]), Is.True,
                $"Emitted:\n{yamlOut}");
        }

        [Test]
        public void RoundTrip_ScalarAlias()
        {
            const string yaml = "a: &v 42\nb: *v\n";
            var reparsed = YamlNode.Parse(YamlSerializer.Serialize(YamlNode.Parse(yaml))).AsMapping();
            Assert.That(ReferenceEquals(reparsed["a"], reparsed["b"]), Is.True);
            Assert.That(reparsed["b"].AsScalar().GetInt32(), Is.EqualTo(42));
        }

        [Test]
        public void RoundTrip_CyclicMapping()
        {
            var node = new YamlMappingNode { ["name"] = "self" };
            node["self"] = node; // cycle

            var yamlOut = YamlSerializer.SerializeToString(node);
            StringAssert.Contains("&", yamlOut);
            StringAssert.Contains("*", yamlOut);

            var reparsed = YamlNode.Parse(yamlOut).AsMapping();
            Assert.That(ReferenceEquals(reparsed, reparsed["self"]), Is.True,
                $"Self-cycle was not reconstructed. Emitted:\n{yamlOut}");
            Assert.That(reparsed["name"].AsScalar().Value, Is.EqualTo("self"));
        }

        [Test]
        public void RoundTrip_CyclicSequence()
        {
            var node = new YamlSequenceNode { 1 };
            node.Add(node); // cycle

            var yamlOut = YamlSerializer.SerializeToString(node);
            var reparsed = YamlNode.Parse(yamlOut).AsSequence();
            Assert.That(reparsed[0].AsScalar().GetInt32(), Is.EqualTo(1));
            Assert.That(ReferenceEquals(reparsed, reparsed[1]), Is.True,
                $"Self-cycle was not reconstructed. Emitted:\n{yamlOut}");
        }

        [Test]
        public void Parse_SelfReferentialAlias()
        {
            // Spec-valid: the anchor occurs (textually) before the alias inside the same node.
            const string yaml = "&a\nname: root\nself: *a\n";
            var node = YamlNode.Parse(yaml).AsMapping();
            Assert.That(ReferenceEquals(node, node["self"]), Is.True);
            Assert.That(node["name"].AsScalar().Value, Is.EqualTo("root"));
        }

        static System.ReadOnlyMemory<byte> StringEncodingBytes(string s) =>
            System.Text.Encoding.UTF8.GetBytes(s);
    }
}
