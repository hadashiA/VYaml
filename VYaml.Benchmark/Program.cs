// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using VYaml.Benchmark.Examples;
using VYaml.Serialization;

namespace VYaml.Benchmark;

static class Program
{
    static int Main()
    {
        var switcher = new BenchmarkSwitcher(new[]
        {
            typeof(SimpleParsingBenchmark),
            typeof(DynamicDeserializationBenchmark),
            typeof(DeserializationBenchmark),
        });
        switcher.Run();

        // var path = Path.Combine(Directory.GetCurrentDirectory(), "Examples", "sample_envoy.yaml");
        // var yamlBytes = File.ReadAllBytes(path);
        // for (var i = 0; i < 100; i++)
        // {
        //     var result = YamlSerializer.Deserialize<SampleEnvoy>(yamlBytes);
        // }

        return 0;
    }
}
