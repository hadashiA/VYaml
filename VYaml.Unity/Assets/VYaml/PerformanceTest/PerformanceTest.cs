using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Unity.PerformanceTesting;
using YamlDotNet.Serialization.NamingConventions;

namespace VYaml.PerformanceTest
{
    public class PerformanceTest
    {
        const int N = 100;

        const string YAML =
            "admin:\r\n  address:\r\n    socket_address: { address: 127.0.0.1, port_value: 9901 }\r\n\r\nstatic_resources:\r\n  listeners:\r\n    - name: listener_0\r\n      address:\r\n        socket_address: { address: 127.0.0.1, port_value: 10000 }\r\n      filter_chains:\r\n        - filters:\r\n            - name: envoy.filters.network.http_connection_manager\r\n              typed_config:\r\n                \"@type\": type.googleapis.com/envoy.extensions.filters.network.http_connection_manager.v3.HttpConnectionManager\r\n                stat_prefix: ingress_http\r\n                codec_type: AUTO\r\n                route_config:\r\n                  name: local_route\r\n                  virtual_hosts:\r\n                    - name: local_service\r\n                      domains: [\"*\"]\r\n                      routes:\r\n                        - match: { prefix: \"/\" }\r\n                          route: { cluster: some_service }\r\n                http_filters:\r\n                  - name: envoy.filters.http.router\r\n                    typed_config:\r\n                      \"@type\": type.googleapis.com/envoy.extensions.filters.http.router.v3.Router\r\n  clusters:\r\n    - name: some_service\r\n      connect_timeout: 0.25s\r\n      lb_policy: ROUND_ROBIN\r\n      type: EDS\r\n      eds_cluster_config:\r\n        eds_config:\r\n          resource_api_version: V3\r\n          api_config_source:\r\n            api_type: GRPC\r\n            transport_api_version: V3\r\n            grpc_services:\r\n              - envoy_grpc:\r\n                  cluster_name: xds_cluster\r\n    - name: xds_cluster\r\n      connect_timeout: 0.25s\r\n      type: STATIC\r\n      lb_policy: ROUND_ROBIN\r\n      typed_extension_protocol_options:\r\n        envoy.extensions.upstreams.http.v3.HttpProtocolOptions:\r\n          \"@type\": type.googleapis.com/envoy.extensions.upstreams.http.v3.HttpProtocolOptions\r\n          explicit_http_config:\r\n            http2_protocol_options:\r\n              connection_keepalive:\r\n                interval: 30s\r\n                timeout: 5s\r\n      upstream_connection_options:\r\n        # configure a TCP keep-alive to detect and reconnect to the admin\r\n        # server in the event of a TCP socket half open connection\r\n        tcp_keepalive: {}\r\n      load_assignment:\r\n        cluster_name: xds_cluster\r\n        endpoints:\r\n          - lb_endpoints:\r\n              - endpoint:\r\n                  address:\r\n                    socket_address:\r\n                      address: 127.0.0.1\r\n                      port_value: 5678";

        [Test]
        [Performance]
        public void SimpleParsing()
        {
            var yamlBytes = Encoding.UTF8.GetBytes(YAML);

            Measure.Method(() =>
                {
                    var parser = VYaml.Parser.YamlParser.FromBytes(yamlBytes);
                    while (parser.Read())
                    {
                    }
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(N)
                .MeasurementCount(20)
                .GC()
                .SampleGroup("VYaml.Parse")
                .Run();

            Measure.Method(() =>
                {
                    using var reader = new StringReader(YAML);
                    var parser = new YamlDotNet.Core.Parser(reader);
                    while (parser.MoveNext())
                    {
                    }
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(N)
                .MeasurementCount(20)
                .GC()
                .SampleGroup("YamlDotNet.Parse")
                .Run();
        }

        [Test]
        [Performance]
        public void DynamicDeserialize()
        {
            var yamlBytes = Encoding.UTF8.GetBytes(YAML);
            var yamldotNetDeserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            Measure.Method(() =>
                {
                    VYaml.Serialization.YamlSerializer.Deserialize<dynamic>(yamlBytes);
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(N)
                .MeasurementCount(20)
                .GC()
                .SampleGroup("VYaml.Deserialize<dynamic>")
                .Run();

            Measure.Method(() =>
                {
                    yamldotNetDeserializer.Deserialize<dynamic>(YAML);
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(N)
                .MeasurementCount(20)
                .GC()
                .SampleGroup("YamlDotNet.Deserialize<dynamic>")
                .Run();
        }

        [Test]
        [Performance]
        public void Deserialize()
        {
            var yamlBytes = Encoding.UTF8.GetBytes(YAML);
            var yamldotNetDeserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            Measure.Method(() =>
                {
                    VYaml.Serialization.YamlSerializer.Deserialize<SampleEnvoy>(yamlBytes);
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(N)
                .MeasurementCount(20)
                .GC()
                .SampleGroup("VYaml.Deserialize<T>")
                .Run();

            Measure.Method(() =>
                {
                    yamldotNetDeserializer.Deserialize<SampleEnvoy>(YAML);
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(N)
                .MeasurementCount(20)
                .GC()
                .SampleGroup("YamlDotNet.Deserialize<T>")
                .Run();
        }

        [Test]
        [Performance]
        public void Serialize()
        {
            var sampleData = new SampleEnvoy
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

            var yamldotNetSerializer = new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            Measure.Method(() =>
                {
                    VYaml.Serialization.YamlSerializer.Serialize(sampleData);
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(N)
                .MeasurementCount(20)
                .GC()
                .SampleGroup("VYaml.Serialize<T>")
                .Run();

            Measure.Method(() =>
                {
                    yamldotNetSerializer.Serialize(sampleData);
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(N)
                .MeasurementCount(20)
                .GC()
                .SampleGroup("YamlDotNet.Serialize<T>")
                .Run();

        }
    }
}
