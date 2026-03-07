using NUnit.Framework;
using System;
using VYaml.Annotations;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;


namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class GeneratedFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize_NoMember()
        {
            Assert.That(Serialize(new SimpleTypeZero()), Is.EqualTo("{}"));
        }

        [Test]
        public void Serialize_PrimitiveMembers()
        {
            var result1 = Serialize(new SimpleTypeOne
            {
                One = 100
            });
            Assert.That(result1, Is.EqualTo("one: 100\n"));

            var result2 = Serialize(new SimpleTypeTwo
            {
                One = 100,
                Two = 200
            });
            Assert.That(result2, Is.EqualTo("one: 100\n" +
                                            "two: 200\n"));
        }

        [Test]
        public void Serialize_NamingConventionOptions()
        {
            var result = Serialize(new SimpleTypeTwo
            {
                One = 111,
                Two = 222
            }, new YamlSerializerOptions
            {
                Resolver = StandardResolver.Instance,
                NamingConvention = NamingConvention.UpperCamelCase
            });

            Assert.That(result, Is.EqualTo("One: 111\nTwo: 222\n"));
        }

        [Test]
        public void Serialize_Struct()
        {
            var result1 = Serialize(new SimpleUnmanagedStruct { MyProperty = 100 });
            Assert.That(result1, Is.EqualTo("myProperty: 100\n"));

            var result2 = Serialize(new SimpleStruct { MyProperty = "あいうえお" });
            Assert.That(result2, Is.EqualTo("myProperty: あいうえお\n"));
        }

        [Test]
        public void Serialize_ArrayMember()
        {
            var result1 = Serialize(new WithArray
            {
                One = new SimpleTypeOne[]
                {
                    new() { One = 111 },
                    new() { One = 222 },
                }
            });
            Assert.That(result1, Is.EqualTo("one:\n" +
                                           "- one: 111\n" +
                                           "- one: 222\n"));

            var result2 = Serialize(new WithArray());
            Assert.That(result2, Is.EqualTo("one: null\n"));
        }

        [Test]
        public void Serialize_IgnoreMember()
        {
            var result = Serialize(new WithIgnoreMember
            {
                A = 100,
                B = 200,
                C = 300
            });
            Assert.That(result, Is.EqualTo(
                "a: 100\n" +
                "c: 300\n"
                ));
        }

        [Test]
        public void Serialize_RenamedMember()
        {
            {
                var result = Serialize(new WithRenamedMember
                {
                    A = 100,
                    B = 200,
                    C = 300
                });
                Assert.That(result, Is.EqualTo(
                    "a: 100\n" +
                    "b-alias: 200\n" +
                    "c: 300\n"
                ));
            }
            {
                var result = Serialize(new WithRenamedMember
                {
                    A = 100,
                    B = 200,
                    C = 300
                }, new YamlSerializerOptions { NamingConvention = NamingConvention.UpperCamelCase });
                Assert.That(result, Is.EqualTo(
                    "A: 100\n" +
                    "b-alias: 200\n" +
                    "C: 300\n"
                ));
            }
        }

        [Test]
        public void Serialize_NamingConvention()
        {
            {
                var result = Serialize(new WithCustomNamingConvention
                {
                    HogeFuga = 123
                });

                Assert.That(result, Is.EqualTo(
                    "hoge_fuga: 123\n"
                ));
            }
            {
                var result = Serialize(new WithCustomNamingConvention
                {
                    HogeFuga = 123,
                }, new YamlSerializerOptions { NamingConvention = NamingConvention.UpperCamelCase });

                Assert.That(result, Is.EqualTo(
                    "hoge_fuga: 123\n"
                ));
            }
        }

        [Test]
        public void Serialize_InterfaceUnion()
        {
            var result1 = Serialize<IUnion>(new InterfaceImpl1
            {
                A = 100,
                B = "foo"
            });

            var result2 = Serialize<IUnion>(new InterfaceImpl2
            {
                A = 200,
                C = "bar"
            });

            Assert.That(result1, Is.EqualTo("!impl1\n" +
                                            "a: 100\n" +
                                            "b: foo\n"));

            Assert.That(result2, Is.EqualTo("!impl2\n" +
                                            "a: 200\n" +
                                            "c: bar\n"));
        }

        [Test]
        public void Serialize_AbstractUnion()
        {
            var result1 = Serialize<AbstractUnion>(new AbstractImpl1(100, "foo"));
            var result2 = Serialize<AbstractUnion>(new AbstractImpl2(200, "bar"));

            Assert.That(result1, Is.EqualTo("!impl1\n" +
                                            "a: 100\n" +
                                            "b: foo\n"));

            Assert.That(result2, Is.EqualTo("!impl2\n" +
                                            "a: 200\n" +
                                            "c: bar\n"));
        }

        [Test]
        public void Serialize_NestedUnion()
        {
            var result = Serialize(new WithUnionMember
            {
                A = 100,
                Union = new InterfaceImpl1
                {
                    A = 200,
                    B = "foo"
                }
            });
            Assert.That(result, Is.EqualTo("a: 100\n" +
                                           "union: !impl1\n" +
                                           "  a: 200\n" +
                                           "  b: foo\n"));
        }

        [Test]
        public void Serialize_NestedArrayUnion()
        {
            var result = Serialize(new WithArrayUnionMember
            {
                A = 100,
                Unions = new IUnion[]
                {
                    new InterfaceImpl1
                    {
                        A = 200,
                        B = "foo"
                    },
                    new InterfaceImpl2
                    {
                        A = 300,
                        C = "bar"
                    },
                }
            });
            Assert.That(result, Is.EqualTo("a: 100\n" +
                                           "unions:\n" +
                                           "- !impl1\n" +
                                           "  a: 200\n" +
                                           "  b: foo\n" +
                                           "- !impl2\n" +
                                           "  a: 300\n" +
                                           "  c: bar\n"));

        }

        public void Seralize_GenericType()
        {
            var result = Serialize(new GenericType<int>
            {
                Value = 100
            });
            Assert.That(result, Is.EqualTo("one: 100\n"));
            var result2 = Serialize(new GenericType<int, string>
            {
                Foo = 111,
                Bar = "aaa"
            });
            Assert.That(result2, Is.EqualTo("foo: 111\nbar: aaa\n"));
        }

        [Test]
        public void Deserialize_NoMember()
        {
            var result = Deserialize<SimpleTypeZero>("{}");
            Assert.That(result, Is.InstanceOf<SimpleTypeZero>());
        }

        [Test]
        public void Deserialize_PrimitiveMembers()
        {
            var result1 = Deserialize<SimpleTypeOne>("{ one: 100 }");
            Assert.That(result1.One, Is.EqualTo(100));

            var result2 = Deserialize<SimpleTypeTwo>("{ one: 100, two: 200 }");
            Assert.That(result2.One, Is.EqualTo(100));
            Assert.That(result2.Two, Is.EqualTo(200));
        }

        [Test]
        public void Deserialize_Struct()
        {
            var result1 = Deserialize<SimpleUnmanagedStruct>("{ myProperty: 100 }");
            Assert.That(result1.MyProperty, Is.EqualTo(100));

            var result2 = Deserialize<SimpleStruct>("{ myProperty: あいうえお }");
            Assert.That(result2.MyProperty, Is.EqualTo("あいうえお"));
        }

        [Test]
        public void Deserialize_ArrayMember()
        {
            var result1 = Deserialize<WithArray>("{ one: [{ one: 1 }, { one: 2 }] }");
            Assert.That(result1.One!.Length, Is.EqualTo(2));
        }

        [Test]
        public void Deserialize_TupleMember()
        {
            var result = Deserialize<WithTuple>(
                "one:   [111]\n" +
                "two:   [222, 333]\n" +
                "three: [444, 555, 666]\n" +
                "four:  [777, 888, 999, 111]\n" +
                "five:  [222, 333, 444, 555, 666]\n" +
                "six:   [777, 888, 999, 111, 222, 333]\n" +
                "seven: [444, 555, 666, 777, 888, 999, 111]");

            Assert.That(result.One, Is.EqualTo(new Tuple<int>(111)));
            Assert.That(result.Two, Is.EqualTo(new Tuple<int, int>(222, 333)));
            Assert.That(result.Three, Is.EqualTo(new Tuple<int, int, int>(444, 555, 666)));
            Assert.That(result.Four, Is.EqualTo(new Tuple<int, int, int, int>(777, 888, 999,111)));
            Assert.That(result.Five, Is.EqualTo(new Tuple<int, int, int, int, int>(222, 333, 444, 555, 666)));
            Assert.That(result.Six, Is.EqualTo(new Tuple<int, int, int, int, int, int>(777, 888, 999, 111, 222, 333)));
            Assert.That(result.Seven, Is.EqualTo(new Tuple<int, int, int, int, int, int, int>(444, 555, 666, 777, 888, 999, 111)));
        }

        [Test]
        public void Deserialize_ValueTupleMember()
        {
            var result = Deserialize<WithValueTuple>(
                "one:   [111]\n" +
                "two:   [222, 333]\n" +
                "three: [444, 555, 666]\n" +
                "four:  [777, 888, 999, 111]\n" +
                "five:  [222, 333, 444, 555, 666]\n" +
                "six:   [777, 888, 999, 111, 222, 333]\n" +
                "seven: [444, 555, 666, 777, 888, 999, 111]");

            Assert.That(result.One, Is.EqualTo(new ValueTuple<int>(111)));
            Assert.That(result.Two, Is.EqualTo(new ValueTuple<int, int>(222, 333)));
            Assert.That(result.Three, Is.EqualTo(new ValueTuple<int, int, int>(444, 555, 666)));
            Assert.That(result.Four, Is.EqualTo(new ValueTuple<int, int, int, int>(777, 888, 999,111)));
            Assert.That(result.Five, Is.EqualTo(new ValueTuple<int, int, int, int, int>(222, 333, 444, 555, 666)));
            Assert.That(result.Six, Is.EqualTo(new ValueTuple<int, int, int, int, int, int>(777, 888, 999, 111, 222, 333)));
            Assert.That(result.Seven, Is.EqualTo(new ValueTuple<int, int, int, int, int, int, int>(444, 555, 666, 777, 888, 999, 111)));
        }

        [Test]
        public void Deserialize_InterfaceUnion()
        {
            var result1 = Deserialize<IUnion>("!impl1 { a: 100, b: foo }");
            Assert.That(result1, Is.InstanceOf<InterfaceImpl1>());
            Assert.That(result1.A, Is.EqualTo(100));
            Assert.That(((InterfaceImpl1)result1).B, Is.EqualTo("foo"));

            var result2 = Deserialize<IUnion>("!impl2 { a: 100, c: bar }");
            Assert.That(result2, Is.InstanceOf<InterfaceImpl2>());
            Assert.That(result2.A, Is.EqualTo(100));
            Assert.That(((InterfaceImpl2)result2).C, Is.EqualTo("bar"));
        }

        [Test]
        public void Deserialize_AbstractUnion()
        {
            var result1 = Deserialize<AbstractUnion>("!impl1 { a: 100, b: foo }");
            Assert.That(result1, Is.InstanceOf<AbstractImpl1>());
            Assert.That(result1.A, Is.EqualTo(100));
            Assert.That(((AbstractImpl1)result1).B, Is.EqualTo("foo"));

            var result2 = Deserialize<AbstractUnion>("!impl2 { a: 100, c: bar }");
            Assert.That(result2, Is.InstanceOf<AbstractImpl2>());
            Assert.That(result2.A, Is.EqualTo(100));
            Assert.That(((AbstractImpl2)result2).C, Is.EqualTo("bar"));
        }

        [Test]
        public void SerializeDeserialize_Union()
        {
            var yaml = Serialize<IUnion>(new InterfaceImpl1
            {
                A = 100,
                B = "foo"
            });
            var result = Deserialize<IUnion>(yaml);

            Assert.That(result, Is.InstanceOf<InterfaceImpl1>());
            Assert.That(result.A, Is.EqualTo(100));
        }

        [Test]
        public void Deserialize_AnotherNamespace()
        {
            var result = Deserialize<VYaml.Tests.TypeDeclarations.More.StandardTypeTwo>("{ one: a, two: b }");
            Assert.That(result.One, Is.EqualTo("a"));
            Assert.That(result.Two, Is.EqualTo("b"));
        }

        [Test]
        public void Deserialize_GlobalNamespace()
        {
            var result = Deserialize<GlobalNamespaceType>("{ myProperty: 111 }");
            Assert.That(result.MyProperty, Is.EqualTo(111));
        }

        [Test]
        public void Deserialize_CustomConstructor()
        {
            var result1 = Deserialize<WithCustomConstructor>("{ foo: 111, bar: aaa }");
            Assert.That(result1.Foo, Is.EqualTo(111));
            Assert.That(result1.Bar, Is.EqualTo("aaa"));

            var result2 = Deserialize<WithCustomConstructor2>("{ foo: 111, bar: aaa }");
            Assert.That(result2.Foo, Is.EqualTo(111));
            Assert.That(result2.Bar, Is.EqualTo("aaa"));
        }

        [Test]
        public void Deserialize_CustomConstructorWithSetter()
        {
            var result = Deserialize<WithCustomConstructorAndOtherProps>("{ foo: 111, bar: aaa, hoge: bbb }");
            Assert.That(result.Foo, Is.EqualTo(111));
            Assert.That(result.Bar, Is.EqualTo("aaa"));
            Assert.That(result.Hoge, Is.EqualTo("bbb"));
        }

        [Test]
        public void Deserialize_CustomConstructorWithDefaultValue()
        {
            var result = Deserialize<WithCustomConstructor3>("{}");
            Assert.That(result.X, Is.EqualTo(100));
            Assert.That(result.Y, Is.EqualTo(222m));
            Assert.That(result.Z, Is.EqualTo(true));
        }

        [Test]
        public void Deserialize_UnsignedDefaultValues()
        {
            var result = Deserialize<WithUnsignedDefaultValues>("{}");
            Assert.That(result.UintValue, Is.EqualTo(123u));
            Assert.That(result.UlongValue, Is.EqualTo(456ul));
            Assert.That(result.UshortValue, Is.EqualTo(789));
            Assert.That(result.ByteValue, Is.EqualTo(255));
        }

        [Test]
        public void Deserialize_UnsignedDefaultValues_PartialOverride()
        {
            var result = Deserialize<WithUnsignedDefaultValues>("{ uintValue: 999 }");
            Assert.That(result.UintValue, Is.EqualTo(999u));
            Assert.That(result.UlongValue, Is.EqualTo(456ul));
            Assert.That(result.UshortValue, Is.EqualTo(789));
            Assert.That(result.ByteValue, Is.EqualTo(255));
        }

        [Test]
        public void Deserialize_CustomNamingConvention()
        {
            var result1 = Deserialize<WithCustomNamingConvention>("{ hoge_fuga: 123 }");
            Assert.That(result1.HogeFuga, Is.EqualTo(123));

            var result2 = Deserialize<WithCustomNamingConvention>("{ hoge_fuga: 123 }",
                new YamlSerializerOptions { NamingConvention = NamingConvention.UpperCamelCase });
            Assert.That(result2.HogeFuga, Is.EqualTo(123));
        }

        [Test]
        public void Deserialize_NamingConventionOptions()
        {
            var result1 = Deserialize<SimpleTypeTwo>("{ One: 123, Two: 456 }", new YamlSerializerOptions
            {
                Resolver = StandardResolver.Instance,
                NamingConvention = NamingConvention.UpperCamelCase
            });
            Assert.That(result1.One, Is.EqualTo(123));
            Assert.That(result1.Two, Is.EqualTo(456));

            var result2 = Deserialize<WithCustomNamingConvention>("{ hoge_fuga: 123 }",
                new YamlSerializerOptions { NamingConvention = NamingConvention.UpperCamelCase });
            Assert.That(result2.HogeFuga, Is.EqualTo(123));
        }

        [Test]
        public void Deserialize_RenamedMember()
        {
            var result1 = Deserialize<WithRenamedMember>("{ \"b-alias\": 123 }");
            Assert.That(result1.B, Is.EqualTo(123));

            var result2 = Deserialize<WithRenamedMember>("{ \"b-alias\": 123 }",
                new YamlSerializerOptions { NamingConvention = NamingConvention.UpperCamelCase });
            Assert.That(result2.B, Is.EqualTo(123));
        }

        public void Deserialize_GenericType()
        {
            var result = Deserialize<GenericType<int>>("{ one: 100 }");
            Assert.That(result.Value, Is.EqualTo(100));

            var result2 = Deserialize<GenericType<int, string>>("{ foo: 111, bar: aaa }");
            Assert.That(result2.Foo, Is.EqualTo(111));
            Assert.That(result2.Bar, Is.EqualTo("aaa"));
        }

    }
}