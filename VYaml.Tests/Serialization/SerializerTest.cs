using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VYaml.Annotations;
using VYaml.Internal;
using VYaml.Parser;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    /// <summary>
    /// Represents a data structure used in YAML serialization and deserialization processes.
    /// </summary>
    /// <remarks>
    /// This class is annotated with the YamlObject attribute to ensure compatibility with YAML serialization mechanisms.
    /// It provides properties for testing the serialization and deserialization of simple objects with string and integer
    /// data types.
    /// </remarks>
    [YamlObject]
    internal partial class TestData
    {
        public string Name { get; init; } = "";
        public int Value { get; init; }
    }

    /// <summary>
    /// Represents a document for YAML serialization and deserialization testing.
    /// </summary>
    /// <remarks>
    /// This class contains shared and reference data, used to validate YAML serialization
    /// and deserialization processes in test scenarios. The class leverages the YamlObject
    /// attribute to specify its compatibility with YAML operations and supports different
    /// ways of preserving object references across multiple YAML documents.
    /// </remarks>
    [YamlObject]
    internal partial class TestDocument
    {
        public TestData Shared { get; set; } = new();
        public TestData Reference { get; set; } = new();
    }

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
        public void Deserialize_Empty()
        {
            var empty = new ReadOnlyMemory<byte>(Array.Empty<byte>());
            var result1 = YamlSerializer.Deserialize<dynamic>(empty);
            Assert.That(result1, Is.Null);
        }

        /// <summary>
        /// Deserializes multiple YAML documents and ensures that anchors defined in one document cannot be referenced by another.
        /// Verifies that anchor definitions and their references are properly isolated within each respective document.
        /// </summary>
        /// <remarks>
        /// The method processes a YAML input containing multiple documents where an anchor is defined in the first document
        /// and an attempt is made to reference it in subsequent documents. The test confirms that such cross-document
        /// references result in an error, as anchors are scoped to their containing document only.
        /// </remarks>
        /// <exception cref="YamlParserException">
        /// Thrown when invalid cross-document references to anchors are detected, ensuring compliance with YAML's
        /// document isolation rules regarding anchor scoping.
        /// </exception>
        [Test]
        public void DeserializeMultipleDocuments_AnchorsDoNotLeakBetweenDocuments()
        {
            const string yaml = """
                                ---
                                shared: &shared_value
                                  name: Document 1
                                  value: 100
                                reference: *shared_value
                                ---
                                # This should fail if anchors leak between documents
                                reference: *shared_value
                                """;

            var yamlBytes = Encoding.UTF8.GetBytes(yaml);
            
            // The deserialization should fail because the second document tries to reference
            // an anchor from the first document
            Assert.Throws<YamlParserException>(() =>
            {
                _ = YamlSerializer.DeserializeMultipleDocuments<Dictionary<string, object>>(yamlBytes).ToList();
            });
        }

        /// <summary>
        /// Deserializes a YAML input containing multiple documents, where the same anchor name can be reused within separate documents.
        /// Verifies that anchors are isolated to their respective documents and do not interfere with each other.
        /// </summary>
        /// <remarks>
        /// Each document contains shared data using anchors and ensures that references within a single document point to the correct objects.
        /// The test confirms that anchors with identical names in different documents do not share references and remain document-specific.
        /// </remarks>
        /// <returns>
        /// A collection of deserialized objects, where each entry represents a single YAML document. Each document's anchor-based references
        /// are validated for consistency and correctness within their isolated scopes.
        /// </returns>
        [Test]
        public void DeserializeMultipleDocuments_SameAnchorNameCanBeReusedInDifferentDocuments()
        {
            const string yaml = """
                                ---
                                data: &anchor
                                  document: 1
                                  content: First document
                                ref: *anchor
                                ---
                                data: &anchor
                                  document: 2
                                  content: Second document
                                ref: *anchor
                                ---
                                data: &anchor
                                  document: 3
                                  content: Third document
                                ref: *anchor
                                """;

            var yamlBytes = Encoding.UTF8.GetBytes(yaml);
            var documents = YamlSerializer.DeserializeMultipleDocuments<Dictionary<string, object>>(yamlBytes).ToList();
            
            Assert.That(documents.Count, Is.EqualTo(3));
            
            for (var i = 0; i < 3; i++)
            {
                var doc = documents[i];
                Assert.That(doc.ContainsKey("data"), Is.True);
                Assert.That(doc.ContainsKey("ref"), Is.True);
                
                var data = doc["data"] as Dictionary<object, object>;
                Assert.That(data, Is.Not.Null);
                Assert.That(data!["document"], Is.EqualTo(i + 1));
                Assert.That(data["content"], Is.EqualTo($"{i switch
                {
                    0 => "First",
                    1 => "Second",
                    _ => "Third"
                }} document"));
                
                var reference = doc["ref"] as Dictionary<object, object>;
                Assert.That(reference, Is.Not.Null);
                Assert.That(ReferenceEquals(data, reference), Is.True);
            }
        }

        /// <summary>
        /// Deserializes a YAML input containing multiple documents into a collection of complex objects, verifying the use of YAML anchors
        /// and references within individual documents.
        /// </summary>
        /// <returns>
        /// A collection of deserialized objects where each item represents a single YAML document. Anchors within each document
        /// are properly resolved, while anchors with the same name in different documents remain isolated.
        /// </returns>
        [Test]
        public void DeserializeMultipleDocuments_ComplexObjectsWithAnchors()
        {
            const string yaml = """
                                ---
                                defaults: &defaults
                                  host: localhost
                                  port: 8080
                                  timeout: 30
                                servers:
                                  - <<: *defaults
                                    name: Server A
                                  - <<: *defaults
                                    name: Server B
                                    port: 8081
                                ---
                                # New document - cannot reference previous anchors
                                servers:
                                  - name: Server C
                                    host: example.com
                                    port: 443
                                """;

            var yamlBytes = Encoding.UTF8.GetBytes(yaml);
            var documents = YamlSerializer.DeserializeMultipleDocuments<Dictionary<string, object>>(yamlBytes).ToList();
            
            Assert.That(documents.Count, Is.EqualTo(2));
            
            // First document
            var doc1 = documents[0];
            Assert.That(doc1.ContainsKey("defaults"), Is.True);
            Assert.That(doc1.ContainsKey("servers"), Is.True);
            
            var servers1 = doc1["servers"] as List<object>;
            Assert.That(servers1, Is.Not.Null);
            Assert.That(servers1!.Count, Is.EqualTo(2));
            
            var serverA = servers1[0] as Dictionary<object, object>;
            Assert.That(serverA!["name"], Is.EqualTo("Server A"));
            // Just verify the structure exists
            Assert.That(serverA.ContainsKey("<<"), Is.True);
            
            var serverB = servers1[1] as Dictionary<object, object>;
            Assert.That(serverB!["name"], Is.EqualTo("Server B"));
            Assert.That(serverB["port"], Is.EqualTo(8081)); // Override
            Assert.That(serverB.ContainsKey("<<"), Is.True);
            
            // Second document
            var doc2 = documents[1];
            Assert.That(doc2.ContainsKey("servers"), Is.True);
            
            var servers2 = doc2["servers"] as List<object>;
            Assert.That(servers2, Is.Not.Null);
            Assert.That(servers2!.Count, Is.EqualTo(1));
            
            var serverC = servers2[0] as Dictionary<object, object>;
            Assert.That(serverC["name"], Is.EqualTo("Server C"));
            Assert.That(serverC["host"], Is.EqualTo("example.com"));
            Assert.That(serverC["port"], Is.EqualTo(443));
        }

        /// <summary>
        /// Deserializes a YAML input containing multiple documents into a strongly-typed list of objects of the specified type.
        /// </summary>
        /// <remarks>
        /// This method processes multiple YAML documents concatenated in a single input while maintaining their individual
        /// document boundaries. It ensures shared anchors and references within each document are handled correctly and ensures
        /// those bindings do not leak between separate documents. Anchors with the same name in different documents
        /// are treated independently. Each document is deserialized into an object of the specified type.
        /// </remarks>
        /// <returns>
        /// An enumerable collection of deserialized objects where each item corresponds to a single YAML document
        /// in the input.
        /// </returns>
        [Test]
        public void DeserializeMultipleDocuments_StronglyTypedObjects()
        {
            const string yaml = """
                                ---
                                shared: &data
                                  name: Test Item
                                  value: 42
                                reference: *data
                                ---
                                shared:
                                  name: Another Item
                                  value: 99
                                reference:
                                  name: Different Reference
                                  value: 123
                                """;

            var yamlBytes = Encoding.UTF8.GetBytes(yaml);
            var documents = YamlSerializer.DeserializeMultipleDocuments<TestDocument>(yamlBytes).ToList();
            
            Assert.That(documents.Count, Is.EqualTo(2));
            
            // First document - shared and reference should be the same object
            var doc1 = documents[0];
            Assert.That(doc1.Shared.Name, Is.EqualTo("Test Item"));
            Assert.That(doc1.Shared.Value, Is.EqualTo(42));
            Assert.That(ReferenceEquals(doc1.Shared, doc1.Reference), Is.True);
            
            // Second document - shared and reference are different objects
            var doc2 = documents[1];
            Assert.That(doc2.Shared.Name, Is.EqualTo("Another Item"));
            Assert.That(doc2.Shared.Value, Is.EqualTo(99));
            Assert.That(doc2.Reference.Name, Is.EqualTo("Different Reference"));
            Assert.That(doc2.Reference.Value, Is.EqualTo(123));
            Assert.That(ReferenceEquals(doc2.Shared, doc2.Reference), Is.False);
        }
    }
}
