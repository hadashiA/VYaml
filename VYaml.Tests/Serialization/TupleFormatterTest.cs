using NUnit.Framework;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class TupleFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize_TupleMem()
        {
            var result = Serialize(("item1", "item2"));
            Assert.That(result, Is.EqualTo("00:00:01"));
        }

        [Test]
        public void Deserialize_TupleMember()
        {
            var result1 = Deserialize<WithTuple>(
                "{ one: [1], two: [1,2], three: [1,2,3] }");

            Assert.That(result1.One.Item1, Is.EqualTo(1));

            Assert.That(result1.Two.Item1, Is.EqualTo(1));
            Assert.That(result1.Two.Item2, Is.EqualTo(2));

            Assert.That(result1.Three.Item1, Is.EqualTo(1));
            Assert.That(result1.Three.Item2, Is.EqualTo(2));
            Assert.That(result1.Three.Item3, Is.EqualTo(3));
        }

        [Test]
        public void Deserialize_ValueTupleMember()
        {
            var result1 = Deserialize<WithValueTuple>(
                "{ one: [1], two: [1,2], three: [1,2,3] }");

            Assert.That(result1.One.Item1, Is.EqualTo(1));

            Assert.That(result1.Two.Item1, Is.EqualTo(1));
            Assert.That(result1.Two.Item2, Is.EqualTo(2));

            Assert.That(result1.Three.Item1, Is.EqualTo(1));
            Assert.That(result1.Three.Item2, Is.EqualTo(2));
            Assert.That(result1.Three.Item3, Is.EqualTo(3));
        }
    }
}