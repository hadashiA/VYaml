using NUnit.Framework;
using VYaml.Annotations;
using VYaml.Serialization;
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
        public void Serialize_NamingConventionOptions()
        {
            var options = new YamlSerializerOptions
            {
                NamingConvention = NamingConvention.UpperCamelCase
            };
            Assert.That(Serialize(SimpleEnum.A, options), Is.EqualTo("A"));
            Assert.That(Serialize(NamingConventionEnum.HogeFuga, options), Is.EqualTo("hoge_fuga"));
            Assert.That(Serialize(DataMemberLabeledEnum.C, options), Is.EqualTo("c-alias"));
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

        [Test]
        public void Deserialize_NamingConventionOptions()
        {
            var options = new YamlSerializerOptions
            {
                NamingConvention = NamingConvention.UpperCamelCase
            };
            Assert.That(Deserialize<SimpleEnum>("A", options), Is.EqualTo(SimpleEnum.A));
            Assert.That(Deserialize<NamingConventionEnum>("hoge_fuga", options), Is.EqualTo(NamingConventionEnum.HogeFuga));
            Assert.That(Deserialize<DataMemberLabeledEnum>("c-alias", options), Is.EqualTo(DataMemberLabeledEnum.C));
        }
    }
}
