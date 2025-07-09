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
        public void SerializeDeserialize_InterfaceWithUnionMemberAttribute()
        {
            var container = new ContainerWithUnionMemberAttribute
            {
                Name = "Test Container",
                Item = new UnionMember1 { Value = 42, Name = "First" }
            };

            var utf8Yaml = YamlSerializer.SerializeToString(container);

            Assert.That(utf8Yaml, Does.Contain("name: Test Container"));
            Assert.That(utf8Yaml, Does.Contain("!member1"));
            Assert.That(utf8Yaml, Does.Contain("value: 42"));
            Assert.That(utf8Yaml, Does.Contain("name: First"));

            var deserialized =
                YamlSerializer.Deserialize<ContainerWithUnionMemberAttribute>(StringEncoding.Utf8.GetBytes(utf8Yaml));

            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized.Name, Is.EqualTo("Test Container"));
            Assert.That(deserialized.Item, Is.TypeOf<UnionMember1>());
            var item1 = (UnionMember1)deserialized.Item;
            Assert.That(item1.Value, Is.EqualTo(42));
            Assert.That(item1.Name, Is.EqualTo("First"));
        }

        [Test]
        public void SerializeDeserialize_AbstractClassWithUnionMemberAttribute()
        {
            var container = new ContainerWithAbstractUnionMemberAttribute
            {
                Code = 100,
                Data = new ConcreteUnionMember2 { IsActive = true }
            };

            var utf8Yaml = YamlSerializer.SerializeToString(container);

            Assert.That(utf8Yaml, Does.Contain("code: 100"));
            Assert.That(utf8Yaml, Does.Contain("!concrete2"));
            Assert.That(utf8Yaml, Does.Contain("isActive: true"));

            var deserialized =
                YamlSerializer.Deserialize<ContainerWithAbstractUnionMemberAttribute>(
                    StringEncoding.Utf8.GetBytes(utf8Yaml));

            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized.Code, Is.EqualTo(100));
            Assert.That(deserialized.Data, Is.TypeOf<ConcreteUnionMember2>());
            var data2 = (ConcreteUnionMember2)deserialized.Data;
            Assert.That(data2.IsActive, Is.True);
            Assert.That(data2.Type, Is.EqualTo("Type2"));
        }

        [Test]
        public void SerializeDeserialize_MixedUnionAttributes()
        {
            // Test with traditional YamlObjectUnion member
            var container1 = new ContainerWithMixedUnion
            {
                Title = "Mixed Test 1",
                Content = new MixedUnionMember1 { Id = "id-001", Version = 5 }
            };

            var yaml1 = YamlSerializer.SerializeToString(container1);
            Assert.That(yaml1, Does.Contain("!mixed1"));

            var deserialized1 =
                YamlSerializer.Deserialize<ContainerWithMixedUnion>(StringEncoding.Utf8.GetBytes(yaml1));
            Assert.That(deserialized1.Content, Is.TypeOf<MixedUnionMember1>());
            Assert.That(((MixedUnionMember1)deserialized1.Content).Version, Is.EqualTo(5));

            // Test with YamlUnionMember attribute
            var container2 = new ContainerWithMixedUnion
            {
                Title = "Mixed Test 2",
                Content = new MixedUnionMember2 { Id = "id-002", Timestamp = new DateTime(2024, 1, 1) }
            };

            var yaml2 = YamlSerializer.SerializeToString(container2);
            Assert.That(yaml2, Does.Contain("!mixed2"));

            var deserialized2 =
                YamlSerializer.Deserialize<ContainerWithMixedUnion>(StringEncoding.Utf8.GetBytes(yaml2));
            Assert.That(deserialized2.Content, Is.TypeOf<MixedUnionMember2>());
            Assert.That(((MixedUnionMember2)deserialized2.Content).Timestamp, Is.EqualTo(new DateTime(2024, 1, 1)));
        }

        [Test]
        public void SerializeDeserialize_MultipleUnionMembers()
        {
            // Test switching between different union members
            var yaml1 = @"name: Container 1
item: !member1
  value: 10
  name: Item One";

            var container1 =
                YamlSerializer.Deserialize<ContainerWithUnionMemberAttribute>(StringEncoding.Utf8.GetBytes(yaml1));
            Assert.That(container1.Item, Is.TypeOf<UnionMember1>());
            Assert.That(((UnionMember1)container1.Item).Name, Is.EqualTo("Item One"));

            var yaml2 = @"name: Container 2
item: !member2
  value: 20
  price: 99.99";

            var container2 =
                YamlSerializer.Deserialize<ContainerWithUnionMemberAttribute>(StringEncoding.Utf8.GetBytes(yaml2));
            Assert.That(container2.Item, Is.TypeOf<UnionMember2>());
            Assert.That(((UnionMember2)container2.Item).Price, Is.EqualTo(99.99m));
        }

        [Test]
        public void SerializeDeserialize_AbstractUnionRoundTrip()
        {
            var original1 = new ConcreteUnionMember1 { Count = 123 };
            var yaml1 = YamlSerializer.SerializeToString(original1);
            Assert.That(yaml1, Does.Contain("!concrete1"));

            var restored1 =
                YamlSerializer.Deserialize<AbstractUnionWithMemberAttribute>(StringEncoding.Utf8.GetBytes(yaml1));
            Assert.That(restored1, Is.TypeOf<ConcreteUnionMember1>());
            Assert.That(((ConcreteUnionMember1)restored1).Count, Is.EqualTo(123));
            Assert.That(restored1.Type, Is.EqualTo("Type1"));

            var original2 = new ConcreteUnionMember2 { IsActive = false };
            var yaml2 = YamlSerializer.SerializeToString(original2);
            Assert.That(yaml2, Does.Contain("!concrete2"));

            var restored2 =
                YamlSerializer.Deserialize<AbstractUnionWithMemberAttribute>(StringEncoding.Utf8.GetBytes(yaml2));
            Assert.That(restored2, Is.TypeOf<ConcreteUnionMember2>());
            Assert.That(((ConcreteUnionMember2)restored2).IsActive, Is.False);
            Assert.That(restored2.Type, Is.EqualTo("Type2"));
        }
    }
}