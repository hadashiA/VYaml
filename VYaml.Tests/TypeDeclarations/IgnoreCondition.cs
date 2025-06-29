using VYaml.Annotations;

namespace VYaml.Tests.TypeDeclarations
{
    [YamlObject]
    public partial class TypeWithNullableProperties
    {
        public string? NullableString { get; set; }
        public string NonNullableString { get; set; } = "default";
        public int? NullableInt { get; set; }
        public int NonNullableInt { get; set; }
        public bool BoolValue { get; set; }
        public double DoubleValue { get; set; }
        public SimpleTypeOne? NullableObject { get; set; }
        public SimpleTypeOne NonNullableObject { get; set; } = new SimpleTypeOne();
    }
}