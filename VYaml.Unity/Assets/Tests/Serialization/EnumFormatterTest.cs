using NUnit.Framework;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class EnumFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize_AsString()
        {
            Assert.That(Serialize(SimpleEnum.C), Is.EqualTo("c"));
        }

        [Test]
        public void Serialize_WithEnumMember()
        {
            Assert.That(Serialize(EnumMemberLabeledEnum.C), Is.EqualTo("c-alias"));
        }

        [Test]
        public void Serialize_WithDataMember()
        {
            Assert.That(Serialize(DataMemberLabeledEnum.C), Is.EqualTo("c-alias"));
        }

        [Test]
        public void Deserialize_AsString()
        {
            var result = Deserialize<SimpleEnum>("c");
            Assert.That(result, Is.EqualTo(SimpleEnum.C));
        }

        [Test]
        public void Deserialize_WithEnumMember()
        {
            var result = Deserialize<EnumMemberLabeledEnum>("c-alias");
            Assert.That(result, Is.EqualTo(EnumMemberLabeledEnum.C));
        }

        [Test]
        public void Deserialize_WithDataMember()
        {
            var result = Deserialize<DataMemberLabeledEnum>("c-alias");
            Assert.That(result, Is.EqualTo(DataMemberLabeledEnum.C));
        }
    }
}
