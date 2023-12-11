using VYaml.Annotations;

namespace VYaml.Tests.TypeDeclarations
{
    [YamlObject(NamingConvention.LowerCamelCase)]
    public enum LowerCamelEnum
    {
        HogeHoge,
        FugaFuga,
    }

    [YamlObject(NamingConvention.UpperCamelCase)]
    public enum UpperCamelEnum
    {
        HogeHoge,
        FugaFuga
    }

    [YamlObject(NamingConvention.SnakeCase)]
    public enum SnakeCaseEnum
    {
        HogeHoge,
        FugaFuga
    }

    [YamlObject(NamingConvention.KebabCase)]
    public enum KebabCaseEnum
    {
        HogeHoeg,
        FugaFuga
    }

    [YamlObject(NamingConvention.LowerCamelCase)]
    public partial class AsLowerCamel
    {
        public int Foo { get; set; }
        public int FooBar { get; set; }
        public LowerCamelEnum Buz { get; set; }
    }

    [YamlObject(NamingConvention.UpperCamelCase)]
    public partial class AsUpperCamel
    {
        public int Foo { get; set; }
        public int FooBar { get; set; }
        public UpperCamelEnum Buz { get; set; }
    }

    [YamlObject(NamingConvention.SnakeCase)]
    public partial class AsSnakeCase
    {
        public int Foo { get; set; }
        public int FooBar { get; set; }
        public SnakeCaseEnum Buz { get; set; }
    }

    [YamlObject(NamingConvention.KebabCase)]
    public partial class AsKebabCase
    {
        public int Foo { get; set; }
        public int FooBar { get; set; }
        public KebabCaseEnum Buz { get; set; }
    }
}
