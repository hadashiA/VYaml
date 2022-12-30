using System.Linq;
using NUnit.Framework;
using VYaml.Internal;
using VYaml.Serialization;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class SerializerTest
    {
        [Test]
        public void DeserializeMultipleDocuments()
        {
            var yamlBytes = StringEncoding.Utf8.GetBytes(SpecExamples.Ex2_28);
            var documents = YamlSerializer.DeserializeMultipleDocuments<dynamic>(yamlBytes).ToArray();
            Assert.That(documents.Length, Is.EqualTo(3));
            Assert.That(documents[0]["Warning"], Is.EqualTo("This is an error message for the log file"));
            Assert.That(documents[1]["Warning"], Is.EqualTo("A slightly different error message."));
            Assert.That(documents[2]["Fatal"], Is.EqualTo("Unknown variable \"bar\""));
        }
    }
}