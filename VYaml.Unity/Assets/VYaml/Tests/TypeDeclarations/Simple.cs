using System;
using System.Runtime.Serialization;
using VYaml.Annotations;

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

    [YamlObject]
    public partial class WithTuple
    {
        public Tuple<int> One { get; set; }
        public Tuple<int, int> Two { get; set; }
        public Tuple<int, int, int> Three { get; set; }
    }

    [YamlObject]
    public partial class WithValueTuple
    {
        public ValueTuple<int> One { get; set; }
        public ValueTuple<int, int> Two { get; set; }
        public ValueTuple<int, int, int> Three { get; set; }
    }

    public enum SimpleEnum
    {
        A,
        B,
        C,
    }

    public enum EnumMemberLabeledEnum
    {
        [EnumMember(Value = "a-alias")]
        A,

        [EnumMember(Value = "b-alias")]
        B,

        [EnumMember(Value = "c-alias")]
        C,
    }

    public enum DataMemberLabeledEnum
    {
        [DataMember(Name = "a-alias")]
        A,

        [DataMember(Name = "b-alias")]
        B,

        [DataMember(Name = "c-alias")]
        C,
    }

    public enum YamlMemberLabeledEnum
    {
        [YamlMember("a-alias")]
        A,

        [YamlMember("b-alias")]
        B,

        [YamlMember("c-alias")]
        C,
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