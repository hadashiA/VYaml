using System.Collections.Generic;
using NUnit.Framework;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class UnionWrappedTest : FormatterTestBase
    {
        [Test]
        public void Serialize()
        {
            Assert.That(Serialize<ISkill>(new ViolenceSkill { Name = "Punch", Power = 1 }),
                Is.EqualTo("violence:\n  name: Punch\n  power: 1\n"));
            Assert.That(Serialize<ISkill>(new MeditateSkill()), Is.EqualTo("meditate: {}\n"));
            Assert.That(Serialize<ISkill?>(null), Is.EqualTo("null"));

            var character = new Character
            {
                Skills = new List<ISkill> { new HugSkill { Name = "Hug" }, new MeditateSkill() },
            };
            Assert.That(Serialize(character), Is.EqualTo(
                "skills:\n" +
                "- hug:\n" +
                "    name: Hug\n" +
                "- meditate: {}\n"));
        }

        [Test]
        public void Deserialize()
        {
            var character = Deserialize<Character>(
                "skills:\n" +
                "  - violence: { name: 暴力, power: 1 }\n" +
                "  - hug:\n" +
                "      name: 抱きしめ\n" +
                "  - meditate: {}\n" +
                "  - meditate:\n" +
                "  - meditate: ~\n");

            Assert.That(character.Skills, Has.Count.EqualTo(5));
            var violence = (ViolenceSkill)character.Skills[0];
            Assert.That(violence.Name, Is.EqualTo("暴力"));
            Assert.That(violence.Power, Is.EqualTo(1));
            Assert.That(((HugSkill)character.Skills[1]).Name, Is.EqualTo("抱きしめ"));
            // an empty value is an empty mapping
            Assert.That(character.Skills[2], Is.InstanceOf<MeditateSkill>());
            Assert.That(character.Skills[3], Is.InstanceOf<MeditateSkill>());
            Assert.That(character.Skills[4], Is.InstanceOf<MeditateSkill>());

            Assert.That(YamlSerializer.Deserialize<ISkill?>(System.Text.Encoding.UTF8.GetBytes("~")), Is.Null);
        }

        [Test]
        public void Deserialize_EmptyValue_WithMembers()
        {
            // same as `hug: {}`: members not in the YAML take default(T); required members are still required
            var hug = (HugSkill)Deserialize<ISkill>("hug:\n");
            Assert.That(hug.Name, Is.EqualTo(((HugSkill)Deserialize<ISkill>("hug: {}")).Name));

            var ex = Assert.Throws<YamlSerializerException>(() => Deserialize<ISkill>("shout:\n"));
            Assert.That(ex!.Message, Does.Contain("'words'"));

            Assert.That(((ShoutSkill)Deserialize<ISkill>("shout: { words: Hey }")).Words, Is.EqualTo("Hey"));
        }

        [Test]
        public void Deserialize_Errors()
        {
            var unknown = Assert.Throws<YamlSerializerException>(() => Deserialize<ISkill>("kick: {}\n"));
            Assert.That(unknown!.Message, Does.Contain("'kick'").And.Contain("'violence'").And.Contain("'meditate'"));

            var several = Assert.Throws<YamlSerializerException>(() => Deserialize<ISkill>("hug: {}\nmeditate: {}\n"));
            Assert.That(several!.Message, Does.Contain("exactly one key"));

            Assert.Throws<VYaml.Parser.YamlParserException>(() => Deserialize<ISkill>("[hug]\n")); // not a mapping
            Assert.Throws<YamlSerializerException>(() => Deserialize<ISkill>("? [hug]\n: {}\n")); // not a scalar key
            Assert.Throws<YamlSerializerException>(() => Deserialize<ISkill>("{}\n"));
        }

        [Test]
        public void OverrideMemberAttributes()
        {
            var hug = (OverrideKeyHug)Deserialize<OverrideKeyBase>("hug:\n  power: 2\n  hug: tight\n  note: n\n");
            Assert.That(hug.Name, Is.EqualTo("tight"));
            Assert.That(hug.Note, Is.EqualTo("n"));
            Assert.That(hug.Power, Is.EqualTo(2));
            // the override stays at the position of the base declaration
            Assert.That(Serialize<OverrideKeyBase>(hug), Is.EqualTo("hug:\n  hug: tight\n  note: n\n  power: 2\n"));

            var wave = (OverrideKeyWave)Deserialize<OverrideKeyBase>("wave: { wave: hello, note: n }");
            Assert.That(wave.Name, Is.EqualTo("hello"));
            Assert.That(wave.Note, Is.Null); // [YamlIgnore] on the override
            Assert.That(Serialize<OverrideKeyBase>(wave), Is.EqualTo("wave:\n  wave: hello\n  power: 0\n"));
        }

        [Test]
        public void RoundTrip()
        {
            var value = new Character
            {
                Skills = new List<ISkill>
                {
                    new ViolenceSkill { Name = "Punch", Power = 3 },
                    new HugSkill { Name = "Hug" },
                    new MeditateSkill(),
                    new ShoutSkill { Words = "Hey" },
                },
            };
            var yaml = Serialize(value);
            var restored = Deserialize<Character>(yaml);

            Assert.That(restored.Skills, Has.Count.EqualTo(4));
            Assert.That(((ViolenceSkill)restored.Skills[0]).Power, Is.EqualTo(3));
            Assert.That(((HugSkill)restored.Skills[1]).Name, Is.EqualTo("Hug"));
            Assert.That(restored.Skills[2], Is.InstanceOf<MeditateSkill>());
            Assert.That(((ShoutSkill)restored.Skills[3]).Words, Is.EqualTo("Hey"));
        }
    }
}
