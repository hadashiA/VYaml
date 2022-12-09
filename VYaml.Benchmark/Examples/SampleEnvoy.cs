using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using VYaml.Serialization;

namespace VYaml.Benchmark.Examples;

[YamlObject]
public partial class TypedConfig
{
    [YamlMember("@type")]
    [YamlDotNet.Serialization.YamlMember(Alias = "@type")]
    [JsonPropertyName("@type")]
    public string Type { get; init; }

    [YamlMember("stat_prefix")]
    [JsonPropertyName("stat_prefix")]
    public string StatPrefix { get; init; }

    [YamlMember("codec_type")]
    [JsonPropertyName("codec_type")]
    public string CodecType { get; init; }

    [YamlMember("route_config")]
    [JsonPropertyName("route_config")]
    public RouteConfig RouteConfig { get; init; }
}

[YamlObject]
public partial class RouteConfig
{
    public string Name { get; init; }

    [YamlMember("virtual_hosts")]
    [JsonPropertyName("virtual_hosts")]
    public List<VirtualHost> VirtualHosts { get; init; }
}

[YamlObject]
public partial class Routes
{
    public Dictionary<string, string> Match { get; init; }
    public Dictionary<string, string> Route { get; init; }
}

[YamlObject]
public partial class VirtualHost
{
    public string Name { get; init; }
    public List<string> Domains { get; init; }
    public List<Routes> Routes { get; init; }
}

[YamlObject]
public partial class SocketAddress
{
    public string Address { get; init; }

    [YamlMember("port_value")]
    [JsonPropertyName("port_value")]
    public int PortValue { get; init; }
}

[YamlObject]
public partial class Address
{
    [YamlMember("socket_address")]
    [JsonPropertyName("socket_address")]
    public SocketAddress SocketAddress { get; init; }
}

[YamlObject]
public partial class Admin
{
    public Address Address { get; init; }
}

[YamlObject]
public partial class Listener
{
    public string Name { get; init; }
    public Address Address { get; init; }

    [YamlMember("filter_chains")]
    [JsonPropertyName("filter_chains")]
    public List<FilterChain> FilterChains { get; init; }
}

[YamlObject]
public partial class FilterChain
{
    public List<Filter> Filters { get; init; }
}

[YamlObject]
public partial class Filter
{
    public string Name { get; init; }

    [YamlMember("typed_config")]
    [JsonPropertyName("typed_config")]
    public TypedConfig TypedConfig { get; init; }

    [YamlMember("http_filters")]
    [JsonPropertyName("http_filters")]
    public List<HttpFilter> HttpFilters { get; init; }
}

[YamlObject]
public partial class HttpFilter
{
    public string Name { get; init; }

    [YamlMember("typed_config")]
    [JsonPropertyName("typed_config")]
    public TypedConfig TypedConfig { get; init; }
}

[YamlObject]
public partial class StaticResources
{
    public List<Listener> Listeners { get; init; }
}

[YamlObject]
public partial class SampleEnvoy
{
    public Admin Admin { get; init; }

    [YamlMember("static_resources")]
    [JsonPropertyName("static_resources")]
    public StaticResources StaticResources { get; init; }
}
