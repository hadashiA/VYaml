using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using VYaml.Parser;
using VYaml.Serialization;

namespace VYaml.Tests.Parser
{
    /// <summary>
    /// Unit tests for validating UTF-8 character handling in YAML parsing mechanisms within the VYaml library.
    /// Ensures correctness and robustness when processing full-width Unicode characters and other special cases.
    /// </summary>
    [TestFixture]
    public class Utf8HandlingTest
    {
        /// <summary>
        /// Verifies that the YAML parser does not lock up when parsing a scalar value containing a full-width Unicode number.
        /// Ensures proper handling of full-width characters, such as "２" (U+FF12), within the YAML document.
        /// </summary>
        [Test]
        public void Parse_FullWidthNumber_ShouldNotLockup()
        {
            // Full-width "２" (U+FF12)
            const string yaml = "value: ２";
            var bytes = Encoding.UTF8.GetBytes(yaml);
            
            var parser = YamlParser.FromBytes(bytes);
            parser.SkipAfter(ParseEventType.MappingStart);
            
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("value"));
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("２"));
        }

        /// <summary>
        /// Validates that parsing YAML scalars containing full-width numeric characters
        /// at the end of their value does not cause the parser to lock up.
        /// Iterates through various Unicode full-width numbers.
        /// </summary>
        [Test]
        public void Parse_FullWidthNumbersAtEndOfScalar_ShouldNotLockup()
        {
            // Test various full-width numbers at the end without trailing whitespace
            var fullWidthNumbers = new[] { "０", "１", "２", "３", "４", "５", "６", "７", "８", "９" };
            
            foreach (var num in fullWidthNumbers)
            {
                var yaml = $"value: test{num}";
                var bytes = Encoding.UTF8.GetBytes(yaml);
                
                var parser = YamlParser.FromBytes(bytes);
                parser.SkipAfter(ParseEventType.MappingStart);
                
                Assert.That(parser.ReadScalarAsString(), Is.EqualTo("value"));
                Assert.That(parser.ReadScalarAsString(), Is.EqualTo($"test{num}"));
            }
        }

        /// <summary>
        /// Tests the ability of the YAML parser to correctly handle and parse YAML documents
        /// containing a mix of ASCII and full-width Unicode characters.
        /// This test ensures that both ASCII and full-width characters are parsed without any
        /// errors or unexpected behaviour, preserving the integrity and structure of the content.
        /// Verifies if the parser can handle full-width characters in both keys
        /// and values within a YAML mapping structure correctly.
        /// </summary>
        /// <remarks>
        /// Ensures no regressions related to Unicode handling, particularly the mix of ASCII
        /// and full-width characters in YAML. This test is critical to validate compatibility
        /// with internationalized content.
        /// </remarks>
        [Test]
        public void Parse_MixedAsciiAndFullWidth_ShouldWork()
        {
            const string yaml = """

                                title: Test １２３
                                description: Full-width ＡＢＣ
                                number: ２
                                """;
            
            var bytes = Encoding.UTF8.GetBytes(yaml);
            var parser = YamlParser.FromBytes(bytes);
            
            parser.SkipAfter(ParseEventType.MappingStart);

            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("title"));
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("Test １２３"));
            
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("description"));
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("Full-width ＡＢＣ"));
            
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("number"));
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("２"));
        }

        /// <summary>
        /// Verifies that parsing YAML strings containing an ideographic space (U+3000) does not cause a lockup or infinite loop.
        /// Ensures the ideographic space is treated as part of the scalar value and is parsed correctly.
        /// This test is designed to handle specific edge cases in UTF-8 character processing,
        /// particularly for non-standard whitespace characters.
        /// </summary>
        [Test]
        public void Parse_IdeographicSpace_ShouldNotLockup()
        {
            // Test with ideographic space (U+3000) - verifies no infinite loop
            const string yaml = "value: 　test"; // Note: ideographic space after regular space
            var bytes = Encoding.UTF8.GetBytes(yaml);
            
            var parser = YamlParser.FromBytes(bytes);
            parser.SkipAfter(ParseEventType.MappingStart);
            
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("value"));
            // Ideographic space is part of the scalar value
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("　test"));
        }

        /// <summary>
        /// Validates the YAML parser's ability to correctly handle and parse full-width brackets
        /// within a scalar. This test ensures that full-width Unicode characters
        /// such as ［ and ］ are treated as part of the content and not as flow symbols.
        /// </summary>
        /// <remarks>
        /// This test addresses the proper handling of full-width Unicode characters in YAML
        /// parsing to prevent errors or incorrect parsing behavior.
        /// </remarks>
        [Test]
        public void Parse_FullWidthBrackets_ShouldWork()
        {
            // Test full-width flow symbols
            const string yaml = "array: ［item1， item2］";
            var bytes = Encoding.UTF8.GetBytes(yaml);
            
            var parser = YamlParser.FromBytes(bytes);
            parser.SkipAfter(ParseEventType.MappingStart);
            
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("array"));
            // The parser should treat these as regular content, not flow symbols
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("［item1， item2］"));
        }

        /// <summary>
        /// Verifies the deserialization process of YAML content that includes full-width Unicode characters.
        /// Ensures proper handling for full-width characters in the data, including names, numeric values, and other text.
        /// </summary>
        /// <remarks>
        /// The test examines correct deserialization of YAML content that includes full-width alphanumeric characters
        /// and ideographic text, ensuring compatibility with non-ASCII data. It extracts the data as key-value pairs
        /// and validates the correctness of each value.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if the YAML content is null or invalid for processing.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// Thrown if the required keys (e.g., "name", "age", "city") are not present in the deserialized dictionary.
        /// </exception>
        [Test]
        public void Deserialize_FullWidthContent_ShouldWork()
        {
            const string yaml = """

                                name: 田中太郎
                                age: ２５
                                city: 東京
                                """;
            
            var data = YamlSerializer.Deserialize<Dictionary<string, string>>(Encoding.UTF8.GetBytes(yaml));
            
            Assert.Multiple(() =>
            {
                Assert.That(data["name"], Is.EqualTo("田中太郎"));
                Assert.That(data["age"], Is.EqualTo("２５"));
                Assert.That(data["city"], Is.EqualTo("東京"));
            });
        }

        /// <summary>
        /// Tests that parsing YAML containing a non-breaking space (U+00A0) does not result in a lockup.
        /// This verifies correct handling of the non-breaking space character in the UTF-8 encoded input.
        /// The parser should correctly process the document and include the non-breaking space as part of the scalar value.
        /// </summary>
        [Test]
        public void Parse_NonBreakingSpace_ShouldNotLockup()
        {
            // Test with non-breaking space (U+00A0) - verifies no infinite loop
            var yamlBytes = new byte[] { 
                (byte)'v', (byte)'a', (byte)'l', (byte)'u', (byte)'e', (byte)':', (byte)' ',
                0xC2, 0xA0, // UTF-8 encoding of NBSP
                (byte)'t', (byte)'e', (byte)'s', (byte)'t' 
            };
            
            var parser = YamlParser.FromBytes(yamlBytes);
            parser.SkipAfter(ParseEventType.MappingStart);
            
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("value"));
            // NBSP is part of the scalar value
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("\u00A0test"));
        }

        /// <summary>
        /// Tests that parsing an empty YAML document with a full-width Unicode character does not cause a lockup
        /// in the YAML parser.
        /// This test verifies the YAML parser's ability to handle edge cases where the document contains only
        /// a single full-width Unicode character. It ensures that the parser can correctly handle and process
        /// input without entering an infinite loop or throwing unexpected errors.
        /// Specifically, the test:
        /// - Encodes a string containing a single full-width Unicode character (`２`) as UTF-8 bytes.
        /// - Initializes a `YamlParser` instance with the UTF-8 encoded bytes.
        /// - Parses the document and validates that it reaches the expected scalar event type.
        /// - Confirms that the scalar value matches the input string.
        /// This scenario is critical for applications relying on parsing documents within the context of
        /// multilingual and full-width character support in YAML data processing.
        /// </summary>
        [Test]
        public void Parse_EmptyDocumentWithFullWidth_ShouldNotLockup()
        {
            // Edge case: only full-width character
            const string yaml = "２";
            var bytes = Encoding.UTF8.GetBytes(yaml);
            
            var parser = YamlParser.FromBytes(bytes);
            parser.SkipAfter(ParseEventType.DocumentStart);
            
            Assert.That(parser.CurrentEventType, Is.EqualTo(ParseEventType.Scalar));
            Assert.That(parser.ReadScalarAsString(), Is.EqualTo("２"));
        }
    }
}