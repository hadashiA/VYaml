// See https://aka.ms/new-console-template for more information


using VYaml;

var path = Path.Combine(Directory.GetCurrentDirectory(), "sample_envoy.yaml");
var bytes = File.ReadAllBytes(path);
var parser = Parser.FromBytes(bytes);

while (parser.Read())
{
}

