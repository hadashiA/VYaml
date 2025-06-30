using System.Buffers;
using System.Text;
using NUnit.Framework;
using VYaml.Parser;

namespace VYaml.Tests.Parser
{
    /// <summary>
    /// Unit test class for validating YAML comment handling functionality in the parser.
    /// This class contains multiple test cases that assess the parser's ability to process
    /// and manage comments in various YAML structures and scenarios, ensuring robust and
    /// accurate handling of comments according to specified options and behaviors.
    /// </summary>
    [TestFixture]
    public class CommentHandlingTest
    {
        /// <summary>
        /// Validates that when a YAML document starts with a comment and, the comment is preserved
        /// properly by the parser when the appropriate parser options are configured.
        /// </summary>
        /// <remarks>
        /// This test ensures that:
        /// - Comments at the document start are correctly identified as a `Comment` event.
        /// - The comment content is correctly retrieved using the parser.
        /// - Events following the comment are correctly processed, such as mapping starts.
        /// It uses a YAML parser configured with the `PreserveComments` and `StripLeadingWhitespace` options.
        /// </remarks>
        [Test]
        public void CommentAtDocumentStart()
        {
            const string yaml = """
                                # Document header comment
                                key: value
                                """;
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);

            // StreamStart
            Assert.That(parser.Read(), Is.True);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.StreamStart));
            
            // DocumentStart
            Assert.That(parser.Read(), Is.True);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            
            // Comment
            Assert.That(parser.Read(), Is.True);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" Document header comment"));
            
            // MappingStart
            Assert.That(parser.Read(), Is.True);
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
        }

        /// <summary>
        /// Tests the handling of YAML comments with leading spaces while parsing.
        /// </summary>
        /// <remarks>
        /// The method verifies that a YAML comment with leading spaces is correctly identified and preserved
        /// when parsing a YAML document. It uses custom parser options to retain comments and avoid stripping
        /// leading whitespace.
        /// </remarks>
        /// <example>
        /// It ensures that the comment is detected as a <see cref="ParseEventType.Comment"/> event,
        /// and the content of the comment, including its leading spaces, can be retrieved as expected.
        /// </example>
        [Test]
        public void CommentWithLeadingSpaces()
        {
            const string yaml = """
                                key: value
                                    # This comment has leading spaces
                                next: value2
                                """;
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            parser.SkipHeader();
            parser.Read(); // key
            parser.Read(); // value
            
            // Comment with leading spaces
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" This comment has leading spaces"));
        }

        /// <summary>
        /// Validates the parsing of YAML documents to ensure the correct handling of blank lines and comments.
        /// </summary>
        /// <remarks>
        /// This method tests the following scenarios:
        /// - YAML documents containing blank lines followed by a comment.
        /// - Preservation of comments when the parser is configured to do so.
        /// - Correct parsing of keys and values in the presence of comments and blank lines.
        /// The method ensures:
        /// - Comments appearing after blank lines are parsed correctly as a comment event.
        /// - YAML scalars (keys and values) are parsed correctly despite the presence of blank lines and comments.
        /// - Configuration options for comment preservation and white-space stripping function as expected.
        /// </remarks>
        [Test]
        public void BlankLinesAndComments()
        {
            const string yaml = """
                                key1: value1

                                # Comment after blank line

                                key2: value2
                                """;
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            parser.SkipHeader();
            
            // key1
            parser.Read();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("key1"));
            
            // value1
            parser.Read();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("value1"));
            
            // Comment
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" Comment after blank line"));
            
            // key2
            parser.Read();
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("key2"));
        }

        /// <summary>
        /// Validates that the YAML parser correctly handles and preserves comments
        /// that are positioned between mapping entries when the parser is configured to do so.
        /// This test ensures that comments within mappings are properly captured and
        /// can be identified as distinct parse events of type <see cref="ParseEventType.Comment"/>.
        /// </summary>
        /// <remarks>
        /// The test utilizes a YAML input containing two key-value pairs separated by a
        /// comment. It confirms that the parser processes the key-value pairs and the
        /// intermediate comment in sequence while adhering to the options specified in
        /// <see cref="YamlParserOptions"/>.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown if the parser does not correctly identify the comment between entries or
        /// if the parsing sequence does not match the expected behavior.
        /// </exception>
        [Test]
        public void CommentBetweenMappingEntries()
        {
            const string yaml = """
                                key1: value1
                                # Comment between entries
                                key2: value2
                                """;
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            parser.SkipHeader();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            
            // key1
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("key1"));
            
            // value1
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("value1"));
            
            // Comment
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" Comment between entries"));
            
            // key2
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("key2"));
        }

        /// <summary>
        /// Tests the correct handling of inline comments that follow a scalar value in a YAML document.
        /// The test verifies that the parser identifies and processes the inline comment
        /// as a comment event after parsing the associated scalar key-value pair.
        /// </summary>
        [Test]
        public void InlineCommentAfterScalar()
        {
            const string yaml = "key: value # inline comment";
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            parser.SkipHeader();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            
            // key
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            
            // value
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            
            // Comment
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" inline comment"));
        }

        /// <summary>
        /// Validates the behavior of parsing a YAML document containing a mid-line comment
        /// preceded by spaces.
        /// </summary>
        /// <remarks>
        /// This method examines how the parser handles comments that appear mid-line in a
        /// YAML document when preceded by spaces. It ensures the comment is correctly
        /// identified and preserved when the appropriate parsing options are enabled.
        /// </remarks>
        /// <seealso cref="YamlParser"/>
        /// <seealso cref="YamlParserOptions"/>
        /// <seealso cref="ParseEventType.Comment"/>
        [Test]
        public void MidLineCommentWithSpaces()
        {
            const string yaml = "key: value    # comment with spaces before it";
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            parser.SkipHeader();
            parser.Read(); // key
            parser.Read(); // value
            
            // Comment
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" comment with spaces before it"));
        }

        /// <summary>
        /// Verifies that comments appearing after a colon in a YAML mapping entry are correctly parsed and preserved when
        /// the PreserveComments option is enabled in the parser.
        /// </summary>
        /// <remarks>
        /// This method tests the parsing of YAML documents with comments immediately following colons within mapping
        /// entries. It ensures that such comments are parsed into separate comment events, preserving their content
        /// as expected, without interfering with the scalar values of keys and values.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown if any part of the parsed YAML document does not match the expected outcome, such as the absence
        /// of expected comment events or mismatched scalar values.
        /// </exception>
        [Test]
        public void CommentAfterColon()
        {
            const string yaml = """
                                key: # comment after colon
                                  value
                                """;
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            parser.SkipHeader();
            
            // key
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("key"));
            
            // Comment
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" comment after colon"));
            
            // value
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("value"));
        }

        /// <summary>
        /// Tests the ability of the YAML parser to correctly handle comments within a sequence.
        /// Verifies that the parser preserves comments when the appropriate option is enabled.
        /// </summary>
        /// <remarks>
        /// The test validates multiple parsing stages, including mapping, sequence, scalar values, and comment handling.
        /// Specific assertions are made to check that comments are recognized as distinct parse events
        /// and their content is preserved as expected.
        /// </remarks>
        [Test]
        public void CommentInSequence()
        {
            const string yaml = """
                                items:
                                  - item1
                                  # Comment in sequence
                                  - item2
                                """;
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            parser.SkipHeader();
            
            // MappingStart
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            
            // key: items
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("items"));
            
            // SequenceStart
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.SequenceStart));
            
            // item1
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("item1"));
            
            // Comment
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" Comment in sequence"));
            
            // item2
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("item2"));
        }

        /// <summary>
        /// Validates the parser's behavior with empty comment lines in a YAML document.
        /// </summary>
        /// <remarks>
        /// The method specifically tests scenarios where a YAML document contains an empty
        /// comment line (a line that only has a '#' with no additional content). It ensures
        /// that the parser correctly identifies this as a comment and assigns it an empty string
        /// as the associated scalar value.
        /// </remarks>
        /// <example>
        /// This test uses a combination of YAML input and parsing settings to simulate and
        /// verify the expected behavior of handling empty comments. It asserts that the
        /// `ParseEventType` for the comment is correctly set to `ParseEventType.Comment`
        /// and that the scalar value of the comment is an empty string.
        /// </example>
        [Test]
        public void EmptyComment()
        {
            const string yaml = """
                                key: value
                                #
                                next: value2
                                """;
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            parser.SkipHeader();
            
            // Skip to after first key-value pair
            parser.Read(); // key
            parser.Read(); // value
            
            // Empty comment
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(""));
        }

        /// <summary>
        /// Ensures that multiple consecutive comments in YAML documents are
        /// correctly parsed and preserved according to the specified parser options.
        /// </summary>
        /// <remarks>
        /// This method verifies the behavior of the parser when handling
        /// multiple consecutive comments. It checks that each comment is
        /// read sequentially with the expected event type and that the content
        /// matches the expected comment text. Additionally, it ensures that
        /// subsequent parsing of other YAML components, such as mappings,
        /// continues correctly after processing the comments.
        /// </remarks>
        [Test]
        public void MultipleConsecutiveComments()
        {
            const string yaml = """
                                # First comment
                                # Second comment
                                # Third comment
                                key: value
                                """;
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            // StreamStart
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.StreamStart));
            
            // DocumentStart
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.DocumentStart));
            
            // First comment
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" First comment"));
            
            // Second comment
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" Second comment"));
            
            // Third comment
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" Third comment"));
            
            // MappingStart
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
        }

        /// <summary>
        /// Tests parsing and handling of comments containing special characters in YAML.
        /// </summary>
        /// <remarks>
        /// This method validates that comments with special characters such as "!@#$%^&*()"
        /// are correctly identified and preserved when the corresponding parsing options
        /// are enabled. It ensures that the parser can maintain comments accurately without
        /// misinterpreting or discarding special characters.
        /// </remarks>
        [Test]
        public void CommentWithSpecialCharacters()
        {
            const string yaml = "key: value # Comment with special chars: !@#$%^&*()";
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            parser.SkipHeader();
            parser.Read(); // key
            parser.Read(); // value
            
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" Comment with special chars: !@#$%^&*()"));
        }

        /// <summary>
        /// Validates the parser's ability to correctly handle comments that resemble YAML content.
        /// </summary>
        /// <remarks>
        /// This test verifies that comments which contain YAML-like syntax, such as `key: value`, are correctly recognized as comments
        /// and not misconstrued as actual YAML mappings. The comments must be preserved when `PreserveComments` is enabled in parser options.
        /// This ensures that the parser's comment handling logic remains robust and accurate.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown if the parser fails to identify the correct event type for the comment or does not preserve the comment content.
        /// </exception>
        [Test]
        public void CommentWithYamlLikeContent()
        {
            const string yaml = """
                                key: value
                                # This looks like YAML: key: value but it's a comment
                                next: value2
                                """;
            var options = new YamlParserOptions { PreserveComments = true, StripLeadingWhitespace = false };
            var parser = CreateParser(yaml, options);
            
            parser.SkipHeader();
            parser.Read(); // key
            parser.Read(); // value
            
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Comment));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo(" This looks like YAML: key: value but it's a comment"));
        }

        /// <summary>
        /// Test method to verify that comments are not preserved in the YAML parsing process
        /// when the <see cref="YamlParserOptions.PreserveComments"/> option is disabled.
        /// </summary>
        /// <remarks>
        /// This test processes a YAML input containing comments and checks that the parser
        /// skips over all comment events and directly parses the mapping and scalar content.
        /// </remarks>
        /// <example>
        /// Parses a YAML document where comments are included but ensures only the mapping
        /// and scalar elements are processed when comment preservation is disabled.
        /// </example>
        [Test]
        public void CommentsNotPreservedWhenDisabled()
        {
            const string yaml = """
                                # Comment
                                key: value # inline comment
                                # Another comment
                                key2: value2
                                """;
            var parser = CreateParser(yaml, YamlParserOptions.Default);
            
            parser.SkipHeader();
            
            // Should skip all comments and go straight to mapping content
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.MappingStart));
            
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("key"));
            
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("value"));
            
            parser.Read();
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.GetScalarAsString(), Is.EqualTo("key2"));
        }

        /// <summary>
        /// Creates and initializes an instance of the <see cref="YamlParser"/> with the provided YAML string and parser options.
        /// </summary>
        /// <param name="yaml">The string containing YAML content to be parsed.</param>
        /// <param name="options">An instance of <see cref="YamlParserOptions"/> specifying parser behavior such as comment preservation and whitespace handling.</param>
        /// <returns>A new instance of <see cref="YamlParser"/> configured with the specified options and ready to parse the provided YAML content.</returns>
        private static YamlParser CreateParser(string yaml, YamlParserOptions options)
        {
            var bytes = Encoding.UTF8.GetBytes(yaml);
            var sequence = new ReadOnlySequence<byte>(bytes);
            return new YamlParser(sequence, options);
        }
    }
}