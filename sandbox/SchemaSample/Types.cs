using System.Collections.Generic;
using VYaml.Annotations;

namespace SchemaSample
{
    /// <summary>
    /// Settings of a server. The root of the sample YAML.
    /// </summary>
    [YamlObject]
    public partial class ServerConfig
    {
        /// <summary>The host name to listen.</summary>
        public required string Host { get; set; }

        /// <summary>The port to listen.</summary>
        [YamlRequired]
        public int Port { get; set; }

        public LogLevel LogLevel { get; set; }

        /// <summary>Endpoints served by this server.</summary>
        public List<Endpoint> Endpoints { get; set; } = new();

        /// <summary>Shapes drawn on the status page, identified by a YAML tag.</summary>
        public List<IShape> Shapes { get; set; } = new();

        /// <summary>Health checks, identified by the <c>type</c> key.</summary>
        public List<IHealthCheck> HealthChecks { get; set; } = new();

        /// <summary>Where to send alerts, identified by the key that wraps each entry.</summary>
        public List<INotification> Notifications { get; set; } = new();
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
    }

    /// <summary>An HTTP endpoint.</summary>
    /// <param name="Path">The request path, e.g. <c>/api/users</c>.</param>
    /// <param name="Timeout">Timeout in seconds.</param>
    [YamlObject]
    public partial record Endpoint(string Path, int Timeout = 30);

    // --- Union by YAML tag ---------------------------------------------------

    /// <summary>A shape, written with a YAML tag: <c>!circle { radius: 1 }</c>.</summary>
    [YamlObject]
    [YamlObjectUnion("!circle", typeof(Circle))]
    [YamlObjectUnion("!rect", typeof(Rect))]
    public partial interface IShape
    {
    }

    [YamlObject]
    public partial class Circle : IShape
    {
        public double Radius { get; set; }
    }

    [YamlObject]
    public partial class Rect : IShape
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }

    // --- Union by discriminator key -------------------------------------------

    /// <summary>A health check, identified by the value of the <c>type</c> key.</summary>
    [YamlObject]
    [YamlObjectUnion("http", typeof(HttpHealthCheck), DiscriminatorKey = "type")]
    [YamlObjectUnion("tcp", typeof(TcpHealthCheck), DiscriminatorKey = "type")]
    public partial interface IHealthCheck
    {
    }

    /// <summary>Requests a URL and expects a 2xx response.</summary>
    /// <param name="Url">The URL to request.</param>
    /// <param name="IntervalSeconds">Seconds between requests (a constructor default becomes "default" in the schema).</param>
    [YamlObject]
    public partial record HttpHealthCheck(string Url, int IntervalSeconds = 10) : IHealthCheck;

    /// <summary>Opens a TCP connection to the port.</summary>
    [YamlObject]
    public partial class TcpHealthCheck : IHealthCheck
    {
        public required int Port { get; set; }
    }

    // --- Union by wrapping key ------------------------------------------------

    /// <summary>A notification target, written as a mapping with one key: <c>slack: { channel: "#alerts" }</c>.</summary>
    [YamlObject]
    [YamlObjectUnion("slack", typeof(SlackNotification), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("email", typeof(EmailNotification), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("webhook", typeof(WebhookNotification), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("console", typeof(ConsoleNotification), Style = YamlUnionStyle.Wrapped)]
    public partial interface INotification
    {
    }

    /// <summary>Posts to a Slack channel.</summary>
    [YamlObject]
    public partial class SlackNotification : INotification
    {
        public required string Channel { get; set; }
    }

    /// <summary>Sends an email.</summary>
    [YamlObject]
    public partial class EmailNotification : INotification
    {
        public required string To { get; set; }
        public string? Subject { get; set; }
    }

    /// <summary>Calls a webhook.</summary>
    [YamlObject]
    public partial class WebhookNotification : INotification
    {
        public required string Url { get; set; }
    }

    /// <summary>Writes to the console. Has no settings: <c>console:</c> or <c>console: {}</c>.</summary>
    [YamlObject]
    public partial class ConsoleNotification : INotification
    {
    }
}
