// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;

namespace VYaml.Benchmark;

static class Program
{
    static int Main()
    {
        var switcher = new BenchmarkSwitcher(new[]
        {
            typeof(SimpleParsingBenchmark),
            typeof(DynamicDeserializationBenchmark),
        });
        switcher.Run();
        return 0;
    }
}
