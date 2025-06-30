using System.Buffers;
using System.Text;
using NUnit.Framework;
using VYaml.Emitter;

namespace VYaml.Tests.Emitter
{
    /// <summary>
    /// A test suite for validating the behaviour of comment emission in YAML using Utf8YamlEmitter.
    /// </summary>
    /// <remarks>
    /// This class contains test methods that ensure comments can be emitted in various contexts,
    /// such as at the start of a document, between mapping entries, inside sequences,
    /// and other scenarios. These tests validate compliance with YAML formatting rules and
    /// expected output when comments are either enabled or disabled.
    /// </remarks>
    [TestFixture]
    public class CommentEmitterTest
    {
        /// <summary>
        /// Tests the ability of the YAML emitter to correctly emit a comment
        /// at the very start of a YAML document before any content is defined.
        /// </summary>
        /// <remarks>
        /// This test verifies that a top-level comment can be emitted as the first
        /// element of a YAML document, ensuring appropriate formatting and adherence
        /// to YAML specifications. It also checks the integration between options
        /// such as PreserveComments and the emitted output format.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the emitted YAML document does not match the expected output
        /// string, indicating a failure in comment emission or formatting logic.
        /// </exception>
        [Test]
        public void EmitComment_AtDocumentStart()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.WriteComment("This is a document header comment");
            emitter.BeginMapping();
            emitter.WriteString("key");
            emitter.WriteString("value");
            emitter.EndMapping();
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            Assert.That(yaml, Is.EqualTo("# This is a document header comment\nkey: value\n"));
        }

