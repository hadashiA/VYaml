using System.Collections;
using NUnit.Framework;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class BitArrayFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize()
        {
            var value = new BitArray(new[] { true, false, true, false, false });
            var result = Serialize(value);
            Assert.That(result, Is.EqualTo("10100"));
        }

        [Test]
        public void Deserialize()
        {
            var result = Deserialize<BitArray>("111000");
            Assert.That(result, Is.EquivalentTo(new BitArray(new[] { true, true, true, false, false, false })));
        }
    }
}