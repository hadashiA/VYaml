using System;
using NUnit.Framework;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class TimeSpanFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize_SecondsPrecision()
        {
            var result = Serialize(TimeSpan.FromSeconds(1));
            Assert.That(result, Is.EqualTo("00:00:01"));
        }

        [Test]
        public void Serialize_MillisecondsPrecision()
        {
            var result = Serialize(TimeSpan.FromMilliseconds(1));
            Assert.That(result, Is.EqualTo("00:00:00.0010000"));
        }

        [Test]
        public void Deserialize_SecondsPrecision()
        {
            var result = Deserialize<TimeSpan>("00:00:01");
            Assert.That(result.TotalSeconds, Is.EqualTo(1));
        }

        [Test]
        public void Deserialize_MillisecondsPrecision()
        {
            var result = Deserialize<TimeSpan>("00:00:00.0010000");
            Assert.That(result.TotalMilliseconds, Is.EqualTo(1));
        }
    }
}
