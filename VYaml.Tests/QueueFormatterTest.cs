using System.Collections.Generic;
using NUnit.Framework;

namespace VYaml.Tests.Serialization
{
    public class QueueFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize()
        {
            var value = new Queue<int>();
            value.Enqueue(111);
            value.Enqueue(222);
            value.Enqueue(333);
            var serialied = Serialize(value);
            Assert.That(serialied, Is.EqualTo("- 111\n- 222\n- 333\n"));

            var deserialized = Deserialize<Queue<int>>(serialied);
            Assert.That(deserialized.Count, Is.EqualTo(3));
            Assert.That(deserialized.Dequeue(), Is.EqualTo(111));
            Assert.That(deserialized.Dequeue(), Is.EqualTo(222));
            Assert.That(deserialized.Dequeue(), Is.EqualTo(333));
        }
    }
}