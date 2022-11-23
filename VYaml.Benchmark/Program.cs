// See https://aka.ms/new-console-template for more information

using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace VYaml.Benchmark;

[MemoryDiagnoser]
public class SimpleParsingBenchmark
{
    const int N = 100;
    byte[] yamlBytes;
    string yamlString;

    [GlobalSetup]
    public void Setup()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Examples", "sample_envoy.yaml");
        yamlBytes = File.ReadAllBytes(path);
        yamlString = Encoding.UTF8.GetString(yamlBytes);
    }

    [Benchmark]
    public void YamlDotNet_Parser()
    {
        // for (var i = 0; i < N; i++)
        {
            using var reader = new StringReader(yamlString);
            var parser = new YamlDotNet.Core.Parser(reader);
            while (parser.MoveNext())
            {
            }
        }
    }

    [Benchmark]
    public void VYaml_Parser()
    {
        // for (var i = 0; i < N; i++)
        {
            var parser = Parser.FromBytes(yamlBytes);
            while (parser.Read())
            {
            }
        }
    }
}

static class Program
{
    static int Main()
    {
        BenchmarkRunner.Run<SimpleParsingBenchmark>();
        return 0;
    }
}
