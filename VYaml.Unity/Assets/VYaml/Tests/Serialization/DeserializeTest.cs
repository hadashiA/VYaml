using NUnit.Framework;
using VYaml.Internal;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class DeserializeTest
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
            var result = Deserialize<SimpleTypeOne>("{ one: 100 }");
            Assert.That(result, Is.InstanceOf<SimpleTypeOne>());
            Assert.That(result.One, Is.EqualTo(100));
        }

        static T Deserialize<T>(string yaml)
        {
            var bytes = StringEncoding.Utf8.GetBytes(yaml);
            return YamlSerializer.Deserialize<T>(bytes);
        }
    }
}