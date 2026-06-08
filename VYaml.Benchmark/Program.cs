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
}
