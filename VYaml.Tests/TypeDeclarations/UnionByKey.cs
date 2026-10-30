using System.Collections.Generic;
using VYaml.Annotations;

namespace VYaml.Tests.TypeDeclarations
{
    [YamlObject]
    [YamlObjectUnionDiscriminator("type")]
    [YamlObjectUnion("circle", typeof(DiscriminatorCircle))]
    [YamlObjectUnion("rect", typeof(DiscriminatorRect))]
    public partial interface IDiscriminatorShape
    {
    }

    [YamlObject]
    public partial class DiscriminatorCircle : IDiscriminatorShape
    {
        public double Radius { get; set; }
    }

    [YamlObject]
    public partial class DiscriminatorRect : IDiscriminatorShape
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }

    [YamlObject]
    [YamlObjectUnionDiscriminator("kind")]
    [YamlObjectUnion("empty", typeof(DiscriminatorEmpty))]
    [YamlObjectUnion("named", typeof(DiscriminatorNamed))]
    public abstract partial class DiscriminatorBase
    {
    }

    [YamlObject]
    public partial class DiscriminatorEmpty : DiscriminatorBase
    {
    }

    [YamlObject]
    public partial class DiscriminatorNamed : DiscriminatorBase
    {
        public string Name { get; }
        public int Count { get; }

        public DiscriminatorNamed(string name, int count = 1)
        {
            Name = name;
            Count = count;
        }
    }

    [YamlObject]
    public partial class DiscriminatorContainer
    {
        public List<IDiscriminatorShape> Shapes { get; set; } = new();
        public DiscriminatorBase? Base { get; set; }
    }

    // like the steps of GitHub Actions workflows
    [YamlObject]
    [YamlObjectUnionKey("uses", typeof(ActionStep))]
    [YamlObjectUnionKey(nameof(RunStep.Run), typeof(RunStep))]
    public partial interface IStep
    {
    }

    [YamlObject]
    public partial class ActionStep : IStep
    {
        public string? Name { get; set; }
        public string Uses { get; set; } = "";
        public Dictionary<string, string>? With { get; set; }
    }

    [YamlObject]
    public partial class RunStep : IStep
    {
        public string? Name { get; set; }
        public string Run { get; set; } = "";
        public string? Shell { get; set; }
    }

    [YamlObject]
    public partial class Job
    {
        public List<IStep> Steps { get; set; } = new();
    }

    [YamlObject]
    [YamlObjectUnionKey("max_count", typeof(UnionKeySnakeCase))]
    [YamlObjectUnionKey("alias", typeof(UnionKeyAlias))]
    public abstract partial class UnionKeyBase
    {
    }

    [YamlObject(NamingConvention.SnakeCase)]
    public partial class UnionKeySnakeCase : UnionKeyBase
    {
        public int MaxCount { get; set; }
    }

    [YamlObject]
    public partial class UnionKeyAlias : UnionKeyBase
    {
        [YamlMember("alias")]
        public string Value { get; set; } = "";
    }
}
