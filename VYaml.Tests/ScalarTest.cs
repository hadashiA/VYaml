using NUnit.Framework;
using VYaml.Internal;

namespace VYaml.Tests;

[TestFixture]
public class ScalarTest
{
    [Test]
    [TestCase("~")]
    [TestCase("null")]
    [TestCase("Null")]
    [TestCase("NULL")]
    public void Null(string input)
    {
        Assert.That(FromString(input).IsNull, Is.True);
    }

    [Test]
    [TestCase("true", ExpectedResult = true)]
    [TestCase("True", ExpectedResult = true)]
    [TestCase("TRUE", ExpectedResult = true)]
    [TestCase("false", ExpectedResult = false)]
    [TestCase("False", ExpectedResult = false)]
    [TestCase("FALSE", ExpectedResult = false)]
    [TestCase("y", ExpectedResult = true)]
    [TestCase("Y", ExpectedResult = true)]
    [TestCase("yes", ExpectedResult = true)]
    [TestCase("Yes", ExpectedResult = true)]
    [TestCase("YES", ExpectedResult = true)]
    [TestCase("YES", ExpectedResult = true)]
    [TestCase("n", ExpectedResult = false)]
    [TestCase("N", ExpectedResult = false)]
    [TestCase("no", ExpectedResult = false)]
    [TestCase("No", ExpectedResult = false)]
    [TestCase("NO", ExpectedResult = false)]
    [TestCase("on", ExpectedResult = true)]
    [TestCase("On", ExpectedResult = true)]
    [TestCase("ON", ExpectedResult = true)]
    [TestCase("off", ExpectedResult = false)]
    [TestCase("Off", ExpectedResult = false)]
    [TestCase("OFF", ExpectedResult = false)]
    public bool Boolean(string input)
    {
        var parsed = FromString(input).TryGetBool(out var value);
        Assert.That(parsed, Is.True);
        return value;
    }

    [Test]
    [TestCase("123", ExpectedResult = 123)]
    [TestCase("+123", ExpectedResult = 123)]
    [TestCase("-123", ExpectedResult = -123)]
    [TestCase("0xC", ExpectedResult = 12)]
    [TestCase("-0xC", ExpectedResult = -12)]
    public int Integer(string input)
    {
        var parsed = FromString(input).TryGetInt32(out var value);
        Assert.That(parsed, Is.True);
        return value;
    }

    static Scalar FromString(string input)
    {
        var bytes = StringEncoding.Utf8.GetBytes(input);
        return new Scalar(bytes);
    }
}