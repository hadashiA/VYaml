using VYaml.Serialization;

namespace VYaml.Benchmark.Examples;

[YamlObject]
public partial class TypedConfig
{
    [YamlMember("@type")]
    public string Type { get; init; }

    [YamlMember("stat_prefix")]
    public string StatPrefix { get; init; }

    [YamlMember("codec_type")]
    public string CodecType { get; init; }

    [YamlMember("route_config")]
    public RouteConfig RouteConfig { get; init; }
}

[YamlObject]
public partial class RouteConfig
{
    public string Name { get; init; }
    //
    // [YamlMember("virtual_hosts")]
    // public IReadOnlyList<VirtualHost> VirtualHosts { get; init; }
}

[YamlObject]
public partial class Route
{
    public IReadOnlyDictionary<string, string> Match { get; init; }
    public IReadOnlyDictionary<string, string> Routes { get; init; }
}

[YamlObject]
public partial class VirtualHost
{
    public string Name { get; init; }
    public IReadOnlyList<string> Domains { get; init; }
    public IReadOnlyList<Route> Routes { get; init; }
}

[YamlObject]
public partial class SocketAddress
{
    public string Address { get; init; }

    [YamlMember("port_value")]
    public string PortValue { get; init; }
}

[YamlObject]
public partial class Address
{
    [YamlMember("socket_address")]
    public SocketAddress SocketAddress { get; init; }
}

[YamlObject]
public partial class Admin
{
    [YamlMember("address")]
    public Address Address { get; init; }
}

[YamlObject]
public partial class Listener
{
    public string Name { get; init; }
    public Address Address { get; init; }

    [YamlMember("filter_chains")]
    public IReadOnlyList<FilterChain> FilterChains { get; init; }
}

[YamlObject]
public partial class FilterChain
{
    public IReadOnlyList<Filter> Filters { get; init; }
}

[YamlObject]
public partial class Filter
{
    public string Name { get; init; }

    [YamlMember("typed_config")]
    public TypedConfig TypedConfig { get; init; }

    // [YamlMember("http_filters")]
    // public IReadOnlyList<Filter> HttpFilters { get; init; }
}

[YamlObject]
public partial class HttpFilter
{
    public string Name { get; init; }

}

[YamlObject]
public partial class StaticResources
{
    [YamlMember("listeners")]
    public IReadOnlyList<Listener> Listeners { get; init; }
}

[YamlObject]
public partial class SampleEnvoy
{
    public Admin Admin { get; init; }

    [YamlMember("static_resources")]
    public StaticResources StaticResources { get; init; }
}
