using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using VYaml.Benchmark.Examples;
using VYaml.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VYaml.Benchmark;

[MemoryDiagnoser]
public class JsonDeserializationBenchmark
{
    byte[]? jsonBytes;
    string? jsonString;

    YamlDotNet.Serialization.IDeserializer yamlDotNetDeserializer;

    readonly System.Text.Json.JsonSerializerOptions systemTextJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [GlobalSetup]
    public void Setup()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Examples", "sample_envoy.json");
        jsonBytes = File.ReadAllBytes(path);
        jsonString = Encoding.UTF8.GetString(jsonBytes);

        yamlDotNetDeserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    [Benchmark]
    public void YamlDotNet_Deserialize()
    {
        yamlDotNetDeserializer.Deserialize<SampleEnvoy>(jsonString!);
    }

    [Benchmark]
    public void SystemTextJson_Deserialize()
    {
        JsonSerializer.Deserialize<SampleEnvoy>(jsonBytes, systemTextJsonOptions);
    }

    [Benchmark]
    public void VYaml_Deserialize()
    {
        YamlSerializer.Deserialize<SampleEnvoy>(jsonBytes);
    }
}
