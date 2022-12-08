using System.Text;
using BenchmarkDotNet.Attributes;
using VYaml.Benchmark.Examples;
using VYaml.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VYaml.Benchmark;

[MemoryDiagnoser]
public class DeserializationBenchmark
{
    byte[]? yamlBytes;
    string? yamlString;

    YamlDotNet.Serialization.IDeserializer yamlDotNetDeserializer;

    [GlobalSetup]
    public void Setup()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Examples", "sample_envoy.yaml");
        yamlBytes = File.ReadAllBytes(path);
        yamlString = Encoding.UTF8.GetString(yamlBytes);
        yamlDotNetDeserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }


    [Benchmark]
    public void YamlDotNet_Deserialize()
    {
        yamlDotNetDeserializer.Deserialize<SampleEnvoy>(yamlString!);
    }

    [Benchmark]
    public void VYaml_Deserialize()
    {
        YamlSerializer.Deserialize<SampleEnvoy>(yamlBytes);
    }
}