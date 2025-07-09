using System;
using VYaml.Annotations;

namespace VYaml.Tests.TypeDeclarations
{
    // Test for YamlUnionMemberAttribute - interface approach
    [YamlObject]
    public partial interface IUnionWithMemberAttribute
    {
        public int Value { get; }
    }

    [YamlObject]
    [YamlUnionMember("!member1", typeof(IUnionWithMemberAttribute))]
    public partial class UnionMember1 : IUnionWithMemberAttribute
    {
        public int Value { get; set; }
        public string Name { get; set; } = default!;
    }

    [YamlObject]
    [YamlUnionMember("!member2", typeof(IUnionWithMemberAttribute))]
    public partial class UnionMember2 : IUnionWithMemberAttribute
    {
        public int Value { get; set; }
        public decimal Price { get; set; }
    }

    // Test for YamlUnionMemberAttribute - abstract class approach
    [YamlObject]
    public abstract partial class AbstractUnionWithMemberAttribute
    {
        public abstract string Type { get; set; }
    }

    [YamlObject]
    [YamlUnionMember("!concrete1", typeof(AbstractUnionWithMemberAttribute))]
    public partial class ConcreteUnionMember1 : AbstractUnionWithMemberAttribute
    {
        public override string Type { get; set; } = "Type1";
        public int Count { get; set; }
    }

    [YamlObject]
    [YamlUnionMember("!concrete2", typeof(AbstractUnionWithMemberAttribute))]
    public partial class ConcreteUnionMember2 : AbstractUnionWithMemberAttribute
    {
        public override string Type { get; set; } = "Type2";
        public bool IsActive { get; set; }
    }

    // Test for mixed approach - both attributes
    [YamlObject]
    [YamlObjectUnion("!mixed1", typeof(MixedUnionMember1))]
    public partial interface IMixedUnion
    {
        public string Id { get; }
    }

    [YamlObject]
    public partial class MixedUnionMember1 : IMixedUnion
    {
        public string Id { get; set; } = default!;
        public int Version { get; set; }
    }

    [YamlObject]
    [YamlUnionMember("!mixed2", typeof(IMixedUnion))]
    public partial class MixedUnionMember2 : IMixedUnion
    {
        public string Id { get; set; } = default!;
        public DateTime Timestamp { get; set; }
    }

    // Test container classes
    [YamlObject]
    public partial class ContainerWithUnionMemberAttribute
    {
        public string Name { get; set; } = default!;
        public IUnionWithMemberAttribute Item { get; set; } = default!;
    }

    [YamlObject]
    public partial class ContainerWithAbstractUnionMemberAttribute
    {
        public int Code { get; set; }
        public AbstractUnionWithMemberAttribute Data { get; set; } = default!;
    }

    [YamlObject]
    public partial class ContainerWithMixedUnion
    {
        public string Title { get; set; } = default!;
        public IMixedUnion Content { get; set; } = default!;
    }
}