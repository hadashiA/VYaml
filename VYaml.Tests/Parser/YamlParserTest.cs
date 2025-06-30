using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using VYaml.Parser;

namespace VYaml.Tests.Parser
{
    [TestFixture]
    public class YamlParserTest
    {
        [Test]
        public void IsNullScalar()
        {
            var parser = CreateParser(new []
            {
                "- null",
                "- ",
                "- ~",
                "- not null",
            });

            parser.SkipAfter(ParseEventType.DocumentStart);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceStart));
            parser.Read();
            Assert.That(parser.IsNullScalar(), Is.True);
            parser.Read();
            Assert.That(parser.IsNullScalar(), Is.True);
            parser.Read();
            Assert.That(parser.IsNullScalar(), Is.True);
            parser.Read();
            Assert.That(parser.IsNullScalar(), Is.False);
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceEnd));
        }

        [Test]
        public void SkipCurrentNode()
        {
            var parser = CreateParser(new []
            {
                "a: 1",
                "b: { ba: 2 }",
                "c: { ca: [100, 200, 300] }",
                "d: { da: [100, 200, 300], db: 100 }",
                "e: { ea: [{eaa: 100}, 200, 300], db: {} }",
                "f: [{ fa: 100, fb: [100, 200, 300] }]",
            });

            parser.SkipAfter(ParseEventType.MappingStart);
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("a"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("b"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("c"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("d"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("e"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("f"));

            parser.Read();
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
        }

        [Test]
        public void Tag_BlockMapping()
        {
            var parser = CreateParser(new []
            {
                "!tag1",
                "a: 100",
                "b: 200",
            });

            parser.SkipAfter(ParseEventType.DocumentStart);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            Assert.That(parser.TryGetCurrentTag(out var tag), Is.True);
            Assert.That(tag.ToString(), Is.EqualTo("!tag1"));
        }

        [Test]
        public void UnityFormat()
        {
            var parser = CreateParser(new []
            {
                "%YAML 1.1",
                "%TAG !u! tag:unity3d.com,2011:",
                "--- !u!29 &1",
                "OcclusionCullingSettings:",
                "  m_ObjectHideFlags: 0",
                "  serializedVersion: 2",
                "  m_OcclusionBakeSettings:",
                "    smallestOccluder: 5",
                "    smallestHole: 0.25",
                "    backfaceThreshold: 100",
                "  m_SceneGUID: 00000000000000000000000000000000",
                "  m_OcclusionCullingData: {fileID: 0}",
                "--- !u!104 &2",
                "RenderSettings:",
                "  m_ObjectHideFlags: 0",
                "  serializedVersion: 9",
                "--- !u!4 &62555683 stripped",
                "Transform:",
                "  m_CorrespondingSourceObject: {fileID: 180319434217191821, guid: 0f48f06ff1ceb490892217c1fb56ad67,",
                "    type: 3}",
                "  m_PrefabInstance: {fileID: 62555682}",
                "  m_PrefabAsset: {fileID: 0}",
                "--- !u!4 &65300469 stripped",
                "Transform:",
                "  m_CorrespondingSourceObject: {fileID: 180319434217191821, guid: 89d43c10426ce4d61b44a4261ce6e27d,",
                "    type: 3}",
                "  m_PrefabInstance: {fileID: 65300468}",
                "  m_PrefabAsset: {fileID: 0}",
                "--- !u!1001 &91330633",
                "PrefabInstance:",
                "  m_ObjectHideFlags: 0",
                "  serializedVersion: 2",
                "  m_Modification:",
                "    serializedVersion: 2",
            });

            parser.SkipAfter(ParseEventType.StreamStart);

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("OcclusionCullingSettings"));
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read();

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("RenderSettings"));
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read();

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("Transform"));
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read();

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("Transform"));
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read();

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("PrefabInstance"));
            parser.SkipCurrentNode();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read();

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.StreamEnd));
            Assert.That(parser.Read(), Is.False);
        }

        [Test]
        public void EmptyElementInSequence()
        {
            var parser = CreateParser(new []
            {
                "keywords:",
                "- ",
                "- _RIDE_ON",
                "- _COME_ON",
            });

            parser.SkipAfter(ParseEventType.DocumentStart);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("keywords"));
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceStart));
            parser.Read();
            Assert.That(parser.IsNullScalar(), Is.True);
            parser.Read();
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("_RIDE_ON"));
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("_COME_ON"));
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
        }

        [Test]
        public void QuotedNumberMustNotBeAnInteger()
        {
            YamlParser parser = CreateParser(new [] {
                "- '012345'",
                "- 012345",
            });

            parser.SkipAfter(ParseEventType.SequenceStart);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.TryReadScalarAsInt32(out _), Is.EqualTo(false));
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("012345"));

            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.TryReadScalarAsInt32(out int value), Is.EqualTo(true));
            Assert.That(value, Is.EqualTo(012345));
        }

        static YamlParser CreateParser(IEnumerable<string> lines)
        {
            var yaml = string.Join("\n", lines);
            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml));
            return new YamlParser(sequence);
        }

        static void CreateParser(string yaml, out YamlParser x)
        {
            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml));
            x = new YamlParser(sequence);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlParser"/> class with the provided YAML content and parser options.
        /// Converts the YAML document into a readable byte sequence and assigns the out parameter to the created parser instance.
        /// </summary>
        /// <param name="yaml">The YAML content to parse, provided as a string.</param>
        /// <param name="x">The output parameter representing the constructed <see cref="YamlParser"/> instance.</param>
        /// <param name="options">The parser options, specified as an instance of <see cref="YamlParserOptions"/>.</param>
        private static void CreateParser(string yaml, out YamlParser x, YamlParserOptions options)
        {
            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml));
            x = new YamlParser(sequence, options);
        }

        /// <summary>
        /// Verifies that comments are emitted as events when <see cref="YamlParserOptions.PreserveComments"/> is enabled.
        /// Tests that the parser correctly identifies and processes comments within a YAML document, adhering to the provided options.
        /// </summary>
        [Test]
        public void CommentEvent_WhenPreserveCommentsEnabled()
        {
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            CreateParser("# This is a comment\nkey: value", out var parser, options);
            
            // Read StreamStart
            Assert.That(parser.Read(), Is.True);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.StreamStart));
            
            // Read DocumentStart
            Assert.That(parser.Read(), Is.True);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            
            // Read Comment
            Assert.That(parser.Read(), Is.True);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" This is a comment"));
            
            // Read MappingStart
            Assert.That(parser.Read(), Is.True);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
        }

        /// <summary>
        /// Ensures that comments are not emitted as events when <see cref="YamlParserOptions.PreserveComments"/> is disabled.
        /// Validates that the parser correctly skips over comments and processes the YAML structure as expected.
        /// </summary>
        [Test]
        public void CommentEvent_NotEmittedWhenPreserveCommentsDisabled()
        {
            CreateParser("# This is a comment\nkey: value", out var parser);
            
            parser.SkipHeader();
            
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
        }

        /// <summary>
        /// Validates the handling of inline comments in a YAML document, ensuring that the parser correctly associates
        /// the comment with the appropriate parse event when <see cref="YamlParserOptions.PreserveComments"/> is enabled.
        /// Verifies the parsing of scalar values and confirms processing of the inline comment following a key-value pair.
        /// </summary>
        [Test]
        public void CommentEvent_InlineComment()
        {
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            CreateParser("key: value # inline comment", out var parser, options);
            
            parser.SkipHeader();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("key"));
            
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("value"));
            
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" inline comment"));
        }

        /// <summary>
        /// Validates the handling and emission of multiple comment events by the YAML parser,
        /// ensuring that comments are recognized as distinct parse events, retained when
        /// <see cref="YamlParserOptions.PreserveComments"/> is enabled, and processed in correct order.
        /// </summary>
        [Test]
        public void CommentEvent_MultipleComments()
        {
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            const string yaml = """
                                # File header comment
                                # Second line of header
                                key1: value1
                                # Comment between entries
                                key2: value2 # inline comment
                                """;
            
            CreateParser(yaml, out var parser, options);
            parser.SkipHeader();
            
            // First comment
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" File header comment"));
            
            parser.Read();
            // Second comment
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" Second line of header"));
            
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
        }
    }
}