using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;
using VYaml.Annotations;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class YamlSchemaGeneratorTest
    {
        [Test]
        public void ObjectMembers()
        {
            var root = Parse(YamlSchemaGenerator.Generate<SchemaServerConfig>());

            Assert.That(root.GetProperty("$schema").GetString(), Is.EqualTo("http://json-schema.org/draft-07/schema#"));
            Assert.That(root.GetProperty("type").GetString(), Is.EqualTo("object"));
            Assert.That(root.GetProperty("description").GetString(), Is.EqualTo("Server settings."));

            var properties = root.GetProperty("properties");
            Assert.That(properties.EnumerateObject().Select(x => x.Name), Is.EqualTo(new[]
            {
                "host", "port", "comment", "timeout", "logLevel", "tag_list", "weights",
                "endpoint", "endpoints", "ports", "extra",
            }));

            Assert.That(properties.GetProperty("host").GetProperty("type").GetString(), Is.EqualTo("string"));
            Assert.That(properties.GetProperty("host").GetProperty("description").GetString(), Is.EqualTo("The host name to listen."));
            Assert.That(properties.GetProperty("port").GetProperty("type").GetString(), Is.EqualTo("integer"));
            Assert.That(Strings(properties.GetProperty("comment").GetProperty("type")), Is.EqualTo(new[] { "string", "null" }));
            Assert.That(Strings(properties.GetProperty("timeout").GetProperty("type")), Is.EqualTo(new[] { "integer", "null" }));
            Assert.That(Strings(properties.GetProperty("logLevel").GetProperty("enum")), Is.EqualTo(new[] { "debug", "warningOnly", "err" }));
            Assert.That(properties.GetProperty("tag_list").GetProperty("items").GetProperty("type").GetString(), Is.EqualTo("string"));
            Assert.That(properties.GetProperty("weights").GetProperty("additionalProperties").GetProperty("type").GetString(), Is.EqualTo("number"));
            Assert.That(properties.GetProperty("ports").GetProperty("uniqueItems").GetBoolean(), Is.True);
            Assert.That(properties.GetProperty("extra").EnumerateObject(), Is.Empty);

            var endpoint = properties.GetProperty("endpoint").GetProperty("anyOf");
            Assert.That(endpoint[0].GetProperty("$ref").GetString(), Is.EqualTo("#/definitions/SchemaEndpoint"));
            Assert.That(endpoint[1].GetProperty("type").GetString(), Is.EqualTo("null"));
            Assert.That(properties.GetProperty("endpoints").GetProperty("items").GetProperty("$ref").GetString(), Is.EqualTo("#/definitions/SchemaEndpoint"));

            var definition = root.GetProperty("definitions").GetProperty("SchemaEndpoint");
            Assert.That(definition.GetProperty("properties").GetProperty("fallback").GetProperty("anyOf")[0].GetProperty("$ref").GetString(),
                Is.EqualTo("#/definitions/SchemaEndpoint"));
        }

        [Test]
        public void NamingConventionOptions()
        {
            var options = new YamlSerializerOptions { NamingConvention = NamingConvention.UpperCamelCase };

            var root = Parse(YamlSchemaGenerator.Generate<SchemaServerConfig>(options));
            var keys = root.GetProperty("properties").EnumerateObject().Select(x => x.Name).ToArray();
            Assert.That(keys, Does.Contain("Host"));
            Assert.That(keys, Does.Contain("LogLevel"));
            Assert.That(keys, Does.Contain("tag_list")); // alias is fixed
            Assert.That(Strings(root.GetProperty("properties").GetProperty("LogLevel").GetProperty("enum")),
                Is.EqualTo(new[] { "Debug", "WarningOnly", "err" }));

            // naming convention by type is fixed
            var snakeCase = Parse(YamlSchemaGenerator.Generate<SchemaSnakeCase>(options));
            Assert.That(snakeCase.GetProperty("properties").EnumerateObject().Select(x => x.Name), Is.EqualTo(new[] { "max_count" }));
        }

        [Test]
        public void ConstructorDefaultValues()
        {
            var root = Parse(YamlSchemaGenerator.Generate<SchemaWithDefaults>());
            var properties = root.GetProperty("properties");

            Assert.That(properties.GetProperty("name").TryGetProperty("default", out _), Is.False);
            Assert.That(properties.GetProperty("name").GetProperty("description").GetString(), Is.EqualTo("The name."));
            Assert.That(properties.GetProperty("count").GetProperty("default").GetInt32(), Is.EqualTo(3));
            Assert.That(properties.GetProperty("label").GetProperty("default").GetString(), Is.EqualTo("a\"b"));
            Assert.That(properties.GetProperty("ratio").GetProperty("default").GetDouble(), Is.EqualTo(0.5));
            Assert.That(properties.GetProperty("level").GetProperty("default").GetString(), Is.EqualTo("err"));
            Assert.That(properties.GetProperty("nothing").GetProperty("default").ValueKind, Is.EqualTo(JsonValueKind.Null));
        }

        [Test]
        public void ConstructorDefaultValues_Deserialize()
        {
            var result = YamlSerializer.Deserialize<SchemaWithDefaults>(System.Text.Encoding.UTF8.GetBytes("name: x"));
            Assert.That(result.Label, Is.EqualTo("a\"b"));
        }

        [Test]
        public void Union()
        {
            var root = Parse(YamlSchemaGenerator.Generate<SchemaShapeContainer>());
            var definitions = root.GetProperty("definitions");

            var shape = definitions.GetProperty("ISchemaShape");
            Assert.That(shape.GetProperty("anyOf").EnumerateArray().Select(x => x.GetProperty("$ref").GetString()), Is.EqualTo(new[]
            {
                "#/definitions/SchemaCircle",
                "#/definitions/SchemaRect",
            }));
            Assert.That(shape.GetProperty("description").GetString(), Does.Contain("!circle").And.Contain("!rect"));
            Assert.That(definitions.GetProperty("SchemaCircle").GetProperty("properties").GetProperty("radius").GetProperty("type").GetString(),
                Is.EqualTo("number"));
        }

        [Test]
        public void UnionByDiscriminatorKey()
        {
            var root = Parse(YamlSchemaGenerator.Generate<IDiscriminatorShape>());

            Assert.That(root.GetProperty("type").GetString(), Is.EqualTo("object"));
            Assert.That(Strings(root.GetProperty("required")), Is.EqualTo(new[] { "type" }));
            Assert.That(Strings(root.GetProperty("properties").GetProperty("type").GetProperty("enum")), Is.EqualTo(new[] { "circle", "rect" }));

            var anyOf = root.GetProperty("anyOf").EnumerateArray().ToArray();
            Assert.That(anyOf[0].GetProperty("properties").GetProperty("type").GetProperty("const").GetString(), Is.EqualTo("circle"));
            Assert.That(anyOf[0].GetProperty("allOf")[0].GetProperty("$ref").GetString(), Is.EqualTo("#/definitions/DiscriminatorCircle"));
            Assert.That(root.GetProperty("definitions").GetProperty("DiscriminatorRect").GetProperty("properties").TryGetProperty("width", out _), Is.True);
        }

        [Test]
        public void UnionByRequiredKey()
        {
            var root = Parse(YamlSchemaGenerator.Generate<Job>());
            var definitions = root.GetProperty("definitions");

            var step = definitions.GetProperty("IStep");
            Assert.That(step.GetProperty("type").GetString(), Is.EqualTo("object"));
            var anyOf = step.GetProperty("anyOf").EnumerateArray().ToArray();
            Assert.That(anyOf.Select(x => Strings(x.GetProperty("required")).Single()), Is.EqualTo(new[] { "uses", "run" }));
            Assert.That(anyOf.Select(x => x.GetProperty("allOf")[0].GetProperty("$ref").GetString()), Is.EqualTo(new[]
            {
                "#/definitions/ActionStep",
                "#/definitions/RunStep",
            }));
        }

        [Test]
        public void UnionByRequiredKey_NamingConvention()
        {
            // the key follows YamlSerializerOptions.NamingConvention unless it is fixed by the subtype
            var options = new YamlSerializerOptions { NamingConvention = NamingConvention.KebabCase };
            var root = Parse(YamlSchemaGenerator.Generate<UnionKeyBase>(options));
            Assert.That(root.GetProperty("anyOf").EnumerateArray().Select(x => Strings(x.GetProperty("required")).Single()),
                Is.EqualTo(new[] { "max_count", "alias" }));

            var job = Parse(YamlSchemaGenerator.Generate<Job>(new YamlSerializerOptions { NamingConvention = NamingConvention.UpperCamelCase }));
            Assert.That(job.GetProperty("definitions").GetProperty("IStep").GetProperty("anyOf").EnumerateArray()
                    .Select(x => Strings(x.GetProperty("required")).Single()),
                Is.EqualTo(new[] { "Uses", "Run" }));
        }

        [Test]
        public void RequiredMembers()
        {
            var modifier = Parse(YamlSchemaGenerator.Generate<WithRequiredModifier>());
            Assert.That(Strings(modifier.GetProperty("required")), Is.EqualTo(new[] { "name", "count" }));

            var attribute = Parse(YamlSchemaGenerator.Generate<WithRequiredAttribute>());
            Assert.That(Strings(attribute.GetProperty("required")), Is.EqualTo(new[] { "name", "max_count" }));

            var record = Parse(YamlSchemaGenerator.Generate<RequiredRecord>());
            Assert.That(Strings(record.GetProperty("required")), Is.EqualTo(new[] { "id" }));

            // no "required" when there is no required member
            var optional = Parse(YamlSchemaGenerator.Generate<SchemaEndpoint>());
            Assert.That(optional.TryGetProperty("required", out _), Is.False);
        }

        [Test]
        public void CollectionRoot()
        {
            var root = Parse(YamlSchemaGenerator.Generate<Dictionary<string, List<SchemaEndpoint>>>());
            Assert.That(root.GetProperty("type").GetString(), Is.EqualTo("object"));
            Assert.That(root.GetProperty("additionalProperties").GetProperty("items").GetProperty("$ref").GetString(),
                Is.EqualTo("#/definitions/SchemaEndpoint"));
            Assert.That(root.GetProperty("definitions").TryGetProperty("SchemaEndpoint", out _), Is.True);
        }

        [Test]
        public void Primitives()
        {
            Assert.That(Parse(YamlSchemaGenerator.Generate<bool>()).GetProperty("type").GetString(), Is.EqualTo("boolean"));
            Assert.That(Parse(YamlSchemaGenerator.Generate<byte>()).GetProperty("maximum").GetInt32(), Is.EqualTo(255));
            Assert.That(Parse(YamlSchemaGenerator.Generate<System.DateTime>()).GetProperty("format").GetString(), Is.EqualTo("date-time"));

            var tuple = Parse(YamlSchemaGenerator.Generate<(int, string)>());
            Assert.That(tuple.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("type").GetString()), Is.EqualTo(new[] { "integer", "string" }));
        }

        static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

        static string?[] Strings(JsonElement array) => array.EnumerateArray().Select(x => x.GetString()).ToArray();
    }
}
