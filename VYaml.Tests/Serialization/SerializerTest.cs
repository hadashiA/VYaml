using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VYaml.Emitter;
using VYaml.Internal;
using VYaml.Parser;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class SerializerTest
    {
        [Test]
        public void Serialize_LowerCamel()
        {
            var result = YamlSerializer.SerializeToString(new AsLowerCamel
            {
                Foo = 111,
                FooBar = 222,
                Buz = LowerCamelEnum.FugaFuga
            });
            Assert.That(result, Is.EqualTo("foo: 111\n" +
                                           "fooBar: 222\n" +
                                           "buz: fugaFuga\n"));
        }

        [Test]
        public void Serialize_UpperCamel()
        {
            var result = YamlSerializer.SerializeToString(new AsUpperCamel
            {
                Foo = 111,
                FooBar = 222,
                Buz = UpperCamelEnum.FugaFuga
            });
            Assert.That(result, Is.EqualTo("Foo: 111\n" +
                                           "FooBar: 222\n" +
                                           "Buz: FugaFuga\n"));
        }

        [Test]
        public void Serialize_SnakeCase()
        {
            var result = YamlSerializer.SerializeToString(new AsSnakeCase
            {
                Foo = 111,
                FooBar = 222,
                Buz = SnakeCaseEnum.FugaFuga
            });
            Assert.That(result, Is.EqualTo("foo: 111\n" +
                                           "foo_bar: 222\n" +
                                           "buz: fuga_fuga\n"));
        }

        [Test]
        public void Serialize_KebabCase()
        {
            var result = YamlSerializer.SerializeToString(new AsKebabCase
            {
                Foo = 111,
                FooBar = 222,
                Buz = KebabCaseEnum.FugaFuga
            });
            Assert.That(result, Is.EqualTo("foo: 111\n" +
                                           "foo-bar: 222\n" +
                                           "buz: fuga-fuga\n"));
        }

        [Test]
        public void Deserialize_LowerCamel()
        {
            var result = YamlSerializer.Deserialize<AsLowerCamel>(
                StringEncoding.Utf8.GetBytes("foo: 111\n" +
                                             "fooBar: 222\n" +
                                             "buz: fugaFuga\n"));

            Assert.That(result.Foo, Is.EqualTo(111));
            Assert.That(result.FooBar, Is.EqualTo(222));
            Assert.That(result.Buz, Is.EqualTo(LowerCamelEnum.FugaFuga));
        }

        [Test]
        public void Deserialize_UpperCamel()
        {
            var result = YamlSerializer.Deserialize<AsUpperCamel>(
                StringEncoding.Utf8.GetBytes("Foo: 111\n" +
                                             "FooBar: 222\n" +
                                             "Buz: FugaFuga\n"));

            Assert.That(result.Foo, Is.EqualTo(111));
            Assert.That(result.FooBar, Is.EqualTo(222));
            Assert.That(result.Buz, Is.EqualTo(UpperCamelEnum.FugaFuga));
        }

        [Test]
        public void Deserialize_SnakeCase()
        {
            var result = YamlSerializer.Deserialize<AsSnakeCase>(
                StringEncoding.Utf8.GetBytes("foo: 111\n" +
                                             "foo_bar: 222\n" +
                                             "buz: fuga_fuga\n"));

            Assert.That(result.Foo, Is.EqualTo(111));
            Assert.That(result.FooBar, Is.EqualTo(222));
            Assert.That(result.Buz, Is.EqualTo(SnakeCaseEnum.FugaFuga));
        }

        [Test]
        public void Deserialize_KebabCase()
        {
            var result = YamlSerializer.Deserialize<AsKebabCase>(
                StringEncoding.Utf8.GetBytes("foo: 111\n" +
                                             "foo-bar: 222\n" +
                                             "buz: fuga-fuga\n"));

            Assert.That(result.Foo, Is.EqualTo(111));
            Assert.That(result.FooBar, Is.EqualTo(222));
            Assert.That(result.Buz, Is.EqualTo(KebabCaseEnum.FugaFuga));
        }

        [Test]
        public void Deserialize_ExplicitDefaultValueFromConstructor()
        {
            var yamlBytes = StringEncoding.Utf8.GetBytes("valueSet: 22");
            var value = YamlSerializer.Deserialize<WithDefaultValue>(yamlBytes);
            Assert.That(value.Value, Is.EqualTo(12));
            Assert.That(value.ValueSet, Is.EqualTo(22));
        }

        [Test]
        public void DeserializeMultipleDocuments()
        {
            var yamlBytes = StringEncoding.Utf8.GetBytes(SpecExamples.Ex2_28);
            var documents = YamlSerializer.DeserializeMultipleDocuments<dynamic>(yamlBytes).ToArray();
            Assert.That(documents.Length, Is.EqualTo(3));
            Assert.That(documents[0]["Warning"], Is.EqualTo("This is an error message for the log file"));
            Assert.That(documents[1]["Warning"], Is.EqualTo("A slightly different error message."));
            Assert.That(documents[2]["Fatal"], Is.EqualTo("Unknown variable \"bar\""));
        }

        [Test]
        public void DeserializeHighDigitNumber()
        {
            var yamlBytes = StringEncoding.Utf8.GetBytes("id1: 8083928222794209684\n" +
                                                         "id2: 123\n" +
                                                         "id3: 8083928222794209684.123456789\n");
            var documents = YamlSerializer.Deserialize<dynamic>(yamlBytes);
            Assert.That(documents.Count, Is.EqualTo(3));
            Assert.That(documents["id1"], Is.InstanceOf<long>());
            Assert.That(documents["id1"], Is.EqualTo(8083928222794209684));
            Assert.That(documents["id2"], Is.InstanceOf<int>());
            Assert.That(documents["id3"], Is.InstanceOf<double>());
        }

        [Test]
        public void SerializeDeserialize_NativeInt()
        {
            var yaml = YamlSerializer.SerializeToString<nint>(42);
            Assert.That(yaml, Is.EqualTo("42"));

            var result = YamlSerializer.Deserialize<nint>(StringEncoding.Utf8.GetBytes("42"));
            Assert.That(result, Is.EqualTo((nint)42));
        }

        [Test]
        public void SerializeDeserialize_NativeUInt()
        {
            var yaml = YamlSerializer.SerializeToString<nuint>(42);
            Assert.That(yaml, Is.EqualTo("42"));

            var result = YamlSerializer.Deserialize<nuint>(StringEncoding.Utf8.GetBytes("42"));
            Assert.That(result, Is.EqualTo((nuint)42));
        }

        [Test]
        public void Serialize_LongString()
        {
            var longString = new string('a', 3000);
            var result = YamlSerializer.SerializeToString(longString);
            Assert.That(result, Does.Contain(longString));
        }

        [Test]
        public void Deserialize_Empty()
        {
            var empty = new ReadOnlyMemory<byte>(Array.Empty<byte>());
            var result1 = YamlSerializer.Deserialize<dynamic>(empty);
            Assert.That(result1, Is.Null);
        }

        [Test]
        public void Deserialize_NestedYamlInsideFormatter_DoesNotCorruptOuterState()
        {
            var result = YamlSerializer.Deserialize<List<NestedParent>>(NestedYaml.Parents, new YamlSerializerOptions
            {
                Resolver = new NestedYamlFormatterResolver(),
            });

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo("A"));
            Assert.That(result[0].Child!.Id, Is.EqualTo("A"));
            Assert.That(result[0].Other!.Id, Is.EqualTo("B"));
        }

        sealed class NestedParent
        {
            public string? Id { get; init; }
            public NestedChild? Child { get; init; }
            public NestedChild? Other { get; init; }
        }

        sealed class NestedChild
        {
            public NestedChild(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }

        static class NestedYaml
        {
            public static readonly byte[] Parents = """
                - id: A
                  child: A
                  other: B
                """u8.ToArray();

            public static readonly byte[] Children = """
                - id: A
                - id: B
                - id: C
                """u8.ToArray();
        }

        sealed class NestedYamlFormatterResolver : IYamlFormatterResolver
        {
            public IYamlFormatter<T>? GetFormatter<T>()
            {
                if (typeof(T) == typeof(NestedParent))
                {
                    return (IYamlFormatter<T>)(object)new NestedParentYamlFormatter();
                }

                if (typeof(T) == typeof(NestedChild))
                {
                    return (IYamlFormatter<T>)(object)new NestedChildYamlFormatter();
                }

                return StandardResolver.Instance.GetFormatter<T>();
            }
        }

        sealed class NestedParentYamlFormatter : IYamlFormatter<NestedParent>
        {
            public void Serialize(ref Utf8YamlEmitter emitter, NestedParent value, YamlSerializationContext context)
            {
                throw new NotImplementedException();
            }

            public NestedParent Deserialize(ref YamlParser parser, YamlDeserializationContext context)
            {
                string? id = null;
                NestedChild? child = null;
                NestedChild? other = null;

                parser.ReadWithVerify(ParseEventType.MappingStart);
                while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
                {
                    var key = parser.ReadScalarAsString();
                    switch (key)
                    {
                        case "id":
                            id = parser.ReadScalarAsString();
                            break;
                        case "child":
                            child = context.DeserializeWithAlias<NestedChild>(ref parser);
                            break;
                        case "other":
                            other = context.DeserializeWithAlias<NestedChild>(ref parser);
                            break;
                        default:
                            throw new YamlSerializerException($"Unexpected key: {key}");
                    }
                }
                parser.ReadWithVerify(ParseEventType.MappingEnd);

                return new NestedParent { Id = id, Child = child, Other = other };
            }
        }

        sealed class NestedChildYamlFormatter : IYamlFormatter<NestedChild>
        {
            public void Serialize(ref Utf8YamlEmitter emitter, NestedChild value, YamlSerializationContext context)
            {
                throw new NotImplementedException();
            }

            public NestedChild Deserialize(ref YamlParser parser, YamlDeserializationContext context)
            {
                var childId = parser.ReadScalarAsString();
                var children = YamlSerializer.Deserialize<List<Dictionary<string, string>>>(NestedYaml.Children);
                return new NestedChild(children.First(x => x["id"] == childId)["id"]);
            }
        }
    }
}
