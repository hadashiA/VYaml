using System.Collections.Generic;
using NUnit.Framework;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization;

[TestFixture]
public class UnionDiscriminatorTest : FormatterTestBase
{
    [Test]
    public void Serialize()
    {
        Assert.That(Serialize<IDiscriminatorShape>(new DiscriminatorCircle { Radius = 1.5 }),
            Is.EqualTo("type: circle\nradius: 1.5\n"));
        Assert.That(Serialize<DiscriminatorBase>(new DiscriminatorEmpty()),
            Is.EqualTo("kind: empty\n"));
        Assert.That(Serialize<DiscriminatorBase>(new DiscriminatorNamed("a", 2)),
            Is.EqualTo("kind: named\nname: a\ncount: 2\n"));
    }

    [Test]
    public void Deserialize()
    {
        var circle = Deserialize<IDiscriminatorShape>("type: circle\nradius: 1.5\n");
        Assert.That(circle, Is.InstanceOf<DiscriminatorCircle>());
        Assert.That(((DiscriminatorCircle)circle).Radius, Is.EqualTo(1.5));

        var rect = Deserialize<IDiscriminatorShape>("{ type: rect, width: 2, height: 3 }");
        Assert.That(rect, Is.InstanceOf<DiscriminatorRect>());
        Assert.That(((DiscriminatorRect)rect).Height, Is.EqualTo(3));

        var empty = Deserialize<DiscriminatorBase>("kind: empty\nunknown: [1, 2]\nfoo: { bar: 1 }\n");
        Assert.That(empty, Is.InstanceOf<DiscriminatorEmpty>());

        var named = (DiscriminatorNamed)Deserialize<DiscriminatorBase>("kind: named\nname: x\n");
        Assert.That(named.Name, Is.EqualTo("x"));
        Assert.That(named.Count, Is.EqualTo(1));

        Assert.That(YamlSerializer.Deserialize<IDiscriminatorShape?>(System.Text.Encoding.UTF8.GetBytes("~")), Is.Null);
    }

    [Test]
    public void RoundTrip_Nested()
    {
        var value = new DiscriminatorContainer
        {
            Shapes = new List<IDiscriminatorShape>
            {
                new DiscriminatorCircle { Radius = 1 },
                new DiscriminatorRect { Width = 2, Height = 3 },
            },
            Base = new DiscriminatorNamed("n", 5),
        };
        var yaml = Serialize(value);
        var result = Deserialize<DiscriminatorContainer>(yaml);

        Assert.That(result.Shapes, Has.Count.EqualTo(2));
        Assert.That(((DiscriminatorCircle)result.Shapes[0]).Radius, Is.EqualTo(1));
        Assert.That(((DiscriminatorRect)result.Shapes[1]).Width, Is.EqualTo(2));
        var named = (DiscriminatorNamed)result.Base!;
        Assert.That(named.Name, Is.EqualTo("n"));
        Assert.That(named.Count, Is.EqualTo(5));
    }

    [Test]
    public void Deserialize_DiscriminatorNotFirst()
    {
        var circle = Deserialize<IDiscriminatorShape>("radius: 1.5\nnested: { type: rect, x: [1, 2] }\ntype: circle\n");
        Assert.That(((DiscriminatorCircle)circle).Radius, Is.EqualTo(1.5));

        var named = (DiscriminatorNamed)Deserialize<DiscriminatorBase>("{ name: x, count: 3, kind: named }");
        Assert.That(named.Name, Is.EqualTo("x"));
        Assert.That(named.Count, Is.EqualTo(3));
    }

    [Test]
    public void Deserialize_DiscriminatorNotFound()
    {
        var ex = Assert.Throws<YamlSerializerException>(() => Deserialize<IDiscriminatorShape>("radius: 1.5\n"));
        Assert.That(ex!.Message, Does.Contain("discriminator 'type'"));
    }

    [Test]
    public void Deserialize_UnknownDiscriminator()
    {
        var ex = Assert.Throws<YamlSerializerException>(() => Deserialize<IDiscriminatorShape>("type: triangle\n"));
        Assert.That(ex!.Message, Does.Contain("triangle"));
    }
}