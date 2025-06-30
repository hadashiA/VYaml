using System;
using NUnit.Framework;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class DateTimeFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize_Utc()
        {
            var result = Serialize(new DateTime(2022, 12, 31, 11, 22, 33, DateTimeKind.Utc));
            Assert.That(result, Is.EqualTo("2022-12-31T11:22:33.0000000Z"));
        }

        [Test]
        public void Serialize_Local()
        {
            var result = Serialize(new DateTime(2022, 12, 31, 11, 22, 33, DateTimeKind.Local));
            // The timezone offset can be either positive (+) or negative (-)
            Assert.That(result.StartsWith("2022-12-31T11:22:33.0000000+") || 
                       result.StartsWith("2022-12-31T11:22:33.0000000-"), Is.True);
        }

        [Test]
        public void Deserialize_Utc()
        {
            var result = Deserialize<DateTime>("2022-12-31T11:22:33Z");
            Assert.That(result, Is.EqualTo(new DateTime(2022, 12, 31, 11, 22, 33, DateTimeKind.Utc)));
        }
    }
}
