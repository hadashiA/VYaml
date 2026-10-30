using System.Collections.Generic;
using VYaml.Annotations;

namespace VYaml.Tests.TypeDeclarations
{
    [YamlObject]
    [YamlObjectUnion("circle", typeof(DiscriminatorCircle), DiscriminatorKey = "type")]
    [YamlObjectUnion("rect", typeof(DiscriminatorRect), DiscriminatorKey = "type")]
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
    [YamlObjectUnion("empty", typeof(DiscriminatorEmpty), DiscriminatorKey = "kind")]
    [YamlObjectUnion("named", typeof(DiscriminatorNamed), DiscriminatorKey = "kind")]
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
    [YamlObjectUnion(typeof(ActionStep), RequiredKey = "uses")]
    [YamlObjectUnion(typeof(RunStep), RequiredKey = nameof(RunStep.Run))]
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
    [YamlObjectUnion(typeof(UnionKeySnakeCase), RequiredKey = "max_count")]
    [YamlObjectUnion(typeof(UnionKeyAlias), RequiredKey = "alias")]
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
