using System;
using NUnit.Framework;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class UriFormatterTest : FormatterTestBase
    {
        [Test]
        public void Serialize()
        {
            var uri = new Uri("https://example.com:5000/?name=Jonathan&age=18#hoge");
            var result = Serialize(uri);
            Assert.That(result, Is.EqualTo("\"https://example.com:5000/?name=Jonathan&age=18#hoge\""));
        }

        [Test]
        public void Deserialize()
        {
            var result = Deserialize<Uri>("https://example.com:5000/?name=Jonathan&age=18#hoge");
            Assert.That(result, Is.EqualTo(new Uri("https://example.com:5000/?name=Jonathan&age=18#hoge")));
        }
    }
}