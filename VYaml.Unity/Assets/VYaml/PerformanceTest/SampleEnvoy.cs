using System.Collections.Generic;
using VYaml.Annotations;

namespace VYaml.PerformanceTest
{
    [YamlObject]
    public partial class TypedConfig
    {
        [YamlMember("@type")]
        [YamlDotNet.Serialization.YamlMember(Alias = "@type")]
        public string Type { get; set; }

        [YamlMember("stat_prefix")]
        public string StatPrefix { get; set; }

        [YamlMember("codec_type")]
        public string CodecType { get; set; }

        [YamlMember("route_config")]
        public RouteConfig RouteConfig { get; set; }
    }

    [YamlObject]
    public partial class RouteConfig
    {
        public string Name { get; set; }

        [YamlMember("virtual_hosts")]
        public List<VirtualHost> VirtualHosts { get; set; }
    }

    [YamlObject]
    public partial class Routes
    {
        public Dictionary<string, string> Match { get; set; }
        public Dictionary<string, string> Route { get; set; }
    }

    [YamlObject]
    public partial class VirtualHost
    {
        public string Name { get; set; }
        public List<string> Domains { get; set; }
        public List<Routes> Routes { get; set; }
    }

    [YamlObject]
    public partial class SocketAddress
    {
        public string Address { get; set; }

        [YamlMember("port_value")]
        public string PortValue { get; set; }
    }

    [YamlObject]
    public partial class Address
    {
        [YamlMember("socket_address")]
        public SocketAddress SocketAddress { get; set; }
    }

    [YamlObject]
    public partial class Admin
    {
        public Address Address { get; set; }
    }

    [YamlObject]
    public partial class Listener
    {
        public string Name { get; set; }
        public Address Address { get; set; }

        [YamlMember("filter_chains")]
        public List<FilterChain> FilterChains { get; set; }
    }

    [YamlObject]
    public partial class FilterChain
    {
        public List<Filter> Filters { get; set; }
    }

    [YamlObject]
    public partial class Filter
    {
        public string Name { get; set; }

        [YamlMember("typed_config")]
        public TypedConfig TypedConfig { get; set; }

        [YamlMember("http_filters")]
        public List<HttpFilter> HttpFilters { get; set; }
    }

    [YamlObject]
    public partial class HttpFilter
    {
        public string Name { get; set; }

        [YamlMember("typed_config")]
        public TypedConfig TypedConfig { get; set; }
    }

    [YamlObject]
    public partial class StaticResources
    {
        public List<Listener> Listeners { get; set; }
    }

    [YamlObject]
    public partial class SampleEnvoy
    {
        public Admin Admin { get; set; }

        [YamlMember("static_resources")]
        public StaticResources StaticResources { get; set; }
    }
}