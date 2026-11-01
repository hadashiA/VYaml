using System;
using System.IO;
using System.Text;
using SchemaSample;
using VYaml.JsonSchema;
using VYaml.Serialization;

// Writes server-config.schema.json and server-config.yaml next to the project,
// then reads the YAML back. Open server-config.yaml in an editor with
// yaml-language-server (VS Code YAML extension) to see completion and validation.

var dir = FindProjectDirectory();

var schema = YamlSchemaGenerator.Generate<ServerConfig>(
    YamlSerializerOptions.Standard,
    new YamlSchemaGeneratorOptions { AdditionalProperties = false });
File.WriteAllText(Path.Combine(dir, "server-config.schema.json"), schema);

const string yaml = """
    # yaml-language-server: $schema=./server-config.schema.json
    host: localhost
    port: 8080
    logLevel: info

    endpoints:
      - path: /api/users
      - path: /api/orders
        timeout: 60

    # union by YAML tag
    shapes:
      - !circle { radius: 1.5 }
      - !rect
        width: 2
        height: 3

    # union by discriminator key
    healthChecks:
      - type: http
        url: http://localhost:8080/healthz
      - type: tcp
        port: 5432

    # union by wrapping key
    notifications:
      - slack: { channel: "#alerts" }
      - email:
          to: ops@example.com
          subject: "[alert] server"
      - webhook: { url: https://example.com/hook }
      - console:

    """;
File.WriteAllText(Path.Combine(dir, "server-config.yaml"), yaml);

var config = YamlSerializer.Deserialize<ServerConfig>(Encoding.UTF8.GetBytes(yaml));
Console.WriteLine($"host: {config.Host}:{config.Port} ({config.LogLevel})");
Console.WriteLine($"endpoints: {string.Join(", ", config.Endpoints.ConvertAll(x => $"{x.Path} ({x.Timeout}s)"))}");
Console.WriteLine($"shapes: {string.Join(", ", config.Shapes.ConvertAll(x => x.GetType().Name))}");
Console.WriteLine($"healthChecks: {string.Join(", ", config.HealthChecks.ConvertAll(x => x.GetType().Name))}");
Console.WriteLine($"notifications: {string.Join(", ", config.Notifications.ConvertAll(x => x.GetType().Name))}");
Console.WriteLine();
Console.WriteLine("round trip:");
Console.WriteLine(YamlSerializer.SerializeToString(config));
Console.WriteLine($"wrote {Path.Combine(dir, "server-config.schema.json")}");
Console.WriteLine($"wrote {Path.Combine(dir, "server-config.yaml")}");

static string FindProjectDirectory()
{
    // bin/Debug/net9.0 -> project directory
    var dir = AppContext.BaseDirectory;
    while (dir != null && !File.Exists(Path.Combine(dir, "SchemaSample.csproj")))
    {
        dir = Path.GetDirectoryName(dir);
    }
    return dir ?? Directory.GetCurrentDirectory();
}
