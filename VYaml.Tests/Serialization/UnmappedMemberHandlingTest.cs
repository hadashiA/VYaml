using NUnit.Framework;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class UnmappedMemberHandlingTest : FormatterTestBase
    {
        static readonly YamlSerializerOptions Disallow = new() { UnmappedMemberHandling = YamlUnmappedMemberHandling.Disallow };

        [Test]
        public void Skip_ByDefault()
        {
            var result = Deserialize<SchemaEndpoint>("path: /a\nunknown: 1\n");
            Assert.That(result.Path, Is.EqualTo("/a"));
        }

        [Test]
        public void Disallow_Throws()
        {
            var ex = Assert.Throws<YamlSerializerException>(() => Deserialize<SchemaEndpoint>("path: /a\nunknown: { x: 1 }\n", Disallow));
            Assert.That(ex!.Message, Does.Contain("'unknown'").And.Contain("SchemaEndpoint"));

            Assert.That(Deserialize<SchemaEndpoint>("path: /a\n", Disallow).Path, Is.EqualTo("/a"));
        }

        [Test]
        public void Disallow_TypeWithoutMembers()
        {
            Assert.Throws<YamlSerializerException>(() => Deserialize<DiscriminatorContainer>("base: { kind: empty, unknown: 1 }\n", Disallow));
            Assert.That(Deserialize<DiscriminatorContainer>("base: { kind: empty }\n", Disallow).Base, Is.InstanceOf<DiscriminatorEmpty>());
        }

        [Test]
        public void Skip_ComplexKey()
        {
            // a type without members skips collection keys too
            var yaml = "? [a, b]\n: v\nkind: empty\n";
            Assert.That(Deserialize<DiscriminatorBase>(yaml), Is.InstanceOf<DiscriminatorEmpty>());
            Assert.Throws<YamlSerializerException>(() => Deserialize<DiscriminatorBase>(yaml, Disallow));
        }

        [Test]
        public void Disallow_DiscriminatorKeyIsNotUnmapped()
        {
            // the discriminator key is not the first key, so the subtype reads it as a key of its mapping
            var result = Deserialize<IDiscriminatorShape>("radius: 2\ntype: circle\n", Disallow);
            Assert.That(((DiscriminatorCircle)result).Radius, Is.EqualTo(2));

            Assert.Throws<YamlSerializerException>(() => Deserialize<IDiscriminatorShape>("radius: 2\ntype: circle\nwidth: 1\n", Disallow));
        }

        [Test]
        public void Disallow_Wrapped()
        {
            var slack = Deserialize<INotification>("slack: { channel: a }", Disallow);
            Assert.That(slack, Is.InstanceOf<SlackNotification>());

            var ex = Assert.Throws<YamlSerializerException>(() => Deserialize<INotification>("slack: { channel: a, to: b }", Disallow));
            Assert.That(ex!.Message, Does.Contain("'to'"));
        }
    }
}
