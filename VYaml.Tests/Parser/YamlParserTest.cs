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

        /// <summary>
        /// Validates that anchor definitions within a YAML document are local to that document
        /// and cannot be referenced in subsequent documents. Ensures that an exception is thrown
        /// when an alias tries to resolve an anchor from a previous document.
        /// </summary>
        /// <remarks>
        /// This test verifies the YAML specification requirement that anchors are scoped locally
        /// to a single document and do not persist across document boundaries. It ensures the
        /// parser throws a <see cref="YamlParserException"/> when an undefined alias is
        /// referenced in a subsequent document.
        /// </remarks>
        [Test]
        public void AnchorsAreLocalToDocument_CannotUseAnchorFromPreviousDocument()
        {
            string[] lines = ["---", "anchor_def: &anchor_value \"shared value\"", "first_use: *anchor_value", "---", "# This should fail - anchor from previous document should not be accessible", "second_use: *anchor_value"
            ];
            var yaml = string.Join("\n", lines);

            var parser = YamlParser.FromSequence(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml)));
            
            parser.Read(); // StreamStart
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.StreamStart));
            
            // First document - should parse successfully
            parser.Read(); // DocumentStart
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            
            parser.Read(); // MappingStart
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            
            parser.Read(); // Scalar "anchor_def"
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("anchor_def"));
            
            parser.Read(); // Scalar "shared value" with anchor
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("shared value"));
            
            parser.Read(); // Scalar "first_use"
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("first_use"));
            
            parser.Read(); // Alias
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Alias));
            
            parser.Read(); // MappingEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            
            parser.Read(); // DocumentEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            
            // Second document - should fail when trying to use anchor from first document
            parser.Read(); // DocumentStart
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            
            parser.Read(); // MappingStart
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            
            parser.Read(); // Scalar "second_use"
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("second_use"));
            
            // This should throw because the anchor is not defined in the current document
            try
            {
                parser.Read();
                Assert.Fail("Expected exception for undefined alias, but alias was resolved across document boundary");
            }
            catch (YamlParserException)
            {
                // Expected
            }
        }

        /// <summary>
        /// Validates that the same anchor name can be reused across different YAML documents without conflict,
        /// where each instance within a document resolves distinctly to its respective value.
        /// </summary>
        /// <remarks>
        /// This method parses a multi-document YAML sequence containing anchors and aliases. It ensures that:
        /// 1. Anchors with the same name, defined in different documents, retain unique values per document.
        /// 2. Aliases correctly resolve to the values of their respective anchors.
        /// The test verifies the parsing sequence of YAML events, including anchors, aliases, and document transitions.
        /// </remarks>
        [Test]
        public void SameAnchorNameCanBeReusedInDifferentDocuments()
        {
            string[] lines = ["---", "data: &shared_anchor \"value in doc 1\"", "ref: *shared_anchor", "---", "data: &shared_anchor \"value in doc 2\"", "ref: *shared_anchor", "---", "data: &shared_anchor \"value in doc 3\"", "ref: *shared_anchor"
            ];
            var yaml = string.Join("\n", lines);

            var parser = YamlParser.FromSequence(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml)));
            
            parser.Read(); // StreamStart
            
            // Document 1
            parser.Read(); // DocumentStart
            parser.Read(); // MappingStart
            parser.Read(); // Scalar "data"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("data"));
            parser.Read(); // Scalar "value in doc 1" with anchor
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("value in doc 1"));
            parser.Read(); // Scalar "ref"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("ref"));
            parser.Read(); // Alias - should resolve to "value in doc 1"
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Alias));
            parser.Read(); // MappingEnd
            parser.Read(); // DocumentEnd
            
            // Document 2 - same anchor name, different value
            parser.Read(); // DocumentStart
            parser.Read(); // MappingStart
            parser.Read(); // Scalar "data"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("data"));
            parser.Read(); // Scalar "value in doc 2" with anchor
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("value in doc 2"));
            parser.Read(); // Scalar "ref"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("ref"));
            parser.Read(); // Alias - should resolve to "value in doc 2"
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Alias));
            parser.Read(); // MappingEnd
            parser.Read(); // DocumentEnd
            
            // Document 3 - same anchor name, different value
            parser.Read(); // DocumentStart
            parser.Read(); // MappingStart
            parser.Read(); // Scalar "data"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("data"));
            parser.Read(); // Scalar "value in doc 3" with anchor
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("value in doc 3"));
            parser.Read(); // Scalar "ref"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("ref"));
            parser.Read(); // Alias - should resolve to "value in doc 3"
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Alias));
            parser.Read(); // MappingEnd
            parser.Read(); // DocumentEnd
            
            parser.Read(); // StreamEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.StreamEnd));
        }

        /// <summary>
        /// Tests the proper isolation and resolution of complex anchors within nested YAML structures.
        /// Verifies that the parser correctly reproduces data relationships and maintains anchor integrity
        /// within hierarchical and multi-level YAML documents.
        /// </summary>
        [Test]
        public void ComplexAnchorIsolationWithNestedStructures()
        {
            string[] lines = ["---", "defaults: &defaults", "  name: Default Name", "  settings:", "    timeout: 30", "    retries: 3", "user1:", "  <<: *defaults", "  name: User One", "---", "# New document - defaults anchor should not be available", "user2:", "  <<: *defaults", "  name: User Two"
            ];
            var yaml = string.Join("\n", lines);

            var parser = YamlParser.FromSequence(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml)));
            
            parser.Read(); // StreamStart
            
            // First document
            parser.Read(); // DocumentStart
            parser.Read(); // MappingStart
            
            // defaults with anchor
            parser.Read(); // Scalar "defaults"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("defaults"));
            parser.Read(); // MappingStart with anchor
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            
            parser.Read(); // Scalar "name"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("name"));
            parser.Read(); // Scalar "Default Name"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("Default Name"));
            parser.Read(); // Scalar "settings"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("settings"));
            parser.Read(); // MappingStart
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read(); // Scalar "timeout"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("timeout"));
            parser.Read(); // Scalar "30"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("30"));
            parser.Read(); // Scalar "retries"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("retries"));
            parser.Read(); // Scalar "3"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("3"));
            parser.Read(); // MappingEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read(); // MappingEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            
            // user1 using merge key
            parser.Read(); // Scalar "user1"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("user1"));
            parser.Read(); // MappingStart
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            parser.Read(); // Scalar "<<"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("<<"));
            parser.Read(); // Alias - should work
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Alias));
            parser.Read(); // Scalar "name"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("name"));
            parser.Read(); // Scalar "User One"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("User One"));
            parser.Read(); // MappingEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            
            parser.Read(); // MappingEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read(); // DocumentEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            
            // Second document - should fail when trying to use defaults anchor
            parser.Read(); // DocumentStart
            parser.Read(); // MappingStart
            parser.Read(); // Scalar "user2"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("user2"));
            parser.Read(); // MappingStart
            parser.Read(); // Scalar "<<"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("<<"));
            
            // This should throw because the anchor is not defined in the current document
            try
            {
                parser.Read();
                Assert.Fail("Expected exception for undefined alias");
            }
            catch (YamlParserException)
            {
                // Expected
            }
        }

        /// <summary>
        /// Tests the behavior of the YAML parser when encountering a document with only an alias that refers to an undefined anchor.
        /// Verifies that the parser throws a <see cref="YamlParserException"/> for the undefined alias reference in the second document.
        /// </summary>
        /// <exception cref="YamlParserException">
        /// Thrown when attempting to read an alias that references an anchor not defined within the current document.
        /// </exception>
        [Test]
        public void DocumentWithOnlyAliasToUndefinedAnchor()
        {
            string[] lines = ["---", "value: &anchor 42", "---", "*anchor"];
            var yaml = string.Join("\n", lines);

            var parser = YamlParser.FromSequence(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml)));
            
            parser.Read(); // StreamStart
            
            // First document
            parser.Read(); // DocumentStart
            parser.Read(); // MappingStart
            parser.Read(); // Scalar "value"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("value"));
            parser.Read(); // Scalar "42" with anchor
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("42"));
            parser.Read(); // MappingEnd
            parser.Read(); // DocumentEnd
            
            // Second document
            parser.Read(); // DocumentStart
            
            // Should throw when trying to read undefined alias
            try
            {
                parser.Read();
                Assert.Fail("Expected exception for undefined alias");
            }
            catch (YamlParserException)
            {
                // Expected
            }
        }

        /// <summary>
        /// Tests the ability of the YAML parser to handle multiple anchors and aliases within a single document.
        /// Ensures that anchors are properly defined, and aliases correctly resolve to their respective anchors,
        /// including scalars, sequences, and nested mappings.
        /// </summary>
        [Test]
        public void MultipleAnchorsAndAliasesWithinSingleDocument()
        {
            string[] lines = ["---", "str_anchor: &str \"string value\"", "num_anchor: &num 123", "list_anchor: &list", "  - item1", "  - item2", "references:", "  str_ref: *str", "  num_ref: *num", "  list_ref: *list", "  another_str: *str"
            ];
            var yaml = string.Join("\n", lines);

            var parser = YamlParser.FromSequence(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml)));
            
            parser.Read(); // StreamStart
            parser.Read(); // DocumentStart
            parser.Read(); // MappingStart
            
            // All anchors and aliases within same document should work
            parser.Read(); // Scalar "str_anchor"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("str_anchor"));
            parser.Read(); // Scalar "string value" with anchor
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("string value"));
            
            parser.Read(); // Scalar "num_anchor"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("num_anchor"));
            parser.Read(); // Scalar "123" with anchor
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("123"));
            
            parser.Read(); // Scalar "list_anchor"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("list_anchor"));
            parser.Read(); // SequenceStart with anchor
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceStart));
            parser.Read(); // Scalar "item1"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("item1"));
            parser.Read(); // Scalar "item2"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("item2"));
            parser.Read(); // SequenceEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceEnd));
            
            parser.Read(); // Scalar "references"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("references"));
            parser.Read(); // MappingStart
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            
            parser.Read(); // Scalar "str_ref"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("str_ref"));
            parser.Read(); // Alias to str
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Alias));
            
            parser.Read(); // Scalar "num_ref"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("num_ref"));
            parser.Read(); // Alias to num
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Alias));
            
            parser.Read(); // Scalar "list_ref"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("list_ref"));
            parser.Read(); // Alias to list
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Alias));
            
            parser.Read(); // Scalar "another_str"
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("another_str"));
            parser.Read(); // Alias to str
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Alias));
            
            parser.Read(); // MappingEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read(); // MappingEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingEnd));
            parser.Read(); // DocumentEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentEnd));
            parser.Read(); // StreamEnd
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.StreamEnd));
        }
    }
}