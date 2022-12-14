using NUnit.Framework;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class EnumFormatterTest : FormatterTestBase
    {
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

        [Test]
        public void Deserialize_WithYamlMember()
        {
            var result = Deserialize<YamlMemberLabeledEnum>("c-alias");
            Assert.That(result, Is.EqualTo(YamlMemberLabeledEnum.C));
        }
    }
}