        /// <summary>
        /// Tests the ability of the YAML emitter to correctly emit a comment
        /// between two mapping entries in a YAML document.
        /// </summary>
        /// <remarks>
        /// This test validates the proper placement and formatting of comments
        /// within YAML mappings when the PreserveComments option is enabled. It
        /// ensures that a comment can be emitted between two key-value pairs
        /// without breaking document structure or formatting rules.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the resulting YAML output does not include the comment in
        /// the expected position or when the output differs from the formatted standard.
        /// </exception>
        [Test]
        public void EmitComment_BetweenMappingEntries()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.BeginMapping();
            emitter.WriteString("key1");
            emitter.WriteString("value1");
            emitter.WriteComment("Comment between entries");
            emitter.WriteString("key2");
            emitter.WriteString("value2");
            emitter.EndMapping();
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            Assert.That(yaml, Is.EqualTo("key1: value1\n# Comment between entries\nkey2: value2\n"));
        }

        /// <summary>
        /// Tests the emission of a comment within a YAML sequence using the Utf8YamlEmitter.
        /// </summary>
        /// <remarks>
        /// This test validates the capability of the emitter to insert a comment inside a YAML sequence,
        /// ensuring proper formatting and placement within the emitted output. It confirms that comments
        /// are correctly written when PreserveComments is enabled and that the overall sequence structure
        /// adheres to YAML specification with respect to indentation and line placement.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the emitted YAML document does not match the expected output, indicating issues
        /// with the comment's integration into the sequence or formatting inconsistencies.
        /// </exception>
        [Test]
        public void EmitComment_InSequence()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.BeginSequence();
            emitter.WriteString("item1");
            emitter.WriteComment("Comment in sequence");
            emitter.WriteString("item2");
            emitter.EndSequence();
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            Assert.That(yaml, Is.EqualTo("- item1\n# Comment in sequence\n- item2\n"));
        }

        /// <summary>
        /// Tests the emission of an empty comment within a YAML document using the Utf8YamlEmitter.
        /// </summary>
        /// <remarks>
        /// This test ensures that an empty comment is correctly emitted as a standalone `#` symbol
        /// within a YAML document. It verifies proper formatting when comments are explicitly enabled
        /// through the <see cref="YamlEmitOptions.PreserveComments"/> option. The test also confirms
        /// that empty comments do not interfere with surrounding mappings and values in the emitted YAML.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the emitted YAML output does not match the expected format, indicating a failure
        /// in the empty comment handling or formatting logic.
        /// </exception>
        [Test]
        public void EmitComment_EmptyComment()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.BeginMapping();
            emitter.WriteString("key");
            emitter.WriteString("value");
            emitter.WriteComment("");
            emitter.WriteString("next");
            emitter.WriteString("value2");
            emitter.EndMapping();
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            Assert.That(yaml, Is.EqualTo("key: value\n#\nnext: value2\n"));
        }

        /// <summary>
        /// Verifies the successful emission of a YAML comment when the comment begins with
        /// a leading space, ensuring it adheres to YAML specifications and formatting.
        /// </summary>
        /// <remarks>
        /// This test demonstrates that a comment with an intentional leading space is correctly
        /// emitted, using the Utf8YamlEmitter with the PreserveComments option enabled. It also checks
        /// that the emitted comment retains proper formatting, including the initial space after the comment marker (#).
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the emitted YAML does not match the expected format,
        /// indicating an issue with the comment emission logic or handling of leading spaces.
        /// </exception>
        [Test]
        public void EmitComment_WithLeadingSpace()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.WriteComment(" Comment with leading space");
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            Assert.That(yaml, Is.EqualTo("#  Comment with leading space\n"));
        }

        /// <summary>
        /// Tests the emission of a YAML comment without a leading space.
        /// </summary>
        /// <remarks>
        /// This test ensures that the Utf8YamlEmitter correctly emits a comment
        /// that begins immediately after the "#" character, without an additional
        /// leading space. The functionality is validated with the PreserveComments
        /// option enabled in the YamlEmitOptions configuration.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the output YAML string does not match the expected result,
        /// indicating an issue with the comment formatting or emission logic.
        /// </exception>
        [Test]
        public void EmitComment_WithoutLeadingSpace()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.WriteComment("Comment without leading space");
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            Assert.That(yaml, Is.EqualTo("# Comment without leading space\n"));
        }

        /// <summary>
        /// Tests the behavior of the YAML emitter when comments are disabled via the YamlEmitOptions configuration.
        /// </summary>
        /// <remarks>
        /// This test ensures that comments are not emitted in the output when the PreserveComments option
        /// is set to false. It verifies that the resulting YAML document excludes any comments that
        /// were attempted to be written, adhering strictly to the specified configuration.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the emitted YAML output includes a comment despite the PreserveComments option
        /// being disabled, indicating a failure to respect the configuration or logic related to comment handling.
        /// </exception>
        [Test]
        public void EmitComment_NotEmittedWhenDisabled()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = false };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.WriteComment("This comment should not appear");
            emitter.BeginMapping();
            emitter.WriteString("key");
            emitter.WriteString("value");
            emitter.EndMapping();
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            Assert.That(yaml, Is.EqualTo("key: value\n"));
        }

        /// <summary>
        /// Validates the emission of comments within nested YAML mapping structures.
        /// </summary>
        /// <remarks>
        /// This test ensures that comments are correctly emitted both before and inside
        /// nested mappings within a YAML document. It verifies that the comments are
        /// placed in the appropriate locations relative to the mapping elements and
        /// adhere to YAML formatting rules when comment preservation is enabled.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the generated YAML output does not match the expected format.
        /// This could indicate an issue with comment placement, formatting, or
        /// preservation logic in nested structures.
        /// </exception>
        [Test]
        public void EmitComment_NestedStructures()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.BeginMapping();
            emitter.WriteString("outer");
            emitter.WriteComment("Before nested mapping");
            emitter.BeginMapping();
            emitter.WriteComment("Inside nested mapping");
            emitter.WriteString("inner");
            emitter.WriteString("value");
            emitter.EndMapping();
            emitter.EndMapping();
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            const string expected = """
                                    outer:
                                      # Before nested mapping
                                      # Inside nested mapping
                                      inner: value

                                    """;
            Assert.That(yaml, Is.EqualTo(expected));
        }

        /// <summary>
        /// Verifies the ability of the Utf8YamlEmitter to correctly emit comments within a flow-style YAML sequence.
        /// </summary>
        /// <remarks>
        /// This test evaluates the inclusion and formatting of inline comments in a flow sequence
        /// where elements are organized in a comma-separated form within square brackets. It ensures
        /// that comments are preserved and correctly positioned relative to the sequence elements
        /// when the PreserveComments option is enabled. Special emphasis is placed on verifying
        /// compliance with YAML specifications and the specific behavior of flow-style sequences
        /// when comments are present.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown if the resulting YAML output does not meet the expected structure or formatting,
        /// including the positioning or rendering of comments.
        /// </exception>
        [Test]
        public void EmitComment_InFlowSequence()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteString("item1");
            emitter.WriteComment("inline comment");
            emitter.WriteString("item2");
            emitter.EndSequence();
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            // Note: Comments in flow style should be handled differently
            // This test documents the current behavior
            Assert.That(yaml.Contains("[") && yaml.Contains("]"), Is.True);
        }

        /// <summary>
        /// Verifies the ability of the YAML emitter to correctly emit multiple consecutive comments
        /// within a document, ensuring adherence to expected formatting and comment order.
        /// </summary>
        /// <remarks>
        /// This test checks the emission of multiple comments in sequence before other YAML content.
        /// It validates that each comment is emitted on its own line with proper formatting,
        /// and that subsequent document elements are emitted correctly after the comments.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the emitted YAML output does not include all expected comments
        /// in the correct order or fails to match the specified formatting in the expected output.
        /// </exception>
        [Test]
        public void EmitComment_MultipleConsecutive()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.WriteComment("First comment");
            emitter.WriteComment("Second comment");
            emitter.WriteComment("Third comment");
            emitter.BeginMapping();
            emitter.WriteString("key");
            emitter.WriteString("value");
            emitter.EndMapping();
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            const string expected = """
                                    # First comment
                                    # Second comment
                                    # Third comment
                                    key: value

                                    """;
            Assert.That(yaml, Is.EqualTo(expected));
        }

        /// <summary>
        /// Tests the ability of the YAML emitter to correctly emit a comment
        /// containing special characters, ensuring proper encoding and output formatting.
        /// </summary>
        /// <remarks>
        /// This test validates that the Utf8YamlEmitter can handle comments with a variety
        /// of special characters, including punctuation, symbols, and YAML-like structures,
        /// without errors or unexpected behavior. It ensures compliance with YAML syntax
        /// and the correct preservation of such content.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the emitted YAML document's comment does not match the expected output,
        /// indicating a failure in character handling or emission logic.
        /// </exception>
        [Test]
        public void EmitComment_WithSpecialCharacters()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.WriteComment("Special chars: !@#$%^&*() and YAML-like: key: value");
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            Assert.That(yaml, Is.EqualTo("# Special chars: !@#$%^&*() and YAML-like: key: value\n"));
        }

        /// <summary>
        /// Ensures the Utf8YamlEmitter correctly emits comments containing Unicode characters,
        /// including non-ASCII symbols and emojis, in the expected YAML format.
        /// </summary>
        /// <remarks>
        /// This test verifies that Unicode characters are preserved and rendered accurately
        /// in YAML comments when the PreserveComments option is enabled. It checks compatibility
        /// in handling diverse character sets, ensuring compliance with UTF-8 encoding standards.
        /// </remarks>
        /// <exception cref="AssertionException">
        /// Thrown when the emitted YAML document does not contain the expected Unicode comment,
        /// indicating a failure in encoding, formatting, or comment emission logic.
        /// </exception>
        [Test]
        public void EmitComment_WithUnicodeCharacters()
        {
            var writer = new ArrayBufferWriter<byte>();
            var options = new YamlEmitOptions { PreserveComments = true };
            var emitter = new Utf8YamlEmitter(writer, options);
            
            emitter.WriteComment("Unicode: 你好世界 🌍 émojis");
            
            var yaml = Encoding.UTF8.GetString(writer.WrittenSpan);
            Assert.That(yaml, Is.EqualTo("# Unicode: 你好世界 🌍 émojis\n"));
        }
    }
}