using NUnit.Framework;
using VYaml.Internal;
using VYaml.Serialization;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class DeserializeTest
    {
        [Test]
        public void Deserialize_dynamic()
        {
            var bytes = StringEncoding.Utf8.GetBytes(SpecExamples.Ex2_27);
            var result = YamlSerializer.Deserialize<dynamic>(bytes);
            Assert.That(result, Is.Null);
        }
    }
}