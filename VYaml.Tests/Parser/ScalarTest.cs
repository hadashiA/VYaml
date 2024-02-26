using NUnit.Framework;
using VYaml.Internal;
using VYaml.Parser;

namespace VYaml.Tests.Parser
{
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
        [TestCase("0o7", ExpectedResult = 7)]
        [TestCase("0o10", ExpectedResult = 8)]
        [TestCase("0o17777777777", ExpectedResult = int.MaxValue)]
        [TestCase("0o00000000000000000000000", ExpectedResult = 0)]
        public int Integer(string input)
        {
            var parsed = FromString(input).TryGetInt32(out var value);
            Assert.That(parsed, Is.True);
            return value;
        }

        [Test]
        [TestCase("0o2000000000000000000000")]
        [TestCase("0o10000000000000000000000")]
        public void IntegerOverflow(string input)
        {
            var parsed = FromString(input).TryGetUInt64(out _);
            Assert.That(parsed, Is.False);
        }

        [Test]
        [TestCase("0.", ExpectedResult = 0.0)]
        [TestCase("-0.0", ExpectedResult = -0.0)]
        [TestCase(".5", ExpectedResult = 0.5)]
        [TestCase("+12e03", ExpectedResult = 12000.0)]
        [TestCase("-2E+05", ExpectedResult = -200000)]
        public double Float(string input)
        {
            var parsed = FromString(input).TryGetDouble(out var value);
            Assert.That(parsed, Is.True);
            return value;
        }

        [Test]
        [TestCase(".NAN")]
        [TestCase(".NaN")]
        [TestCase(".nan")]
        public void Nan(string input)
        {
            var parsed = FromString(input).TryGetDouble(out var value);
            Assert.That(parsed, Is.True);
            Assert.That(double.IsNaN(value), Is.True);
        }

        [Test]
        [TestCase(".inf")]
        [TestCase(".Inf")]
        [TestCase(".INF")]
        public void Infinity(string input)
        {
            var parsed = FromString(input).TryGetDouble(out var value);
            Assert.That(parsed, Is.True);
            Assert.That(double.IsInfinity(value), Is.True);
        }

        [Test]
        [TestCase("-.inf")]
        [TestCase("-.Inf")]
        [TestCase("-.INF")]
        public void NegativeInfinity(string input)
        {
            var parsed = FromString(input).TryGetDouble(out var value);
            Assert.That(parsed, Is.True);
            Assert.That(double.IsNegativeInfinity(value), Is.True);
        }

        static Scalar FromString(string input)
        {
            var bytes = StringEncoding.Utf8.GetBytes(input);
            return new Scalar(bytes);
        }
    }
}
