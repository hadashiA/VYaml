using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;

namespace VYaml.Tests.Serialization
{
    /// <summary>
    /// Test type representing a child object for nested deserialization tests.
    /// </summary>
    [YamlObject]
    public partial class NestedTestChild
    {
        public string Id { get; set; } = "";
    }

    /// <summary>
    /// Test type representing a parent object that references a child by ID.
    /// </summary>
    [YamlObject]
    public partial class NestedTestParentWithChildId
    {
        public string Id { get; set; } = "";
        public string ChildId { get; set; } = "";
        public NestedTestChild? Child { get; set; }
    }

    /// <summary>
    /// Test type representing a parent object with a collection of children.
    /// </summary>
    [YamlObject]
    public partial class NestedTestParentWithChildren
    {
        public string Id { get; set; } = "";
        public List<NestedTestChild> Children { get; set; } = new();
    }

    /// <summary>
    /// Test type that tracks the number of nested deserialization calls.
    /// </summary>
    [YamlObject]
    public partial class NestedTestCountingObject
    {
        public int Count { get; set; }
        public int NestedCalls { get; set; }
    }

    /// <summary>
    /// Test type containing a YAML string that gets deserialized.
    /// </summary>
    [YamlObject]
    public partial class NestedTestDataWithYaml
    {
        public string Data { get; set; } = "";
        public int ParsedValue { get; set; }
    }

    /// <summary>
    /// Simple test type containing a single value.
    /// </summary>
    [YamlObject]
    public partial class NestedTestSimpleValue
    {
        public int Value { get; set; }
    }
    /// <summary>
    /// Tests for nested YAML deserialization within custom formatters.
    /// Verifies that the fix for issue #149 works correctly.
    /// </summary>
    [TestFixture]
    public class NestedYamlFormatterTest : FormatterTestBase
    {
        [Test]
        public void Deserialize_YamlInsideFormatter_ShouldNotThrowInvalidOperationException()
        {
            // Arrange
            var childrenYaml = @"
- id: A
- id: B
- id: C";
            var parentYaml = @"
- id: Parent1
  childId: A";

            var resolver = new NestedYamlFormatterResolver(childrenYaml);
            var options = new YamlSerializerOptions { Resolver = resolver };

            var parents = Deserialize<List<NestedTestParentWithChildId>>(parentYaml, options);
            
            Assert.Multiple(() =>
            {
                Assert.That(parents, Is.Not.Null);
                Assert.That(parents.Count, Is.EqualTo(1));
                Assert.That(parents[0].Id, Is.EqualTo("Parent1"));
                Assert.That(parents[0].Child, Is.Not.Null);
                Assert.That(parents[0].Child!.Id, Is.EqualTo("A"));
            });
        }

