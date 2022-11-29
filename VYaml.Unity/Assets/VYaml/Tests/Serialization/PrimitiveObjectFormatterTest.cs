using NUnit.Framework;
using VYaml.Internal;
using VYaml.Serialization;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class PrimitiveObjectFormatterTest
    {
        [Test]
        public void Deserialize_dynamic()
        {
            var bytes = StringEncoding.Utf8.GetBytes(SpecExamples.Ex2_27);
            var result = YamlSerializer.Deserialize<dynamic>(bytes, new YamlSerializerOptions
            {
                Resolver = PrimitiveObjectResolver.Instance
            });

            Assert.That(result["invoice"], Is.EqualTo(34843));
            Assert.That(result["date"], Is.EqualTo("2001-01-23"));
            Assert.That(result["bill-to"]["given"], Is.EqualTo("Chris"));
            Assert.That(result["bill-to"]["family"], Is.EqualTo("Dumars"));
            Assert.That(result["bill-to"]["address"]["lines"], Is.EqualTo("458 Walkman Dr.\nSuite #292\n"));
            Assert.That(result["bill-to"]["address"]["city"], Is.EqualTo("Royal Oak"));
            Assert.That(result["bill-to"]["address"]["state"], Is.EqualTo("MI"));
            Assert.That(result["bill-to"]["address"]["postal"], Is.EqualTo(48046));
            Assert.That(result["ship-to"]["given"], Is.EqualTo("Chris"));
            Assert.That(result["ship-to"]["family"], Is.EqualTo("Dumars"));
            Assert.That(result["ship-to"]["address"]["lines"], Is.EqualTo("458 Walkman Dr.\nSuite #292\n"));
            Assert.That(result["ship-to"]["address"]["city"], Is.EqualTo("Royal Oak"));
            Assert.That(result["ship-to"]["address"]["state"], Is.EqualTo("MI"));
            Assert.That(result["ship-to"]["address"]["postal"], Is.EqualTo(48046));
            Assert.That(result["product"][0]["sku"], Is.EqualTo("BL394D"));
            Assert.That(result["product"][0]["quantity"], Is.EqualTo(4));
            Assert.That(result["product"][0]["description"], Is.EqualTo("Basketball"));
            Assert.That(result["product"][0]["price"], Is.EqualTo(450.00));
            Assert.That(result["product"][1]["sku"], Is.EqualTo("BL4438H"));
            Assert.That(result["product"][1]["quantity"], Is.EqualTo(1));
            Assert.That(result["product"][1]["description"], Is.EqualTo("Super Hoop"));
            Assert.That(result["product"][1]["price"], Is.EqualTo(2392.00));
            Assert.That(result["tax"], Is.EqualTo(251.42));
            Assert.That(result["total"], Is.EqualTo(4443.52));
            Assert.That(result["comments"], Is.EqualTo("Late afternoon is best. Backup contact is Nancy Billsmer @ 338-4338."));
        }
    }
}
