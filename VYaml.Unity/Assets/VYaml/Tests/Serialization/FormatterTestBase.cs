using NUnit.Framework;
using VYaml.Internal;
using VYaml.Serialization;

namespace VYaml.Tests.Serialization
{
    public class FormatterTestBase
    {
        protected static T Deserialize<T>(string yaml)
        {
            var bytes = StringEncoding.Utf8.GetBytes(yaml);
            var result = YamlSerializer.Deserialize<T>(bytes);
            Assert.That(result, Is.InstanceOf<T>());
            return result;
        }
    }
}