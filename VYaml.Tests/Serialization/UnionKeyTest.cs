using System.Collections.Generic;
using NUnit.Framework;
using VYaml.Annotations;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class UnionKeyTest : FormatterTestBase
    {
        [Test]
        public void Deserialize()
        {
            var job = Deserialize<Job>(
                "steps:\n" +
                "  - name: Checkout\n" +
                "    uses: actions/checkout@v4\n" +
                "    with:\n" +
                "      fetch-depth: '0'\n" +
                "  - run: dotnet test\n" +
                "  - name: Publish\n" +
                "    shell: bash\n" +
                "    run: |\n" +
                "      dotnet publish\n" +
                "  - { uses: actions/upload-artifact@v4 }\n");

            Assert.That(job.Steps, Has.Count.EqualTo(4));

            var checkout = (ActionStep)job.Steps[0];
            Assert.That(checkout.Name, Is.EqualTo("Checkout"));
            Assert.That(checkout.Uses, Is.EqualTo("actions/checkout@v4"));
            Assert.That(checkout.With!["fetch-depth"], Is.EqualTo("0"));

            Assert.That(((RunStep)job.Steps[1]).Run, Is.EqualTo("dotnet test"));

            var publish = (RunStep)job.Steps[2];
            Assert.That(publish.Name, Is.EqualTo("Publish"));
            Assert.That(publish.Shell, Is.EqualTo("bash"));
            Assert.That(publish.Run, Is.EqualTo("dotnet publish\n"));

            Assert.That(((ActionStep)job.Steps[3]).Uses, Is.EqualTo("actions/upload-artifact@v4"));
        }

        [Test]
        public void Deserialize_FirstDeclaredWins()
        {
            // both keys: the subtype declared first wins, and the other key is ignored as an unknown member
            var step = Deserialize<IStep>("run: echo\nuses: actions/checkout@v4\n");
            Assert.That(step, Is.InstanceOf<ActionStep>());
            Assert.That(((ActionStep)step).Uses, Is.EqualTo("actions/checkout@v4"));
        }

        [Test]
        public void Deserialize_NotFound()
        {
            var ex = Assert.Throws<YamlSerializerException>(() => Deserialize<IStep>("name: x\n"));
            Assert.That(ex!.Message, Does.Contain("'uses'").And.Contain("'run'"));

            Assert.That(YamlSerializer.Deserialize<IStep?>(System.Text.Encoding.UTF8.GetBytes("~")), Is.Null);
        }

        [Test]
        public void Serialize()
        {
            var job = new Job
            {
                Steps = new List<IStep>
                {
                    new ActionStep { Uses = "actions/checkout@v4" },
                    new RunStep { Name = "Test", Run = "dotnet test" },
                }
            };
            var options = new YamlSerializerOptions { DefaultIgnoreCondition = YamlIgnoreCondition.WhenWritingNull };
            Assert.That(Serialize(job, options), Is.EqualTo(
                "steps:\n" +
                "- uses: actions/checkout@v4\n" +
                "- name: Test\n" +
                "  run: dotnet test\n"));

            var result = Deserialize<Job>(Serialize(job));
            Assert.That(((ActionStep)result.Steps[0]).Uses, Is.EqualTo("actions/checkout@v4"));
            Assert.That(((RunStep)result.Steps[1]).Run, Is.EqualTo("dotnet test"));
        }

        [Test]
        public void KeyFollowsMemberNamingRules()
        {
            Assert.That(Deserialize<UnionKeyBase>("max_count: 3\n"), Is.InstanceOf<UnionKeySnakeCase>());
            Assert.That(((UnionKeyAlias)Deserialize<UnionKeyBase>("alias: a\n")).Value, Is.EqualTo("a"));

            // keys following YamlSerializerOptions.NamingConvention
            var options = new YamlSerializerOptions { NamingConvention = NamingConvention.UpperCamelCase };
            var step = Deserialize<IStep>("Name: x\nRun: echo\n", options);
            Assert.That(((RunStep)step).Run, Is.EqualTo("echo"));
        }

        [Test]
        public void Lookahead_DoesNotBreakParser()
        {
            // anchors, aliases, nested collections and multiple documents around a lookahead
            var steps = Deserialize<List<IStep>>(
                "- &a\n" +
                "  name: n\n" +
                "  with: { k: v }\n" +
                "  unknown: [1, 2, { x: y, z: [ { run: r } ] }]\n" +
                "  uses: u\n" +
                "- *a\n" +
                "- name: last\n" +
                "  run: r\n");
            Assert.That(steps, Has.Count.EqualTo(3));
            Assert.That(((ActionStep)steps[0]).Uses, Is.EqualTo("u"));
            Assert.That(steps[1], Is.SameAs(steps[0]));
            Assert.That(((RunStep)steps[2]).Name, Is.EqualTo("last"));
        }
    
        [Test]
        public void OverrideMemberAttributes()
        {
            var hug = (OverrideKeyHug)Deserialize<OverrideKeyBase>("power: 2\nhug: tight\nnote: n\n");
            Assert.That(hug.Name, Is.EqualTo("tight"));
            Assert.That(hug.Note, Is.EqualTo("n"));
            Assert.That(hug.Power, Is.EqualTo(2));
            // the override stays at the position of the base declaration
            Assert.That(Serialize<OverrideKeyBase>(hug), Is.EqualTo("hug: tight\nnote: n\npower: 2\n"));

            var wave = (OverrideKeyWave)Deserialize<OverrideKeyBase>("wave: hello\nnote: n\n");
            Assert.That(wave.Name, Is.EqualTo("hello"));
            Assert.That(wave.Note, Is.Null); // [YamlIgnore] on the override
            Assert.That(Serialize<OverrideKeyBase>(wave), Is.EqualTo("wave: hello\npower: 0\n"));
        }
    }
}
