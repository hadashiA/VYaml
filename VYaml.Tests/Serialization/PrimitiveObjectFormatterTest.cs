using System;
using System.Collections.Generic;
using NUnit.Framework;
using VYaml.Annotations;
using VYaml.Internal;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class PrimitiveObjectFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize_Enum()
        {
            var options = new YamlSerializerOptions
            {
                NamingConvention = NamingConvention.UpperCamelCase
            };
            Assert.That(Serialize<dynamic>(SimpleEnum.A, options), Is.EqualTo("A"));
            Assert.That(Serialize<dynamic>(NamingConventionEnum.HogeFuga, options), Is.EqualTo("hoge_fuga"));
            Assert.That(Serialize<dynamic>(DataMemberLabeledEnum.C, options), Is.EqualTo("c-alias"));
        }

        [Test]
        public void Serialize_dynamic()
        {
            var data = new Dictionary<string, object>
            {
                { "invoice", 34843 },
                { "date", new DateTime(2001, 1, 23) },
                { "bill-to", new Dictionary<string, object>
                    {
                        { "given", "Chris" },
                        { "family", "Dumars" },
                        { "address", new Dictionary<string, object>
                            {
                                { "lines", "458 Walkman Dr.\nSuite #292\n" },
                                { "city", "Royal Oak" },
                                { "state", "MI" },
                                { "postal", "48046" },
                            }
                        }
                    }
                },
                { "product", new object[]
                    {
                        new Dictionary<string, object>
                        {
                            { "sku", "BL394D" },
                            { "quantity", 4 },
                            { "description", "Basketball" },
                            { "price", 450.00 },
                        },
                        new Dictionary<string, object>
                        {
                            { "sku", "BL4438H" },
                            { "quantity", 1 },
                            { "description", "Super Hoop" },
                            { "price", 2392.00 },
                        }
                    }
                },
                { "tax", 251.42 },
                { "total", 4443.52 },
                { "comments", "Late afternoon is best.\nBackup contact is Nancy\nBillsmer @ 338-4338." }
            };

            var result = Serialize<dynamic>(data);
            Assert.That(result, Is.EqualTo(
@"invoice: 34843
date: 2001-01-23T00:00:00.0000000
bill-to:
  given: Chris
  family: Dumars
  address:
    lines: |
      458 Walkman Dr.
      Suite #292
    city: Royal Oak
    state: MI
    postal: ""48046""
product:
- sku: BL394D
  quantity: 4
  description: Basketball
  price: 450
- sku: BL4438H
  quantity: 1
  description: Super Hoop
  price: 2392
tax: 251.42
total: 4443.52
comments: |-
  Late afternoon is best.
  Backup contact is Nancy
  Billsmer @ 338-4338.
"));
        }

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
