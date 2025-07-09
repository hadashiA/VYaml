using System;
using NUnit.Framework;
using VYaml.Internal;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class UnionMemberAttributeTest
    {
        [Test]
        public void Serialize_InterfaceWithUnionMemberAttribute()
        {
            var container = new ContainerWithUnionMemberAttribute
            {
                Name = "Test Container",
                Item = new UnionMember1 { Value = 42, Name = "First" }
            };

            var utf8Yaml = YamlSerializer.SerializeToString(container);

            Assert.Multiple(() =>
            {
                Assert.That(utf8Yaml, Does.Contain("name: Test Container"));
                Assert.That(utf8Yaml, Does.Contain("!member1"));
                Assert.That(utf8Yaml, Does.Contain("value: 42"));
                Assert.That(utf8Yaml, Does.Contain("name: First"));
            });
        }

        [Test]
        public void Deserialize_InterfaceWithUnionMemberAttribute()
        {
            var yaml = @"name: Test Container
item: !member1
  value: 42
  name: First";

            var deserialized =
                YamlSerializer.Deserialize<ContainerWithUnionMemberAttribute>(StringEncoding.Utf8.GetBytes(yaml));

            Assert.Multiple(() =>
            {
                Assert.That(deserialized, Is.Not.Null);
                Assert.That(deserialized.Name, Is.EqualTo("Test Container"));
                Assert.That(deserialized.Item, Is.TypeOf<UnionMember1>());
                var item1 = (UnionMember1)deserialized.Item;
                Assert.That(item1.Value, Is.EqualTo(42));
                Assert.That(item1.Name, Is.EqualTo("First"));
            });
        }

        [Test]
        public void Serialize_AbstractClassWithUnionMemberAttribute()
        {
            var container = new ContainerWithAbstractUnionMemberAttribute
            {
                Code = 100,
                Data = new ConcreteUnionMember2 { IsActive = true }
            };

            var utf8Yaml = YamlSerializer.SerializeToString(container);

            Assert.Multiple(() =>
            {
                Assert.That(utf8Yaml, Does.Contain("code: 100"));
                Assert.That(utf8Yaml, Does.Contain("!concrete2"));
                Assert.That(utf8Yaml, Does.Contain("isActive: true"));
            });
        }

        [Test]
        public void Deserialize_AbstractClassWithUnionMemberAttribute()
        {
            var yaml = @"code: 100
data: !concrete2
  isActive: true";

            var deserialized =
                YamlSerializer.Deserialize<ContainerWithAbstractUnionMemberAttribute>(
                    StringEncoding.Utf8.GetBytes(yaml));

            Assert.Multiple(() =>
            {
                Assert.That(deserialized, Is.Not.Null);
                Assert.That(deserialized.Code, Is.EqualTo(100));
                Assert.That(deserialized.Data, Is.TypeOf<ConcreteUnionMember2>());
                var data2 = (ConcreteUnionMember2)deserialized.Data;
                Assert.That(data2.IsActive, Is.True);
                Assert.That(data2.Type, Is.Null); // Type is not set in yaml
            });
        }

        [Test]
        public void Serialize_Mixed_YamlObjectUnionAttribute()
        {
            var container1 = new ContainerWithMixedUnion
            {
                Title = "Mixed Test 1",
                Content = new MixedUnionMember1 { Id = "id-001", Version = 5 }
            };

            var yaml1 = YamlSerializer.SerializeToString(container1);

            Assert.That(yaml1, Does.Contain("!mixed1"));
        }

        [Test]
        public void Serialize_Mixed_YamlUnionMemberAttribute()
        {
            var container2 = new ContainerWithMixedUnion
            {
                Title = "Mixed Test 2",
                Content = new MixedUnionMember2 { Id = "id-002", Timestamp = new DateTime(2024, 1, 1) }
            };

            var yaml2 = YamlSerializer.SerializeToString(container2);

            Assert.That(yaml2, Does.Contain("!mixed2"));
        }

        [Test]
        public void Deserialize_Mixed_YamlObjectUnionAttribute()
        {
            var yaml1 = @"title: Mixed Test 1
content: !mixed1
  id: id-001
  version: 5";

            var deserialized1 =
                YamlSerializer.Deserialize<ContainerWithMixedUnion>(StringEncoding.Utf8.GetBytes(yaml1));

            Assert.Multiple(() =>
            {
                Assert.That(deserialized1.Content, Is.TypeOf<MixedUnionMember1>());
                Assert.That(((MixedUnionMember1)deserialized1.Content).Version, Is.EqualTo(5));
            });
        }

        [Test]
        public void Deserialize_Mixed_YamlUnionMemberAttribute()
        {
            var yaml2 = @"title: Mixed Test 2
content: !mixed2
  id: id-002
  timestamp: 2024-01-01T00:00:00";

            var deserialized2 =
                YamlSerializer.Deserialize<ContainerWithMixedUnion>(StringEncoding.Utf8.GetBytes(yaml2));

            Assert.Multiple(() =>
            {
                Assert.That(deserialized2.Content, Is.TypeOf<MixedUnionMember2>());
                Assert.That(((MixedUnionMember2)deserialized2.Content).Timestamp, Is.EqualTo(new DateTime(2024, 1, 1)));
            });
        }

        [Test]
        public void Serialize_MultipleUnionMembers_YamlObjectUnionAttribute()
        {
            var container1 = new ContainerWithUnionMemberAttribute
            {
                Name = "Container 1",
                Item = new UnionMember1 { Value = 10, Name = "Item One" }
            };

            var yaml1 = YamlSerializer.SerializeToString(container1);

            Assert.Multiple(() =>
            {
                Assert.That(yaml1, Does.Contain("name: Container 1"));
                Assert.That(yaml1, Does.Contain("!member1"));
                Assert.That(yaml1, Does.Contain("value: 10"));
                Assert.That(yaml1, Does.Contain("name: Item One"));
            });
        }

        [Test]
        public void Serialize_MultipleUnionMembers_YamlUnionMemberAttribute()
        {
            var container2 = new ContainerWithUnionMemberAttribute
            {
                Name = "Container 2",
                Item = new UnionMember2 { Value = 20, Price = 99.99m }
            };

            var yaml2 = YamlSerializer.SerializeToString(container2);

            Assert.Multiple(() =>
            {
                Assert.That(yaml2, Does.Contain("name: Container 2"));
                Assert.That(yaml2, Does.Contain("!member2"));
                Assert.That(yaml2, Does.Contain("value: 20"));
                Assert.That(yaml2, Does.Contain("price: 99.99"));
            });
        }

        [Test]
        public void Deserialize_MultipleUnionMembers_YamlObjectUnionAttribute()
        {
            var yaml1 = @"name: Container 1
item: !member1
  value: 10
  name: Item One";

            var container1 =
                YamlSerializer.Deserialize<ContainerWithUnionMemberAttribute>(StringEncoding.Utf8.GetBytes(yaml1));

            Assert.Multiple(() =>
            {
                Assert.That(container1.Item, Is.TypeOf<UnionMember1>());
                Assert.That(((UnionMember1)container1.Item).Name, Is.EqualTo("Item One"));
            });
        }

        [Test]
        public void Deserialize_MultipleUnionMembers_YamlUnionMemberAttribute()
        {
            var yaml2 = @"name: Container 2
item: !member2
  value: 20
  price: 99.99";

            var container2 =
                YamlSerializer.Deserialize<ContainerWithUnionMemberAttribute>(StringEncoding.Utf8.GetBytes(yaml2));

            Assert.Multiple(() =>
            {
                Assert.That(container2.Item, Is.TypeOf<UnionMember2>());
                Assert.That(((UnionMember2)container2.Item).Price, Is.EqualTo(99.99m));
            });
        }
    }
}