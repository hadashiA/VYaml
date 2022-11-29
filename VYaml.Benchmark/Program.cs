// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;

namespace VYaml.Benchmark;

static class Program
{
    static int Main()
    {
        BenchmarkRunner.Run<SimpleParsingBenchmark>();
        return 0;
    }
}
