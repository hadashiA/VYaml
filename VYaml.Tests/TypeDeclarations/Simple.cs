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
        public Tuple<int> One { get; set; } = default!;
        public Tuple<int, int> Two { get; set; } = default!;
        public Tuple<int, int, int> Three { get; set; } = default!;
        public Tuple<int, int, int, int> Four { get; set; } = default!;
        public Tuple<int, int, int, int, int> Five { get; set; } = default!;
        public Tuple<int, int, int, int, int, int> Six { get; set; } = default!;
        public Tuple<int, int, int, int, int, int, int> Seven { get; set; } = default!;
    }

    [YamlObject]
    public partial class WithValueTuple
    {
        public ValueTuple<int> One { get; set; }
        public ValueTuple<int, int> Two { get; set; }
        public ValueTuple<int, int, int> Three { get; set; }
        public ValueTuple<int, int, int, int> Four { get; set; }
        public ValueTuple<int, int, int, int, int> Five { get; set; }
        public ValueTuple<int, int, int, int, int, int> Six { get; set; }
        public ValueTuple<int, int, int, int, int, int, int> Seven { get; set; }
    }

    [YamlObject]
    public partial class WithDefaultValue
    {
        public readonly int Value;
        public readonly int ValueSet;

        [YamlConstructor]
        public WithDefaultValue(int valueSet, int value = 12)
        {
            ValueSet = valueSet;
            Value = value;
        }
    }

    public enum SimpleEnum
    {
        A,
        B,
        C,
    }

    [YamlObject(NamingConvention.SnakeCase)]
    public enum NamingConventionEnum
    {
        HogeFuga,
        BarGuz,
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

    [Flags, YamlObject]
    public enum EnumFlagMember
    {
        A = 1,
        B = 2,
        C = 3,
    }
    
    [YamlObject]
    [YamlObjectUnion("!impl1", typeof(InterfaceImpl1))]
    [YamlObjectUnion("!impl2", typeof(InterfaceImpl2))]
    public partial interface IUnion
    {
        public int A { get; }
    }

    [YamlObject]
    public partial class InterfaceImpl1 : IUnion
    {
        public int A { get; set; }
        public string B { get; set; } = default!;
    }

    [YamlObject]
    public partial class InterfaceImpl2 : IUnion
    {
        public int A { get; set; }
        public string C { get; set; } = default!;
    }

    [YamlObject]
    [YamlObjectUnion("!impl1", typeof(AbstractImpl1))]
    [YamlObjectUnion("!impl2", typeof(AbstractImpl2))]
    public abstract partial class AbstractUnion
    {
        public abstract int A { get; protected set; }
    }

    [YamlObject]
    public partial class AbstractImpl1 : AbstractUnion
    {
        public override int A { get; protected set; }
        public string B { get; private set; } = default!;

        public AbstractImpl1(int a, string b)
        {
            A = a;
            B = b;
        }
    }

    [YamlObject]
    public partial class AbstractImpl2 : AbstractUnion
    {
        public override int A { get; protected set; }
        public string C { get; private set; }

        public AbstractImpl2(int a, string c)
        {
            A = a;
            C = c;
        }
    }

    [YamlObject]
    public partial class WithUnionMember
    {
        public int A { get; set; }
        public IUnion Union { get; set; } = default!;
    }

    [YamlObject]
    public partial class WithArrayUnionMember
    {
        public int A { get; set; }
        public IUnion[] Unions { get; set; } = default!;
    }

    [YamlObject]
    public partial class WithIgnoreMember
    {
        public int A { get; set; }

        [YamlIgnore]
        public int B { get; set; }

        public int C { get; set; }
    }

    [YamlObject]
    public partial class WithRenamedMember
    {
        public int A { get; set; }

        [YamlMember("b-alias")]
        public int B { get; set; }

        public int C { get; set; }
    }

    [YamlObject]
    public partial class WithCustomConstructor
    {
        public int Foo { get; }
        public string Bar { get; }

        public WithCustomConstructor(int foo, string bar)
        {
            Foo = foo;
            Bar = bar;
        }
    }

    [YamlObject]
    public partial class WithCustomConstructor2
    {
        public int Foo { get; }
        public string Bar { get; }

        public WithCustomConstructor2()
        {
            Foo = default;
            Bar = default!;
        }

        [YamlConstructor]
        public WithCustomConstructor2(int foo, string bar)
        {
            Foo = foo;
            Bar = bar;
        }
    }

    [YamlObject]
    public partial record WithCustomConstructor3
    {
        public int X { get; }
        public decimal Y { get; }
        public bool Z { get; }

        [YamlConstructor]
        public WithCustomConstructor3(int x = 100, decimal y = 222m, bool z = true)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    [YamlObject]
    public partial class WithCustomConstructorAndOtherProps
    {
        public int Foo { get; }
        public string Bar { get; }
        public string Hoge { get; set; } = default!;

        [YamlConstructor]
        public WithCustomConstructorAndOtherProps(int foo, string bar)
        {
            Foo = foo;
            Bar = bar;
        }
    }

    [YamlObject(NamingConvention.SnakeCase)]
    public partial class WithCustomNamingConvention
    {
        public int HogeFuga { get; set; }
    }

    [YamlObject]
    public partial class WithUnsignedDefaultValues
    {
        public uint UintValue { get; }
        public ulong UlongValue { get; }
        public ushort UshortValue { get; }
        public byte ByteValue { get; }

        [YamlConstructor]
        public WithUnsignedDefaultValues(
            uint uintValue = 123u,
            ulong ulongValue = 456ul,
            ushort ushortValue = 789,
            byte byteValue = 255)
        {
            UintValue = uintValue;
            UlongValue = ulongValue;
            UshortValue = ushortValue;
            ByteValue = byteValue;
        }
    }

    [YamlObject]
    public partial class GenericType<T>
    {
        public T? Value { get; set; }
    }

    [YamlObject]
    public partial class GenericType<T, T2>
    {
        public T? Foo { get; set; }

        public T2? Bar { get; set; }
    }

    [YamlObject]
    public partial class WithNumericDefaultValues
    {
        public ushort Address { get; }
        public short ShortValue { get; }
        public sbyte SbyteValue { get; }
        public bool ReadOnly { get; }
        public string Name { get; }

        [YamlConstructor]
        public WithNumericDefaultValues(
            ushort address = 0,
            short shortValue = 0,
            sbyte sbyteValue = 0,
            bool readOnly = false,
            string name = "")
        {
            Address = address;
            ShortValue = shortValue;
            SbyteValue = sbyteValue;
            ReadOnly = readOnly;
            Name = name;
        }
    }
    [YamlObject]
    public partial class WithPrivateMembers
    {
        public int PublicField;

        [YamlMember("privateField")]
        int _privateField;

        [YamlMember]
        internal string InternalProperty { get; set; } = default!;

        public string PublicProperty { get; set; } = default!;

        [YamlConstructor]
        public WithPrivateMembers(int publicField, int privateField, string internalProperty, string publicProperty)
        {
            PublicField = publicField;
            _privateField = privateField;
            InternalProperty = internalProperty;
            PublicProperty = publicProperty;
        }

        public int GetPrivateField() => _privateField;
    }

    [YamlObject]
    public partial class WithPrivateMembersSettable
    {
        public int PublicValue { get; set; }

        [YamlMember("secret")]
        int _secret;

        public WithPrivateMembersSettable()
        {
        }

        public int GetSecret() => _secret;
    }

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
