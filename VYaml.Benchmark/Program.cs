// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            typeof(JsonDeserializationBenchmark),
        });
        switcher.Run();

        // var path = Path.Combine(Directory.GetCurrentDirectory(), "Examples", "sample_envoy.yaml");
        // var yamlBytes = File.ReadAllBytes(path);
        // for (var i = 0; i < 100; i++)
        // {
        //     var result = YamlSerializer.Deserialize<SampleEnvoy>(yamlBytes);
        // }

        // var path = Path.Combine(Directory.GetCurrentDirectory(), "Examples", "sample_envoy.json");
        // var bytes = File.ReadAllBytes(path);
        // var str = File.ReadAllText(path);
        // var json = JsonSerializer.Deserialize<SampleEnvoy>(bytes, new JsonSerializerOptions
        // {
        //     PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        // });

        // var json = Newtonsoft.Json.JsonConvert.DeserializeObject<SampleEnvoy>(str, new JsonSerializerSettings
        // {
        //     ContractResolver = new CamelCasePropertyNamesContractResolver()
        // });

        return 0;
    }
}
