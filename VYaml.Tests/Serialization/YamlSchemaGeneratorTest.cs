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
            Assert.That(properties.GetProperty("nothing").TryGetProperty("default", out _), Is.False);
            // char is an integer (code point), float keeps its single-precision text
            Assert.That(properties.GetProperty("separator").GetProperty("default").GetInt32(), Is.EqualTo(','));
            Assert.That(properties.GetProperty("weight").GetProperty("default").GetRawText(), Is.EqualTo("0.1"));
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
            Assert.That(Strings(anyOf[0].GetProperty("required")), Is.EqualTo(new[] { "type" }));
            Assert.That(anyOf[0].GetProperty("properties").GetProperty("radius").GetProperty("type").GetString(), Is.EqualTo("number"));
            Assert.That(anyOf[1].GetProperty("properties").TryGetProperty("width", out _), Is.True);
        }

        [Test]
        public void UnionByDiscriminatorKey_CustomSubTypeSchema()
        {
            var schemaOptions = new YamlSchemaGeneratorOptions().AddTypeSchema<DiscriminatorCircle>("""{ "type": "object" }""");
            var root = Parse(YamlSchemaGenerator.Generate<IDiscriminatorShape>(null, schemaOptions));
            var circle = root.GetProperty("anyOf")[0];
            Assert.That(circle.GetProperty("properties").GetProperty("type").GetProperty("const").GetString(), Is.EqualTo("circle"));
            Assert.That(Strings(circle.GetProperty("required")), Is.EqualTo(new[] { "type" }));
            Assert.That(circle.GetProperty("allOf")[0].GetProperty("allOf")[0].GetProperty("type").GetString(), Is.EqualTo("object"));
        }

        [Test]
        public void UnionByRequiredKey()
        {
            var root = Parse(YamlSchemaGenerator.Generate<Job>());
            var definitions = root.GetProperty("definitions");

            var step = definitions.GetProperty("IStep");
            Assert.That(step.GetProperty("type").GetString(), Is.EqualTo("object"));
            // the keys are listed for completion
            Assert.That(step.GetProperty("properties").EnumerateObject().Select(x => x.Name), Is.EqualTo(new[] { "uses", "run" }));
            // if/then/else chain in the order of declaration (the first one wins)
            Assert.That(Chain(step), Is.EqualTo(new[]
            {
                ("uses", "#/definitions/ActionStep"),
                ("run", "#/definitions/RunStep"),
            }));
            // none of the keys
            var last = step.GetProperty("else").GetProperty("else");
            Assert.That(last.GetProperty("anyOf").EnumerateArray().Select(x => Strings(x.GetProperty("required")).Single()),
                Is.EqualTo(new[] { "uses", "run" }));
        }

        [Test]
        public void UnionByRequiredKey_NamingConvention()
        {
            // the key follows YamlSerializerOptions.NamingConvention unless it is fixed by the subtype
            var options = new YamlSerializerOptions { NamingConvention = NamingConvention.KebabCase };
            var root = Parse(YamlSchemaGenerator.Generate<UnionKeyBase>(options));
            Assert.That(Chain(root).Select(x => x.Key), Is.EqualTo(new[] { "max_count", "alias" }));

            var job = Parse(YamlSchemaGenerator.Generate<Job>(new YamlSerializerOptions { NamingConvention = NamingConvention.UpperCamelCase }));
            Assert.That(Chain(job.GetProperty("definitions").GetProperty("IStep")).Select(x => x.Key),
                Is.EqualTo(new[] { "Uses", "Run" }));
        }

        [Test]
        public void UnionByRequiredKey_OverrideMember()
        {
            var root = Parse(YamlSchemaGenerator.Generate<OverrideKeyBase>());
            Assert.That(Chain(root).Select(x => x.Key), Is.EqualTo(new[] { "hug", "wave" }));

            var definitions = root.GetProperty("definitions");
            Assert.That(definitions.GetProperty("OverrideKeyHug").GetProperty("properties").EnumerateObject().Select(x => x.Name),
                Is.EqualTo(new[] { "hug", "note", "power" }));
            Assert.That(definitions.GetProperty("OverrideKeyWave").GetProperty("properties").EnumerateObject().Select(x => x.Name),
                Is.EqualTo(new[] { "wave", "power" }));
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
        public void AdditionalProperties()
        {
            var allowed = Parse(YamlSchemaGenerator.Generate<SchemaEndpoint>());
            Assert.That(allowed.TryGetProperty("additionalProperties", out _), Is.False);

            var disallowed = Parse(YamlSchemaGenerator.Generate<SchemaServerConfig>(null, new YamlSchemaGeneratorOptions { AdditionalProperties = false }));
            Assert.That(disallowed.GetProperty("additionalProperties").GetBoolean(), Is.False);
            Assert.That(disallowed.GetProperty("definitions").GetProperty("SchemaEndpoint").GetProperty("additionalProperties").GetBoolean(), Is.False);
            // dictionaries keep their additionalProperties for values
            Assert.That(disallowed.GetProperty("properties").GetProperty("weights").GetProperty("additionalProperties").GetProperty("type").GetString(),
                Is.EqualTo("number"));

            // follows UnmappedMemberHandling by default
            var options = new YamlSerializerOptions { UnmappedMemberHandling = YamlUnmappedMemberHandling.Disallow };
            Assert.That(Parse(YamlSchemaGenerator.Generate<SchemaEndpoint>(options)).GetProperty("additionalProperties").GetBoolean(), Is.False);
            Assert.That(Parse(YamlSchemaGenerator.Generate<SchemaEndpoint>(options, new YamlSchemaGeneratorOptions { AdditionalProperties = true }))
                .TryGetProperty("additionalProperties", out _), Is.False);
        }

        [Test]
        public void AdditionalProperties_DiscriminatorKey()
        {
            // the subtypes are expanded so that "additionalProperties": false does not reject the discriminator key
            var root = Parse(YamlSchemaGenerator.Generate<IDiscriminatorShape>(null, new YamlSchemaGeneratorOptions { AdditionalProperties = false }));
            var circle = root.GetProperty("anyOf")[0];
            Assert.That(circle.GetProperty("additionalProperties").GetBoolean(), Is.False);
            Assert.That(circle.GetProperty("properties").EnumerateObject().Select(x => x.Name), Is.EqualTo(new[] { "type", "radius" }));
            Assert.That(circle.GetProperty("properties").GetProperty("type").GetProperty("const").GetString(), Is.EqualTo("circle"));
            Assert.That(Strings(circle.GetProperty("required")), Is.EqualTo(new[] { "type" }));
        }

        [Test]
        public void CustomSchemas()
        {
            var schemaOptions = new YamlSchemaGeneratorOptions()
                .AddTypeSchema<SchemaSkillRange>("""
                    { "anyOf": [{ "type": "integer" }, { "const": "inf" }, { "type": "object", "properties": { "skip": { "type": "integer" }, "count": { "type": "integer" } } }] }
                    """)
                .AddMemberSchema<SchemaCard>(nameof(SchemaCard.Rarity), """{ "anyOf": [{ "type": "integer" }, { "const": "inf" }] }""");
            var json = YamlSchemaGenerator.Generate<SchemaCard>(null, schemaOptions);
            var properties = Parse(json).GetProperty("properties");

            var range = properties.GetProperty("range").GetProperty("allOf")[0];
            Assert.That(range.GetProperty("anyOf")[1].GetProperty("const").GetString(), Is.EqualTo("inf"));
            // Nullable<T> of a custom type allows null
            Assert.That(properties.GetProperty("optionalRange").GetProperty("anyOf")[1].GetProperty("type").GetString(), Is.EqualTo("null"));

            var rarity = properties.GetProperty("rarity");
            Assert.That(rarity.GetProperty("allOf")[0].GetProperty("anyOf")[1].GetProperty("const").GetString(), Is.EqualTo("inf"));
            Assert.That(rarity.GetProperty("default").GetInt32(), Is.EqualTo(1));
        }

        [Test]
        public void DefaultValues_NullIsOmitted()
        {
            var properties = Parse(YamlSchemaGenerator.Generate<SchemaCard>()).GetProperty("properties");
            Assert.That(properties.GetProperty("range").TryGetProperty("default", out _), Is.False);
            Assert.That(properties.GetProperty("optionalRange").TryGetProperty("default", out _), Is.False);
            Assert.That(properties.GetProperty("skills").TryGetProperty("default", out _), Is.False);
        }

        [Test]
        public void EnumNamingConventionByType()
        {
            // [YamlObject(NamingConvention)] of the enum itself is used, same as EnumAsStringFormatter
            var mood = Parse(YamlSchemaGenerator.Generate<SchemaCard>()).GetProperty("properties").GetProperty("mood");
            Assert.That(Strings(mood.GetProperty("enum")), Is.EqualTo(new[] { "most-anxious", "calm" }));
            Assert.That(mood.GetProperty("default").GetString(), Is.EqualTo("most-anxious"));
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

        // (required key, $ref of then) of an if/then/else chain
        static (string? Key, string? Ref)[] Chain(JsonElement schema)
        {
            var result = new List<(string?, string?)>();
            while (schema.TryGetProperty("if", out var condition))
            {
                result.Add((Strings(condition.GetProperty("required")).Single(), schema.GetProperty("then").GetProperty("$ref").GetString()));
                schema = schema.GetProperty("else");
            }
            return result.ToArray();
        }
    }
}
