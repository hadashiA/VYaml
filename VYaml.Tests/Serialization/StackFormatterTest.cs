using System.Collections.Generic;
using NUnit.Framework;

namespace VYaml.Tests.Serialization
{
    public class StackFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize()
        {
            var value = new Stack<int>();
            value.Push(111);
            value.Push(222);
            value.Push(333);
            var serialied = Serialize(value);
            Assert.That(serialied, Is.EqualTo("- 333\n- 222\n- 111\n"));

            var deserialized = Deserialize<Stack<int>>(serialied);
            Assert.That(deserialized.Count, Is.EqualTo(3));
            Assert.That(deserialized.Pop(), Is.EqualTo(333));
            Assert.That(deserialized.Pop(), Is.EqualTo(222));
            Assert.That(deserialized.Pop(), Is.EqualTo(111));
        }
    }
}