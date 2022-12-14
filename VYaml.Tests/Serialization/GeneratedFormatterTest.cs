using NUnit.Framework;
using VYaml.Internal;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class GeneratedFormatterTest : FormatterTestBase
    {
        [Test]
        public void Deserialize_NoMember()
        {
            var result = Deserialize<SimpleTypeZero>("{}");
            Assert.That(result, Is.InstanceOf<SimpleTypeZero>());
        }

        [Test]
        public void Deserialize_PrimitiveMembers()
        {
            var result1 = Deserialize<SimpleTypeOne>("{ one: 100 }");
            Assert.That(result1.One, Is.EqualTo(100));

            var result2 = Deserialize<SimpleTypeTwo>("{ one: 100, two: 200 }");
            Assert.That(result2.One, Is.EqualTo(100));
            Assert.That(result2.Two, Is.EqualTo(200));
        }

        [Test]
        public void Deserialize_Struct()
        {
            var result1 = Deserialize<SimpleUnmanagedStruct>("{ myProperty: 100 }");
            Assert.That(result1.MyProperty, Is.EqualTo(100));

            var result2 = Deserialize<SimpleStruct>("{ myProperty: あいうえお }");
            Assert.That(result2.MyProperty, Is.EqualTo("あいうえお"));
        }

        [Test]
        public void Deserialize_ArrayMember()
        {
            var result1 = Deserialize<WithArray>("{ one: [{ one: 1 }, { one: 2 }] }");
            Assert.That(result1.One!.Length, Is.EqualTo(2));
        }
    }
}