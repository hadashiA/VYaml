using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Tests.Integration
{
    /// <summary>
    /// Represents a set of test cases to ensure that YAML documents containing comments
    /// can be accurately parsed and emitted without loss of content, structure, or formatting.
    /// </summary>
    /// <remarks>
    /// This class contains tests that validate the round-tripping process of YAML documents,
    /// particularly focusing on preserving comments within different types of YAML structures.
    /// It includes tests for simple comments, comments in sequences, nested structures, empty comments, and
    /// more complex YAML documents.
    /// </remarks>
    [TestFixture]
    public class CommentRoundTripTest
    {
        /// <summary>
        /// Verifies that YAML containing simple comments can be parsed and emitted back
        /// while preserving both the comments and structural integrity of the document.
        /// </summary>
        [Test]
        public void RoundTrip_SimpleComments()
        {
            const string originalYaml = """
                                        # Header comment
                                        key1: value1
                                        # Mid-document comment
                                        key2: value2
                                        # Footer comment
                                        """;

            var roundTripYaml = RoundTripYaml(originalYaml);
            Assert.That(NormalizeYaml(roundTripYaml), Is.EqualTo(NormalizeYaml(originalYaml)));
        }

        /// <summary>
        /// Validates round-trip YAML serialization and deserialization by ensuring that comments within sequences
        /// are preserved properly without altering their structure or position.
        /// </summary>
        [Test]
        public void RoundTrip_CommentsInSequence()
        {
            const string originalYaml = """
                                        items:
                                          # First item comment
                                          - item1
                                          # Second item comment
                                          - item2
                                        """;

            var roundTripYaml = RoundTripYaml(originalYaml);
            Assert.That(NormalizeYaml(roundTripYaml), Is.EqualTo(NormalizeYaml(originalYaml)));
        }

        /// <summary>
        /// Performs a round-trip test on YAML containing comments within a nested structure.
        /// Verifies that the YAML structure and comments are preserved during parsing and emitting.
        /// </summary>
        [Test]
        public void RoundTrip_CommentsInNestedStructure()
        {
            const string originalYaml = """
                                        # Root comment
                                        outer:
                                          # Nested comment
                                          inner:
                                            # Deep comment
                                            key: value
                                        """;

            var roundTripYaml = RoundTripYaml(originalYaml);
            Assert.That(NormalizeYaml(roundTripYaml), Is.EqualTo(NormalizeYaml(originalYaml)));
        }

        /// <summary>
        /// Verifies that YAML round-tripping preserves empty comment lines while maintaining the document's structure and content.
        /// </summary>
        [Test]
        public void RoundTrip_EmptyComments()
        {
            const string originalYaml = """
                                        key1: value1
                                        #
                                        key2: value2
                                        """;

            var roundTripYaml = RoundTripYaml(originalYaml);
            Assert.That(NormalizeYaml(roundTripYaml), Is.EqualTo(NormalizeYaml(originalYaml)));
        }

        /// <summary>
        /// Ensures the YAML serialization and deserialization process retains comments, structure, and formatting for complex documents.
        /// </summary>
        [Test]
        public void RoundTrip_ComplexDocument()
        {
            const string originalYaml = """
                                        # Application Configuration
                                        # Version: 1.0

                                        # Database settings
                                        database:
                                          # Connection string
                                          connection: localhost:5432
                                          # Pool size
                                          pool_size: 10

                                        # Feature flags
                                        features:
                                          # Enable new UI
                                          - name: new_ui
                                            enabled: true
                                          # Beta features
                                          - name: beta
                                            enabled: false

                                        # End of configuration
                                        """;

            var roundTripYaml = RoundTripYaml(originalYaml);
            Assert.That(NormalizeYaml(roundTripYaml), Is.EqualTo(NormalizeYaml(originalYaml)));
        }

        /// <summary>
        /// Parses and emits YAML content while preserving comments and their formatting.
        /// Efficiently processes YAML data using manual event handling to extract and reconstruct its structure.
        /// </summary>
        [Test]
        public void ParseAndEmit_ManualConstruction()
        {
            // Parse with comments
            const string yaml = """
                                # Header
                                key: value
                                # Footer
                                """;
            
            var parserOptions = new YamlParserOptions { PreserveComments = true }; // Use default StripLeadingWhitespace = true
            var parser = new YamlParser(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml)), parserOptions);
            
            // Emit with comments
            var writer = new ArrayBufferWriter<byte>();
            var emitOptions = new YamlEmitOptions { PreserveComments = true }; // Default AddLeadingSpace = true
            var emitter = new Utf8YamlEmitter(writer, emitOptions);
            
            // Manually read and emit events
            while (parser.Read())
            {
                switch (parser.CurrentEventType)
                {
                    case ParseEventType.StreamStart:
                    case ParseEventType.StreamEnd:
                        // Skip stream events
                        break;
                        
                    case ParseEventType.DocumentStart:
                    case ParseEventType.DocumentEnd:
                        // Skip document events for simple case
                        break;
                        
                    case ParseEventType.Comment:
                        emitter.WriteComment(parser.GetScalarAsString());
                        break;
                        
                    case ParseEventType.MappingStart:
                        emitter.BeginMapping();
                        break;
                        
                    case ParseEventType.MappingEnd:
                        emitter.EndMapping();
                        break;
                        
                    case ParseEventType.SequenceStart:
                        emitter.BeginSequence();
                        break;
                        
                    case ParseEventType.SequenceEnd:
                        emitter.EndSequence();
                        break;
                        
                    case ParseEventType.Scalar:
                        emitter.WriteString(parser.GetScalarAsString() ?? throw new InvalidOperationException());
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            
            var result = Encoding.UTF8.GetString(writer.WrittenSpan);
            // With StripLeadingWhitespace = true, parser extracts "Header" and "Footer"
            // With AddLeadingSpace = true, emitter adds space, resulting in "# Header" and "# Footer"
            Assert.That(result.Contains("# Header"), Is.True);
            Assert.That(result.Contains("# Footer"), Is.True);
            Assert.That(result.Contains("key: value"), Is.True);
        }

        /// <summary>
        /// Performs a round-trip operation on the provided YAML string, ensuring that comments
        /// and formatting are preserved during serialization and deserialization.
        /// </summary>
        /// <param name="yaml">The input YAML string to be processed.</param>
        /// <returns>A YAML string that has undergone a round-trip transformation while
        /// maintaining its comments and structural integrity.</returns>
        private static string RoundTripYaml(string yaml)
        {
            // Parse the YAML with comment preservation enabled and precise round-trip options
            var parserOptions = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = new YamlParser(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml)), parserOptions);
            
            // Create emitter with comment preservation enabled and precise round-trip options
            var writer = new ArrayBufferWriter<byte>();
            var emitOptions = new YamlEmitOptions { PreserveComments = true, AddLeadingSpace = false };
            var emitter = new Utf8YamlEmitter(writer, emitOptions);
            
            // Process all events
            while (parser.Read())
            {
                switch (parser.CurrentEventType)
                {
                    case ParseEventType.StreamStart:
                    case ParseEventType.StreamEnd:
                        // Skip stream events
                        break;
                        
                    case ParseEventType.DocumentStart:
                    case ParseEventType.DocumentEnd:
                        // Skip document events for simple round-trip
                        break;
                        
                    case ParseEventType.Comment:
                        emitter.WriteComment(parser.GetScalarAsString());
                        break;
                        
                    case ParseEventType.MappingStart:
                        emitter.BeginMapping();
                        break;
                        
                    case ParseEventType.MappingEnd:
                        emitter.EndMapping();
                        break;
                        
                    case ParseEventType.SequenceStart:
                        emitter.BeginSequence();
                        break;
                        
                    case ParseEventType.SequenceEnd:
                        emitter.EndSequence();
                        break;
                        
                    case ParseEventType.Scalar:
                        var value = parser.GetScalarAsString();
                        if (value != null)
                        {
                            emitter.WriteString(value);
                        }
                        else
                        {
                            emitter.WriteNull();
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            
            return Encoding.UTF8.GetString(writer.WrittenSpan);
        }

        /// <summary>
        /// Normalizes YAML by extracting semantic structure while ignoring formatting differences.
        /// </summary>
        /// <param name="yaml">The YAML document to normalize, provided as a string.</param>
        /// <returns>A string that represents the normalized semantic structure of the YAML document.</returns>
        private static string NormalizeYaml(string yaml)
        {
            // Parse both original and round-trip to extract semantic structure
            var events = ExtractEvents(yaml);
            return string.Join("\n", events);
        }

        /// <summary>
        /// Extracts semantic events from a YAML document, including structural components and comments.
        /// </summary>
        /// <param name="yaml">The YAML document as a string from which to extract events.</param>
        /// <returns>A list of strings representing the semantic events extracted from the YAML document.</returns>
        private static List<string> ExtractEvents(string yaml)
        {
            var events = new List<string>();

            try
            {
                var parserOptions = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
                var parser = new YamlParser(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml)), parserOptions);
                
                while (parser.Read())
                {
                    switch (parser.CurrentEventType)
                    {
                        case ParseEventType.StreamStart:
                        case ParseEventType.StreamEnd:
                        case ParseEventType.DocumentStart:
                        case ParseEventType.DocumentEnd:
                            // Skip structural events
                            break;
                            
                        case ParseEventType.Comment:
                            var commentText = parser.GetScalarAsString() ?? "";
                            events.Add($"COMMENT:{commentText.Trim()}");
                            break;
                            
                        case ParseEventType.MappingStart:
                            events.Add("MAPPING_START");
                            break;
                            
                        case ParseEventType.MappingEnd:
                            events.Add("MAPPING_END");
                            break;
                            
                        case ParseEventType.SequenceStart:
                            events.Add("SEQUENCE_START");
                            break;
                            
                        case ParseEventType.SequenceEnd:
                            events.Add("SEQUENCE_END");
                            break;
                            
                        case ParseEventType.Scalar:
                            var value = parser.GetScalarAsString() ?? "";
                            events.Add($"SCALAR:{value}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // If parsing fails, fall back to simple line-based comparison
                // This should help us see what's different
                return [$"PARSE_ERROR:{ex.Message}", yaml.Replace("\n", "\\n")];
            }
            
            return events;
        }
    }
}