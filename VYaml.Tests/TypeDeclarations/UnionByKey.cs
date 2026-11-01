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

    // the key of each subtype is given by [YamlMember] on an override property
    [YamlObject]
    [YamlObjectUnion("hug", typeof(OverrideKeyHug), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("wave", typeof(OverrideKeyWave), Style = YamlUnionStyle.Wrapped)]
    public abstract partial class OverrideKeyBase
    {
        public abstract string Name { get; set; }
        public virtual string? Note { get; set; }
        public int Power { get; set; }
    }

    [YamlObject]
    public partial class OverrideKeyHug : OverrideKeyBase
    {
        [YamlMember("hug")]
        public override string Name { get; set; } = "";
    }

    [YamlObject]
    public partial class OverrideKeyWave : OverrideKeyBase
    {
        [YamlMember("wave")]
        public override string Name { get; set; } = "";

        [YamlIgnore]
        public override string? Note { get; set; }
    }

    // wrapped by a mapping with one key: `- violence: { name: Punch, power: 1 }`
    [YamlObject]
    [YamlObjectUnion("violence", typeof(ViolenceSkill), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("hug", typeof(HugSkill), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("meditate", typeof(MeditateSkill), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("shout", typeof(ShoutSkill), Style = YamlUnionStyle.Wrapped)]
    public partial interface ISkill
    {
    }

    /// <summary>
    /// Hits the target.
    /// </summary>
    [YamlObject]
    public partial class ViolenceSkill : ISkill
    {
        public string Name { get; set; } = "";
        public int Power { get; set; }
    }

    [YamlObject]
    public partial class HugSkill : ISkill
    {
        public string Name { get; set; } = "";
    }

    [YamlObject]
    public partial class MeditateSkill : ISkill
    {
    }

    [YamlObject]
    public partial class ShoutSkill : ISkill
    {
        [YamlRequired]
        public string Words { get; set; } = "";
    }

    [YamlObject]
    public partial class Character
    {
        public List<ISkill> Skills { get; set; } = new();
    }
}
