using NUnit.Framework;
using VYaml.Annotations;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class RequiredMemberTest : FormatterTestBase
    {
        [Test]
        public void Deserialize_AllPresent()
        {
            var result = Deserialize<WithRequiredModifier>("name: a\ncount: 3\n");
            Assert.That(result.Name, Is.EqualTo("a"));
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Comment, Is.Null);

            var attribute = Deserialize<WithRequiredAttribute>("max_count: 2\nname: b\n");
            Assert.That(attribute.MaxCount, Is.EqualTo(2));
            Assert.That(attribute.Name, Is.EqualTo("b"));
        }

        [Test]
        public void Deserialize_Missing()
        {
            var ex = Assert.Throws<YamlSerializerException>(() => Deserialize<WithRequiredModifier>("comment: x\n"));
            Assert.That(ex!.Message, Does.Contain("'name'").And.Contain("'count'").And.Contain("WithRequiredModifier"));

            ex = Assert.Throws<YamlSerializerException>(() => Deserialize<WithRequiredAttribute>("name: b\noptional: 1\n"));
            Assert.That(ex!.Message, Does.Contain("'max_count'").And.Not.Contain("'name'"));

            ex = Assert.Throws<YamlSerializerException>(() => Deserialize<RequiredRecord>("value: 1\n"));
            Assert.That(ex!.Message, Does.Contain("'id'"));

            ex = Assert.Throws<YamlSerializerException>(() => Deserialize<RequiredStruct>("{}"));
            Assert.That(ex!.Message, Does.Contain("'x'"));
        }

        [Test]
        public void Deserialize_NullValueSatisfiesRequired()
        {
            var result = Deserialize<WithRequiredAttribute>("name: ~\nmax_count: 1\n");
            Assert.That(result.Name, Is.Null);
        }

        [Test]
        public void Deserialize_NamingConventionOptions()
        {
            var options = new YamlSerializerOptions { NamingConvention = NamingConvention.UpperCamelCase };
            var result = Deserialize<WithRequiredModifier>("Name: a\nCount: 1\n", options);
            Assert.That(result.Name, Is.EqualTo("a"));
        }

        [Test]
        public void Deserialize_UnionSubType()
        {
            var step = (RequiredActionStep)Deserialize<IRequiredStep>("name: n\nuses: u\n");
            Assert.That(step.Uses, Is.EqualTo("u"));

            var ex = Assert.Throws<YamlSerializerException>(() => Deserialize<IRequiredStep>("uses: u\n"));
            Assert.That(ex!.Message, Does.Contain("'name'"));
        }

        [Test]
        public void Serialize_RequiredIsAlwaysWritten()
        {
            var options = new YamlSerializerOptions { DefaultIgnoreCondition = YamlIgnoreCondition.WhenWritingDefault };
            var yaml = Serialize(new WithRequiredAttribute { Name = null, MaxCount = 0, Optional = 0 }, options);
            Assert.That(yaml, Is.EqualTo("name: null\nmax_count: 0\n"));

            var roundTrip = Deserialize<WithRequiredAttribute>(yaml, options);
            Assert.That(roundTrip.Name, Is.Null);
        }
    }
}
