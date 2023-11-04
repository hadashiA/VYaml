using System.Linq;
using NUnit.Framework;
using VYaml.Internal;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class SerializerTest
    {
        [Test]
        public void Deserialize_ExplicitDefaultValueFromConstructor()
        {
            var yamlBytes = StringEncoding.Utf8.GetBytes("valueSet: 22");
            var value = YamlSerializer.Deserialize<WithDefaultValue>(yamlBytes);
            Assert.That(value.Value, Is.EqualTo(12));
            Assert.That(value.ValueSet, Is.EqualTo(22));
        }

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

        [Test]
        public void DeserializeHighDigitNumber()
        {
            var yamlBytes = StringEncoding.Utf8.GetBytes("id1: 8083928222794209684\n" +
                                                         "id2: 123\n" +
                                                         "id3: 8083928222794209684.123456789\n");
            var documents = YamlSerializer.Deserialize<dynamic>(yamlBytes);
            Assert.That(documents.Count, Is.EqualTo(3));
            Assert.That(documents["id1"], Is.InstanceOf<long>());
            Assert.That(documents["id1"], Is.EqualTo(8083928222794209684));
            Assert.That(documents["id2"], Is.InstanceOf<int>());
            Assert.That(documents["id3"], Is.InstanceOf<double>());
        }
    }
}