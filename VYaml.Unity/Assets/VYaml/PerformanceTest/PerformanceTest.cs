using System.IO;
using System.Text;
using VYaml;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;

namespace VYaml.PerformanceTest
{
    public class PrebuildSetup : IPrebuildSetup
    {
        const string YAML =
            "admin:\r\n  address:\r\n    socket_address: { address: 127.0.0.1, port_value: 9901 }\r\n\r\nstatic_resources:\r\n  listeners:\r\n    - name: listener_0\r\n      address:\r\n        socket_address: { address: 127.0.0.1, port_value: 10000 }\r\n      filter_chains:\r\n        - filters:\r\n            - name: envoy.filters.network.http_connection_manager\r\n              typed_config:\r\n                \"@type\": type.googleapis.com/envoy.extensions.filters.network.http_connection_manager.v3.HttpConnectionManager\r\n                stat_prefix: ingress_http\r\n                codec_type: AUTO\r\n                route_config:\r\n                  name: local_route\r\n                  virtual_hosts:\r\n                    - name: local_service\r\n                      domains: [\"*\"]\r\n                      routes:\r\n                        - match: { prefix: \"/\" }\r\n                          route: { cluster: some_service }\r\n                http_filters:\r\n                  - name: envoy.filters.http.router\r\n                    typed_config:\r\n                      \"@type\": type.googleapis.com/envoy.extensions.filters.http.router.v3.Router\r\n  clusters:\r\n    - name: some_service\r\n      connect_timeout: 0.25s\r\n      lb_policy: ROUND_ROBIN\r\n      type: EDS\r\n      eds_cluster_config:\r\n        eds_config:\r\n          resource_api_version: V3\r\n          api_config_source:\r\n            api_type: GRPC\r\n            transport_api_version: V3\r\n            grpc_services:\r\n              - envoy_grpc:\r\n                  cluster_name: xds_cluster\r\n    - name: xds_cluster\r\n      connect_timeout: 0.25s\r\n      type: STATIC\r\n      lb_policy: ROUND_ROBIN\r\n      typed_extension_protocol_options:\r\n        envoy.extensions.upstreams.http.v3.HttpProtocolOptions:\r\n          \"@type\": type.googleapis.com/envoy.extensions.upstreams.http.v3.HttpProtocolOptions\r\n          explicit_http_config:\r\n            http2_protocol_options:\r\n              connection_keepalive:\r\n                interval: 30s\r\n                timeout: 5s\r\n      upstream_connection_options:\r\n        # configure a TCP keep-alive to detect and reconnect to the admin\r\n        # server in the event of a TCP socket half open connection\r\n        tcp_keepalive: {}\r\n      load_assignment:\r\n        cluster_name: xds_cluster\r\n        endpoints:\r\n          - lb_endpoints:\r\n              - endpoint:\r\n                  address:\r\n                    socket_address:\r\n                      address: 127.0.0.1\r\n                      port_value: 5678";

        public static byte[] YamlBytes { get; private set; } = null!;
        public static string YamlString => YAML;

        public void Setup()
        {
            YamlBytes = Encoding.UTF8.GetBytes(YAML);
        }
    }

    [PrebuildSetup(typeof(PrebuildSetup))]
    public class PerformanceTest
    {
        [Test]
        [Performance]
        public void SimpleParsing()
        {
            Measure.Method(() =>
                {
                    var parser = VYaml.YamlParser.FromBytes(PrebuildSetup.YamlBytes);
                    while (parser.Read())
                    {
                    }
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(100)
                .MeasurementCount(20)
                .GC()
                .SampleGroup("VYaml")
                .Run();

            Measure.Method(() =>
                {
                    using var reader = new StringReader(PrebuildSetup.YamlString);
                    var parser = new YamlDotNet.Core.Parser(reader);
                    while (parser.MoveNext())
                    {
                    }
                })
                .WarmupCount(5)
                .IterationsPerMeasurement(100)
                .MeasurementCount(20)
                .GC()
                .SampleGroup("YamlDotNet")
                .Run();
        }
    }
}
