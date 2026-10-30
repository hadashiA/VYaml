using System.Collections.Generic;
using System.Runtime.Serialization;
using VYaml.Annotations;

namespace VYaml.Tests.TypeDeclarations
{
    /// <summary>
    /// Server settings.
    /// </summary>
    [YamlObject]
    public partial class SchemaServerConfig
    {
        /// <summary>
        /// The host name
        /// to listen.
        /// </summary>
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public string? Comment { get; set; }
        public int? Timeout { get; set; }
        public SchemaLogLevel LogLevel { get; set; }
        [YamlMember("tag_list")]
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, double> Weights { get; set; } = new();
        public SchemaEndpoint? Endpoint { get; set; }
        public SchemaEndpoint[] Endpoints { get; set; } = [];
        public HashSet<int> Ports { get; set; } = new();
        public object? Extra { get; set; }

        [YamlIgnore]
        public int Ignored { get; set; }
    }

    [YamlObject]
    public partial class SchemaEndpoint
    {
        public string Path { get; set; } = "";
        public SchemaEndpoint? Fallback { get; set; }
    }

    public enum SchemaLogLevel
    {
        Debug,
        WarningOnly,
        [EnumMember(Value = "err")]
        Error,
    }

    [YamlObject(NamingConvention.SnakeCase)]
    public partial class SchemaSnakeCase
    {
        public int MaxCount { get; set; }
    }

    /// <param name="Name">The name.</param>
    [YamlObject]
    public partial record SchemaWithDefaults(
        string Name,
        int Count = 3,
        string Label = "a\"b",
        double Ratio = 0.5,
        SchemaLogLevel Level = SchemaLogLevel.Error,
        string? Nothing = null);

    [YamlObject]
    [YamlObjectUnion("!circle", typeof(SchemaCircle))]
    [YamlObjectUnion("!rect", typeof(SchemaRect))]
    public partial interface ISchemaShape
    {
    }

    [YamlObject]
    public partial class SchemaCircle : ISchemaShape
    {
        public double Radius { get; set; }
    }

    [YamlObject]
    public partial class SchemaRect : ISchemaShape
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }

    [YamlObject]
    public partial class SchemaShapeContainer
    {
        public List<ISchemaShape> Shapes { get; set; } = new();
    }

    // serialized by a custom formatter, e.g. `3`, `inf` or `{ skip: 1, count: 2 }`
    public struct SchemaSkillRange
    {
        public int Skip;
        public int? Count;
    }

    [YamlObject(NamingConvention.KebabCase)]
    public enum SchemaMood
    {
        MostAnxious,
        Calm,
    }

    [YamlObject]
    public partial class SchemaCard
    {
        public int Rarity { get; }
        public SchemaSkillRange Range { get; }
        public SchemaSkillRange? OptionalRange { get; }
        public List<string> Skills { get; }
        public SchemaMood Mood { get; }

        public SchemaCard(int rarity = 1, SchemaSkillRange range = default, SchemaSkillRange? optionalRange = null, List<string> skills = null!, SchemaMood mood = SchemaMood.MostAnxious)
        {
            Rarity = rarity;
            Range = range;
            OptionalRange = optionalRange;
            Skills = skills;
            Mood = mood;
        }
    }
}