        [Test]
        public void Deserialize_MultipleNestedYamlCalls_ShouldWorkCorrectly()
        {
            // This test verifies that multiple YAML deserializations can happen
            // within a single formatter without corrupting the parser state
            var resolver = new CountingFormatterResolver();
            var options = new YamlSerializerOptions { Resolver = resolver };

            var result = Deserialize<NestedTestCountingObject>("count: 3", options);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count, Is.EqualTo(3));
                Assert.That(result.NestedCalls, Is.EqualTo(3));
            });
        }

        [Test]
        public void Deserialize_DeeplyNestedFormatters_ShouldHandleRecursion()
        {
            // This tests that the fix works even with deeply nested parsing
            var resolver = new SimpleNestedFormatterResolver();
            var options = new YamlSerializerOptions { Resolver = resolver };

            var data = Deserialize<NestedTestDataWithYaml>("data: 'value: 42'", options);

            Assert.Multiple(() =>
            {
                Assert.That(data, Is.Not.Null);
                Assert.That(data.Data, Is.EqualTo("value: 42"));
                Assert.That(data.ParsedValue, Is.EqualTo(42));
            });
        }

        // Test types are defined at namespace level due to VYaml source generator requirements

        // Custom resolvers and formatters
        class CountingFormatterResolver : IYamlFormatterResolver
        {
            public IYamlFormatter<T>? GetFormatter<T>()
            {
                if (typeof(T) == typeof(NestedTestCountingObject))
                    return (IYamlFormatter<T>?)new CountingFormatter();
                return StandardResolver.Instance.GetFormatter<T>();
            }
        }

        class CountingFormatter : IYamlFormatter<NestedTestCountingObject>
        {
            public void Serialize(ref Utf8YamlEmitter emitter, NestedTestCountingObject value, YamlSerializationContext context)
            {
                throw new NotImplementedException("Test only deserializes");
            }

            public NestedTestCountingObject Deserialize(ref YamlParser parser, YamlDeserializationContext context)
            {
                var obj = new NestedTestCountingObject();
                
                parser.ReadWithVerify(ParseEventType.MappingStart);
                while (parser.CurrentEventType != ParseEventType.MappingEnd)
                {
                    var key = parser.ReadScalarAsString();
                    switch (key)
                    {
                        case "count":
                            obj.Count = parser.ReadScalarAsInt32();
                            // Make nested deserialization calls
                            for (int i = 0; i < obj.Count; i++)
                            {
                                var dummyYaml = "id: dummy" + i;
                                var bytes = System.Text.Encoding.UTF8.GetBytes(dummyYaml);
                                var dummy = YamlSerializer.Deserialize<NestedTestChild>(bytes);
                                if (dummy != null)
                                {
                                    obj.NestedCalls++;
                                }
                            }
                            break;
                        default:
                            parser.SkipCurrentNode();
                            break;
                    }
                }
                parser.ReadWithVerify(ParseEventType.MappingEnd);
                
                return obj;
            }
        }

        class SimpleNestedFormatterResolver : IYamlFormatterResolver
        {
            public IYamlFormatter<T>? GetFormatter<T>()
            {
                if (typeof(T) == typeof(NestedTestDataWithYaml))
                    return (IYamlFormatter<T>?)new DataWithYamlFormatter();
                return StandardResolver.Instance.GetFormatter<T>();
            }
        }

        class DataWithYamlFormatter : IYamlFormatter<NestedTestDataWithYaml>
        {
            public void Serialize(ref Utf8YamlEmitter emitter, NestedTestDataWithYaml value, YamlSerializationContext context)
            {
                throw new NotImplementedException("Test only deserializes");
            }

            public NestedTestDataWithYaml Deserialize(ref YamlParser parser, YamlDeserializationContext context)
            {
                var data = new NestedTestDataWithYaml();
                
                parser.ReadWithVerify(ParseEventType.MappingStart);
                while (parser.CurrentEventType != ParseEventType.MappingEnd)
                {
                    var key = parser.ReadScalarAsString();
                    switch (key)
                    {
                        case "data":
                            data.Data = parser.ReadScalarAsString() ?? "";
                            // Parse the YAML string to extract the value
                            if (!string.IsNullOrEmpty(data.Data))
                            {
                                var yamlBytes = System.Text.Encoding.UTF8.GetBytes(data.Data);
                                var parsed = YamlSerializer.Deserialize<NestedTestSimpleValue>(yamlBytes);
                                data.ParsedValue = parsed?.Value ?? 0;
                            }
                            break;
                        default:
                            parser.SkipCurrentNode();
                            break;
                    }
                }
                parser.ReadWithVerify(ParseEventType.MappingEnd);
                
                return data;
            }
        }

        class NestedYamlFormatterResolver : IYamlFormatterResolver
        {
            private readonly string childrenYaml;

            public NestedYamlFormatterResolver(string childrenYaml)
            {
                this.childrenYaml = childrenYaml;
            }

            public IYamlFormatter<T>? GetFormatter<T>()
            {
                if (typeof(T) == typeof(NestedTestParentWithChildId))
                    return (IYamlFormatter<T>?)new ParentWithChildIdFormatter(childrenYaml);
                return StandardResolver.Instance.GetFormatter<T>();
            }
        }

        class ParentWithChildIdFormatter : IYamlFormatter<NestedTestParentWithChildId>
        {
            private readonly string childrenYaml;

            public ParentWithChildIdFormatter(string childrenYaml)
            {
                this.childrenYaml = childrenYaml;
            }

            public void Serialize(ref Utf8YamlEmitter emitter, NestedTestParentWithChildId value, YamlSerializationContext context)
            {
                throw new NotImplementedException("Test only deserializes");
            }

            public NestedTestParentWithChildId Deserialize(ref YamlParser parser, YamlDeserializationContext context)
            {
                var parent = new NestedTestParentWithChildId();
                
                parser.ReadWithVerify(ParseEventType.MappingStart);
                while (parser.CurrentEventType != ParseEventType.MappingEnd)
                {
                    var key = parser.ReadScalarAsString();
                    switch (key)
                    {
                        case "id":
                            parent.Id = parser.ReadScalarAsString() ?? "";
                            break;
                        case "childId":
                            var childId = parser.ReadScalarAsString() ?? "";
                            parent.ChildId = childId;
                            
                            // This is where the nested YAML deserialization happens
                            var childrenBytes = System.Text.Encoding.UTF8.GetBytes(childrenYaml);
                            var children = YamlSerializer.Deserialize<List<NestedTestChild>>(childrenBytes);
                            parent.Child = children.FirstOrDefault(c => c.Id == childId);
                            break;
                        default:
                            parser.SkipCurrentNode();
                            break;
                    }
                }
                parser.ReadWithVerify(ParseEventType.MappingEnd);
                
                return parent;
            }
        }

    }
}