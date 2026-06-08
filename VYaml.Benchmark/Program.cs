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
    static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "manual")
        {
            ManualParse(args.Length > 1 ? args[1] : "sample_envoy.yaml");
            return 0;
        }

        if (args.Length > 0 && args[0] == "compare")
        {
            Compare();
            return 0;
        }

        var switcher = new BenchmarkSwitcher([
            typeof(DeserializationBenchmark),
            typeof(SerializationBenchmark),
            typeof(SimpleParsingBenchmark),
            typeof(DynamicDeserializationBenchmark),
            typeof(JsonDeserializationBenchmark)
        ]);
        switcher.Run();
        return 0;
    }

    static void ManualParse(string fileName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Examples", fileName);
        var yamlBytes = File.ReadAllBytes(path);

        const int warmup = 20_000;
        const int iterations = 200_000;

        long Tokens()
        {
            var parser = VYaml.Parser.YamlParser.FromBytes(yamlBytes);
            long n = 0;
            while (parser.Read()) n++;
            return n;
        }

        long tokenCount = 0;
        for (var i = 0; i < warmup; i++) tokenCount = Tokens();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++) tokenCount = Tokens();
        sw.Stop();

        var nsPerOp = sw.Elapsed.TotalMilliseconds * 1_000_000.0 / iterations;
        var mbPerSec = (yamlBytes.Length / 1_048_576.0) / (nsPerOp / 1_000_000_000.0);
        Console.WriteLine($"bytes={yamlBytes.Length} tokens={tokenCount} " +
                          $"iters={iterations} {nsPerOp:F0} ns/op  {mbPerSec:F1} MB/s  " +
                          $"total={sw.Elapsed.TotalSeconds:F2}s");

    }

    static void Compare()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Examples", "sample_envoy.yaml");
        var yamlBytes = File.ReadAllBytes(path);
        var yamlString = System.Text.Encoding.UTF8.GetString(yamlBytes);

        var ydnDeserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        var ydnSerializer = new YamlDotNet.Serialization.SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance)
            .Build();

        var sample = YamlSerializer.Deserialize<SampleEnvoy>(yamlBytes);

        Console.WriteLine($"bytes={yamlBytes.Length}");
        Measure("VYaml_Deserialize", () => YamlSerializer.Deserialize<SampleEnvoy>(yamlBytes));
        Measure("YamlDotNet_Deserialize", () => ydnDeserializer.Deserialize<SampleEnvoy>(yamlString));
        Measure("VYaml_Serialize", () => YamlSerializer.Serialize(sample));
        Measure("YamlDotNet_Serialize", () => ydnSerializer.Serialize(sample));
    }

    static void Measure(string name, Action action)
    {
        for (var i = 0; i < 2_000; i++) action();

        const int iterations = 50_000;
        var best = double.MaxValue;
        for (var rep = 0; rep < 5; rep++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++) action();
            sw.Stop();
            var us = sw.Elapsed.TotalMilliseconds * 1000.0 / iterations;
            if (us < best) best = us;
        }

        const int allocIters = 2_000;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < allocIters; i++) action();
        var after = GC.GetAllocatedBytesForCurrentThread();
        var bytesPerOp = (after - before) / (double)allocIters;

        Console.WriteLine($"{name,-26} {best,8:F2} us  {bytesPerOp,10:F0} B/op");
    }
}
