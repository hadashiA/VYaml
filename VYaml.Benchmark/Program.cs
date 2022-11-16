// See https://aka.ms/new-console-template for more information

using System.Buffers;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using VYaml;
using VYaml.Formatters;

[MemoryDiagnoser]
public class ParserBenchmark
{
    byte[] yamlBytes;
    string yamlString;

    public ParserBenchmark()
    {
        var data = new[]
        {
            "- item 1",
            "- item 2",
            "-",
            "  - item 3.1",
            "  - item 3.2",
            "-",
            "  key 1: value 1",
            "  key 2: value 2",
            "  key 3: { a: [{x: 100, y: 200}, {x: 300, y: 400}] }"
        };
        yamlString = string.Join('\n', data);
        yamlBytes = Encoding.UTF8.GetBytes(yamlString);
    }

    [Benchmark]
    public void YamlDotNet_Scanner()
    {
        // var scanner = new Scanner(new StreamReader(new MemoryStream(yamlBytes)));
        // while (scanner.MoveNext())
        // {
        // }
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
        var result = deserializer.Deserialize<object>(yamlString);

    }

    [Benchmark]
    public void VYaml_Utf8YamlReader()
    {
        var reader = new Utf8Tokenizer(new ReadOnlySequence<byte>(yamlBytes));
        reader.SkipAfter(TokenType.DocumentStart);
        var result = new PrimitiveObjectFormatter().Deserialize(ref reader);
    }
}

static class Program
{
    static int Main()
    {
        BenchmarkRunner.Run<ParserBenchmark>();
        return 0;
    }
}
