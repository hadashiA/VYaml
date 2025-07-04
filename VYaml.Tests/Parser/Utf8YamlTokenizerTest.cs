using System.Buffers;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using VYaml.Parser;

namespace VYaml.Tests
{
    [TestFixture]
    class Utf8YamlTokenizerTest
    {
        [Test]
        public void Empty()
        {
            CreateTokenizer("", out var tokenizer);
            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));
            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
            Assert.That(tokenizer.Read(), Is.False);
        }

        [Test]
        [TestCase("a scaler")]
        [TestCase("a:,b")]
        [TestCase(":,b")]
        public void PlainScaler(string scalerValue)
        {
            CreateTokenizer(scalerValue, out var reader);
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(reader.TakeCurrentTokenContent<Scalar>().ToString(), Is.EqualTo(scalerValue));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
            Assert.That(reader.Read(), Is.False);
        }

        [Test]
        public void ExplicitScaler()
        {
            CreateTokenizer(new []
            {
                "---",
                "'a scaler'",
                "---",
            }, out var reader);

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.DocumentStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.SingleQuotedScaler));
        }

        [Test]
        public void FlowSequence()
        {
            CreateTokenizer("[item 1, item 2, item 3]", out var tokenizer);

            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.FlowSequenceStart));

            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(tokenizer.TakeCurrentTokenContent<Scalar>().ToString(), Is.EqualTo("item 1"));

            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref tokenizer).ToString(), Is.EqualTo("item 2"));

            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref tokenizer).ToString(), Is.EqualTo("item 3"));

            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.FlowSequenceEnd));

            Assert.That(tokenizer.Read(), Is.True);
            Assert.That(tokenizer.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
            Assert.That(tokenizer.Read(), Is.False);
        }

        [Test]
        public void FlowMapping()
        {
            CreateTokenizer(new []
            {
                "{",
                "  a simple key: a value, # Note that the KEY token is produced.",
                "  ? a complex key: another value,",
                "}"
            }, out var reader);

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a simple key"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a value"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a complex key"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("another value"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowMappingEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));

            Assert.That(reader.Read(), Is.False);
        }

        [Test]
        public void BlockSequences()
        {
            CreateTokenizer(new []
            {
                "- item 1",
                "- item 2",
                "-",
                "  - item 3.1",
                "  - item 3.2",
                "-",
                "  key 1: value 1",
                "  key 2: value 2",
            }, out var reader);

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockSequenceStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockSequenceStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 3.1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 3.2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("key 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("value 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("key 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("value 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
            Assert.That(reader.Read(), Is.False);
        }

        [Test]
        public void BlockMappings()
        {
            CreateTokenizer(new []
            {
                "a simple key: a value   # The KEY token is produced here.",
                "? a complex key",
                ": another value",
                "a mapping:",
                "  key 1: value 1",
                "  key 2: value 2",
                "a sequence:",
                "  - item 1",
                "  - item 2"
            }, out var reader);

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a simple key"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a value"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a complex key"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("another value"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a mapping"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("key 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("value 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("key 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("value 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a sequence"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockSequenceStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
            Assert.That(reader.Read(), Is.False);
        }

        [Test]
        public void NoBlockSequenceStart()
        {
            CreateTokenizer(new []
            {
                "key:",
                "- item 1",
                "- item 2",
            }, out var reader);

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("key"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
            Assert.That(reader.Read(), Is.False);
        }

        [Test]
        public void CollectionsInSequence()
        {
            CreateTokenizer(new []
            {
                "- - item 1",
                "  - item 2",
                "- key 1: value 1",
                "  key 2: value 2",
                "- ? complex key",
                "  : complex value",
            }, out var reader);

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockSequenceStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockSequenceStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("key 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("value 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("key 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("value 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("complex key"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("complex value"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
            Assert.That(reader.Read(), Is.False);
        }

        [Test]
        public void CollectionsInMapping()
        {
            CreateTokenizer(new []
            {
                "? a sequence",
                ": - item 1",
                "  - item 2",
                "? a mapping",
                ": key 1: value 1",
                "  key 2: value 2",
            }, out var reader);

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a sequence"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockSequenceStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("item 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a mapping"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("key 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("value 1"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("key 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("value 2"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
            Assert.That(reader.Read(), Is.False);
        }

        [Test]
        public void SpecEx7_3()
        {
            CreateTokenizer(new []
            {
                "{",
                "    ? foo :,",
                "    : bar,",
                "}"
            }, out var reader);

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("foo"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("bar"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowMappingEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
            Assert.That(reader.Read(), Is.False);
        }

        [Test]
        [Ignore("")]
        public void Mix()
        {
            CreateTokenizer(new[]
            {
                "- item 1",
                "- item 2",
                "-",
                "  - item 3.1",
                "  - item 3.2",
                "-",
                "  key 1: value 1",
                "  key 2: value 2",
                "  key 3: { a: [{x: 100, y: 200}, {x: 300, y: 400}] }"
            }, out var reader);
        }

        [Test]
        [TestCase(':')]
        [TestCase('?')]
        public void PlainScaler_StartingWithIndicatorInFlow(char literal)
        {
            CreateTokenizer($"{{a: {literal}b}}", out var reader);

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowMappingStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo("a"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
            Assert.That(Scalar(ref reader).ToString(), Is.EqualTo($"{literal}b"));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowMappingEnd));

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
            Assert.That(reader.Read(), Is.False);
        }

        [Test]
        [TestCase("! x", "!")]
        [TestCase("!a%21b x", "!a!b")]
        [TestCase("!%F0%9F%98%80", "!😀")]
        [TestCase("!!str x", "!!str")]
        [TestCase("!a!str- x", "!a!str-")]
        [TestCase("!!str, x", "!!str")]
        [TestCase("[!!str], x", "!!str")]
        [TestCase("{!!str}, x", "!!str")]
        [TestCase("[!!map{}, x]", "!!map")]
        [TestCase("[!!seq[], x]", "!!seq")]
        public void Tag(string input, string value)
        {
            CreateTokenizer(input, out var reader);

            while (reader.Read() && reader.CurrentTokenType != TokenType.Tag) {}

            Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.Tag));
            Assert.That(reader.TakeCurrentTokenContent<Tag>().ToString(), Is.EqualTo(value));
        }

        [Test]
        [TestCase("null", ExpectedResult = true)]
        [TestCase("Null", ExpectedResult = true)]
        [TestCase("NULL", ExpectedResult = true)]
        [TestCase("nUll", ExpectedResult = false)]
        [TestCase("null0", ExpectedResult = false)]
        public bool IsNull(string input)
        {
            CreateTokenizer(input, out var tokenizer);
            tokenizer.Read();
            tokenizer.Read();
            return Scalar(ref tokenizer).IsNull();
        }

        static void CreateTokenizer(IEnumerable<string> lines, out Utf8YamlTokenizer tokenizer)
        {
            var yaml = string.Join("\n", lines);
            CreateTokenizer(yaml, out tokenizer);
        }

        static void CreateTokenizer(string yaml, out Utf8YamlTokenizer x)
        {
            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml));
            x = new Utf8YamlTokenizer(sequence);
        }

        static Scalar Scalar(ref Utf8YamlTokenizer tokenizer)
        {
            return tokenizer.TakeCurrentTokenContent<Scalar>();
        }
    }
}
