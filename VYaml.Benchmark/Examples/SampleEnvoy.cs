using System.Text.Json.Serialization;
using Newtonsoft.Json;
using VYaml.Annotations;

namespace VYaml.Benchmark.Examples;

[YamlObject]
public partial class TypedConfig
{
    [YamlMember("@type")]
    [YamlDotNet.Serialization.YamlMember(Alias = "@type")]
    [JsonProperty(PropertyName = "@type")]
    [JsonPropertyName("@type")]
    public string Type { get; init; } = default!;

    [YamlMember("stat_prefix")]
    [JsonPropertyName("stat_prefix")]
    [JsonProperty(PropertyName = "stat_prefix")]
    public string StatPrefix { get; init; } = default!;

    [YamlMember("codec_type")]
    [JsonPropertyName("codec_type")]
    [JsonProperty(PropertyName = "codec_type")]
    public string CodecType { get; init; } = default!;

    [YamlMember("route_config")]
    [JsonPropertyName("route_config")]
    [JsonProperty(PropertyName = "route_config")]
    public RouteConfig RouteConfig { get; init; } = default!;
}

[YamlObject]
public partial class RouteConfig
{
    public string Name { get; init; } = default!;

    [YamlMember("virtual_hosts")]
    [JsonPropertyName("virtual_hosts")]
    [JsonProperty(PropertyName = "virtual_hosts")]
    public List<VirtualHost> VirtualHosts { get; init; } = default!;
}

[YamlObject]
public partial class Routes
{
    public Dictionary<string, string> Match { get; init; } = default!;
    public Dictionary<string, string> Route { get; init; } = default!;
}

[YamlObject]
public partial class VirtualHost
{
    public string Name { get; init; } = default!;
    public List<string> Domains { get; init; } = default!;
    public List<Routes> Routes { get; init; } = default!;
}

[YamlObject]
public partial class SocketAddress
{
    public string Address { get; init; } = default!;

    [YamlMember("port_value")]
    [JsonPropertyName("port_value")]
    [JsonProperty(PropertyName = "port_value")]
    public int PortValue { get; init; }
}

[YamlObject]
public partial class Address
{
    [YamlMember("socket_address")]
    [JsonPropertyName("socket_address")]
    [JsonProperty(PropertyName = "socket_address")]
    public SocketAddress SocketAddress { get; init; } = default!;
}

[YamlObject]
public partial class Admin
{
    public Address Address { get; init; } = default!;
}

[YamlObject]
public partial class Listener
{
    public string Name { get; init; } = default!;
    public Address Address { get; init; } = default!;

    [YamlMember("filter_chains")]
    [JsonPropertyName("filter_chains")]
    [JsonProperty(PropertyName = "filter_chains")]
    public List<FilterChain> FilterChains { get; init; } = default!;
}

[YamlObject]
public partial class FilterChain
{
    public List<Filter> Filters { get; init; } = default!;
}

[YamlObject]
public partial class Filter
{
    public string Name { get; init; } = default!;

    [YamlMember("typed_config")]
    [JsonPropertyName("typed_config")]
    [JsonProperty(PropertyName = "typed_config")]
    public TypedConfig TypedConfig { get; init; } = default!;

    [YamlMember("http_filters")]
    [JsonPropertyName("http_filters")]
    [JsonProperty(PropertyName = "http_filters")]
    public List<HttpFilter> HttpFilters { get; init; } = default!;
}

[YamlObject]
public partial class HttpFilter
{
    public string Name { get; init; } = default!;

    [YamlMember("typed_config")]
    [JsonPropertyName("typed_config")]
    [JsonProperty(PropertyName = "typed_config")]
    public TypedConfig TypedConfig { get; init; } = default!;
}

[YamlObject]
public partial class StaticResources
{
    public List<Listener> Listeners { get; init; } = default!;
}

[YamlObject]
public partial class SampleEnvoy
{
    public Admin Admin { get; init; } = default!;

    [YamlMember("static_resources")]
    [JsonPropertyName("static_resources")]
    [JsonProperty(PropertyName = "static_resources")]
    public StaticResources StaticResources { get; init; } = default!;
}
