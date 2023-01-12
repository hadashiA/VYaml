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
            typeof(DeserializationBenchmark),
            typeof(SerializationBenchmark),
            typeof(SimpleParsingBenchmark),
            typeof(DynamicDeserializationBenchmark),
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
        //
        // var sampleData = new SampleEnvoy
        // {
        //     Admin = new Admin
        //     {
        //         Address = new Address
        //         {
        //             SocketAddress = new SocketAddress
        //             {
        //                 PortValue = 9901,
        //                 Address = "127.0.0.1"
        //             }
        //         },
        //     },
        //     StaticResources = new StaticResources
        //     {
        //         Listeners = new List<Listener>
        //         {
        //             new()
        //             {
        //                 Name = "listener_0",
        //                 Address = new Address
        //                 {
        //                     SocketAddress = new SocketAddress
        //                     {
        //                         Address = "127.0.0.1",
        //                         PortValue = 10000
        //                     }
        //                 },
        //                 FilterChains = new List<FilterChain>
        //                 {
        //                     new()
        //                     {
        //                         Filters = new List<Filter>
        //                         {
        //                             new()
        //                             {
        //                                 TypedConfig = new TypedConfig
        //                                 {
        //                                     Type =
        //                                         "type.googleapis.com/envoy.extensions.filters.network.http_connection_manager.v3.HttpConnectionManager",
        //                                     StatPrefix = "ingress_http",
        //                                     CodecType = "AUTO",
        //                                     RouteConfig = new RouteConfig
        //                                     {
        //                                         Name = "local_service",
        //                                         VirtualHosts = new List<VirtualHost>
        //                                         {
        //                                             new()
        //                                             {
        //                                                 Name = "local_service",
        //                                                 Domains = new List<string> { "*" },
        //                                                 Routes = new List<Routes>
        //                                                 {
        //                                                     new()
        //                                                     {
        //                                                         Match = new Dictionary<string, string>
        //                                                         {
        //                                                             { "Prefix", "/" }
        //                                                         },
        //                                                         Route = new Dictionary<string, string>
        //                                                         {
        //                                                             { "cluster", "some_service" }
        //                                                         }
        //                                                     }
        //                                                 }
        //                                             }
        //                                         },
        //                                     }
        //                                 }
        //                             }
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // };
        // var o = YamlSerializer.SerializeToString(sampleData);
        return 0;
    }
}
