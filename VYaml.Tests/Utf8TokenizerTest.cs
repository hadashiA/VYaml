using System.Buffers;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace VYaml.Tests;

[TestFixture]
class Utf8TokenizerTest
{
    [Test]
    public void Empty()
    {
        CreateReader("", out var reader);
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
        Assert.That(reader.Read(), Is.False);
    }

    [Test]
    [TestCase("a scaler")]
    [TestCase("a:,b")]
    [TestCase(":,b")]
    public void PlainScaler(string scalerValue)
    {
        CreateReader(scalerValue, out var reader);
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo(scalerValue));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
        Assert.That(reader.Read(), Is.False);
    }

    [Test]
    public void ExplicitScaler()
    {
        CreateReader(new []
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
        CreateReader("[item 1, item 2, item 3]", out var reader);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowSequenceStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 2"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 3"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowSequenceEnd));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
        Assert.That(reader.Read(), Is.False);
    }

    [Test]
    public void FlowMapping()
    {
        CreateReader(new []
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
        Assert.That(reader.GetString(), Is.EqualTo("a simple key"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("a value"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("a complex key"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("another value"));

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
        CreateReader(new []
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
        Assert.That(reader.GetString(), Is.EqualTo("item 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 2"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockSequenceStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 3.1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 3.2"));

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
        Assert.That(reader.GetString(), Is.EqualTo("key 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("value 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("key 2"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("value 2"));

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
        CreateReader(new []
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
        Assert.That(reader.GetString(), Is.EqualTo("a simple key"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("a value"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("a complex key"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("another value"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("a mapping"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockMappingStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("key 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("value 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("key 2"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("value 2"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("a sequence"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockSequenceStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 2"));

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
        CreateReader(new []
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
        Assert.That(reader.GetString(), Is.EqualTo("key"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 2"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
        Assert.That(reader.Read(), Is.False);
    }

    [Test]
    public void CollectionsInSequence()
    {
        CreateReader(new []
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
        Assert.That(reader.GetString(), Is.EqualTo("item 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 2"));

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
        Assert.That(reader.GetString(), Is.EqualTo("key 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("value 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("key 2"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("value 2"));

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
        Assert.That(reader.GetString(), Is.EqualTo("complex key"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("complex value"));

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
        CreateReader(new []
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
        Assert.That(reader.GetString(), Is.EqualTo("a sequence"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockSequenceStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEntryStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("item 2"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockEnd));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("a mapping"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.BlockMappingStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("key 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("value 1"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("key 2"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("value 2"));

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
        CreateReader(new []
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
        Assert.That(reader.GetString(), Is.EqualTo("foo"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));
        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("bar"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowEntryStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowMappingEnd));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
        Assert.That(reader.Read(), Is.False);
    }

    [Test]
    public void Mix()
    {
        CreateReader(new[]
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

        while (reader.Read())
        {
            TestContext.Out.WriteLine($"{reader.CurrentTokenType}");
            if (reader.CurrentTokenType == TokenType.PlainScalar)
            {
                TestContext.Out.WriteLine(reader.GetString());
            }
        }
    }

    [Test]
    [TestCase(':')]
    [TestCase('?')]
    public void PlainScaler_StartingWithIndicatorInFlow(char literal)
    {
        CreateReader($"{{a: {literal}b}}", out var reader);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowMappingStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.KeyStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo("a"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.ValueStart));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.PlainScalar));
        Assert.That(reader.GetString(), Is.EqualTo($"{literal}b"));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.FlowMappingEnd));

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.CurrentTokenType, Is.EqualTo(TokenType.StreamEnd));
        Assert.That(reader.Read(), Is.False);
    }

    [Test]
    [TestCase("null", ExpectedResult = true)]
    [TestCase("Null", ExpectedResult = true)]
    [TestCase("NULL", ExpectedResult = true)]
    [TestCase("nUll", ExpectedResult = false)]
    [TestCase("null0", ExpectedResult = false)]
    public bool IsNull(string input)
    {
        CreateReader(input, out var reader);
        reader.Read();
        reader.Read();
        return reader.IsNull();
    }

    static void CreateReader(IEnumerable<string> lines, out Utf8Tokenizer tokenizer)
    {
        var yaml = string.Join('\n', lines);
        CreateReader(yaml, out tokenizer);
    }

    static void CreateReader(string yaml, out Utf8Tokenizer x)
    {
        var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(yaml));
        x = new Utf8Tokenizer(sequence);
    }
}