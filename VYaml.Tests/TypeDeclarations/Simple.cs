using VYaml.Serialization;

namespace VYaml.Tests.TypeDeclarations
{
    [YamlObject]
    public partial class SimpleTypeZero
    {
    }

    [YamlObject]
    public partial class SimpleTypeOne
    {
        public int One { get; set; }
    }


    [YamlObject]
    public partial class SimpleTypeTwo
    {
        public int One { get; set; }
        public int Two { get; set; }
    }

    [YamlObject]
    public partial struct SimpleUnmanagedStruct
    {
        public int MyProperty { get; set; }
    }

    [YamlObject]
    public partial struct SimpleStruct
    {
        public string MyProperty { get; set; }
    }

    [YamlObject]
    public partial class WithArray
    {
        public SimpleTypeOne[]? One { get; set; }
    }
}

// another namespace, same type name
namespace VYaml.Tests.TypeDeclarations.More
{
    [YamlObject]
    public partial class StandardTypeTwo
    {
        public string? One { get; set; }
        public string? Two { get; set; }

        public StandardTypeTwo()
        {
            // new StandardTypeTwoFormatter();
        }
    }
}

[YamlObject]
public partial class GlobalNamespaceType
{
    public int MyProperty { get; set; }

    public GlobalNamespaceType()
    {
        // _ = new GlobalNamespaceTypeFormatter();
    }
}