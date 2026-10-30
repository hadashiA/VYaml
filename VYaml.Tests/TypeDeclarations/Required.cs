using VYaml.Annotations;

namespace VYaml.Tests.TypeDeclarations
{
    [YamlObject]
    public partial class WithRequiredModifier
    {
        public required string Name { get; set; }
        public required int Count;
        public string? Comment { get; set; }
    }

    [YamlObject]
    public partial class WithRequiredAttribute
    {
        [YamlRequired]
        public string? Name { get; set; }
        [YamlRequired]
        [YamlMember("max_count")]
        public int MaxCount { get; set; }
        public int Optional { get; set; }
    }

    [YamlObject]
    public partial record RequiredRecord([property: YamlRequired] string Id, int Value = 0);

    [YamlObject]
    public partial struct RequiredStruct
    {
        [YamlRequired]
        public int X;
    }

    [YamlObject]
    [YamlObjectUnion(typeof(RequiredActionStep), RequiredKey = "uses")]
    public partial interface IRequiredStep
    {
    }

    [YamlObject]
    public partial class RequiredActionStep : IRequiredStep
    {
        public required string Uses { get; set; }
        [YamlRequired]
        public string? Name { get; set; }
    }
}
