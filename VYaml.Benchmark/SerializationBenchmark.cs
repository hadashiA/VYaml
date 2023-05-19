using System.Text;
using BenchmarkDotNet.Attributes;
using VYaml.Benchmark.Examples;
using VYaml.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[MemoryDiagnoser]
public class SerializationBenchmark
{
    const int N = 100;

    byte[]? yamlBytes;
    string? yamlString;

    YamlDotNet.Serialization.ISerializer yamlDotNetSerializer = default!;
    SampleEnvoy sampleData = default!;

    [GlobalSetup]
    public void Setup()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Examples", "sample_envoy.yaml");
        yamlBytes = File.ReadAllBytes(path);
        yamlString = Encoding.UTF8.GetString(yamlBytes);
        yamlDotNetSerializer = new YamlDotNet.Serialization.SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        sampleData = new SampleEnvoy
        {
            Admin = new Admin
            {
                Address = new Address
                {
                    SocketAddress = new SocketAddress
                    {
                        PortValue = 9901,
                        Address = "127.0.0.1"
                    }
                },
            },
            StaticResources = new StaticResources
            {
                Listeners = new List<Listener>
                {
                    new()
                    {
                        Name = "listener_0",
                        Address = new Address
                        {
                            SocketAddress = new SocketAddress
                            {
                                Address = "127.0.0.1",
                                PortValue = 10000
                            }
                        },
                        FilterChains = new List<FilterChain>
                        {
                            new()
                            {
                                Filters = new List<Filter>
                                {
                                    new()
                                    {
                                        TypedConfig = new TypedConfig
                                        {
                                            Type =
                                                "type.googleapis.com/envoy.extensions.filters.network.http_connection_manager.v3.HttpConnectionManager",
                                            StatPrefix = "ingress_http",
                                            CodecType = "AUTO",
                                            RouteConfig = new RouteConfig
                                            {
                                                Name = "local_service",
                                                VirtualHosts = new List<VirtualHost>
                                                {
                                                    new()
                                                    {
                                                        Name = "local_service",
                                                        Domains = new List<string> { "*" },
                                                        Routes = new List<Routes>
                                                        {
                                                            new()
                                                            {
                                                                Match = new Dictionary<string, string>
                                                                {
                                                                    { "Prefix", "/" }
                                                                },
                                                                Route = new Dictionary<string, string>
                                                                {
                                                                    { "cluster", "some_service" }
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }


    [Benchmark]
    public void YamlDotNet_Sserialize()
    {
        yamlDotNetSerializer.Serialize(sampleData);
    }

    [Benchmark]
    public void VYaml_Serialize()
    {
        YamlSerializer.Serialize(sampleData);
    }
}