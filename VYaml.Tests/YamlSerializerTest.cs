using System.Buffers;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using VYaml.Formatters;

namespace VYaml.Tests;

[TestFixture]
public class YamlSerializerTest
{
    // [Test]
    // public void Deserialize_DynamicList()
    // {
    //     var data = new[]
    //     {
    //         "- item 1",
    //         "- item 2",
    //         "-",
    //         "  - item 3.1",
    //         "  - item 3.2",
    //         "-",
    //         "  key 1: value 1",
    //         "  key 2: value 2",
    //         "  key 3: { a: [{x: 100, y: 200}, {x: 300, y: 400}] }"
    //     };
    //
    //     var yamlString = string.Join('\n', data);
    //     var yamlBytes = Encoding.UTF8.GetBytes(yamlString);
    //     var reader = new Utf8YamlReader(new ReadOnlySequence<byte>(yamlBytes));
    //     reader.SkipAfter(TokenType.StreamStart);
    //     var result = (dynamic)new PrimitiveObjectFormatter().Deserialize(ref reader);
    //
    //     Assert.That(result, Is.InstanceOf<List<object>>());
    //     Assert.That(result[0], Is.EqualTo("item 1"));
    //     Assert.That(result[1], Is.EqualTo("item 2"));
    //     Assert.That(result[2][0], Is.EqualTo("item 3.1"));
    //     Assert.That(result[2][1], Is.EqualTo("item 3.2"));
    //     Assert.That(result[3]["key 1"], Is.EqualTo("value 1"));
    //     Assert.That(result[3]["key 2"], Is.EqualTo("value 2"));
    //     Assert.That(result[3]["key 3"]["a"][0]["x"], Is.EqualTo(100));
    //     Assert.That(result[3]["key 3"]["a"][0]["y"], Is.EqualTo(200));
    //     Assert.That(result[3]["key 3"]["a"][1]["x"], Is.EqualTo(300));
    //     Assert.That(result[3]["key 3"]["a"][1]["y"], Is.EqualTo(400));
    // }
}
