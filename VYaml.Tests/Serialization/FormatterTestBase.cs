using NUnit.Framework;
using VYaml.Internal;
using VYaml.Serialization;

namespace VYaml.Tests.Serialization
{
    public class FormatterTestBase
    {
        protected static string Serialize<T>(T value, YamlSerializerOptions? options = null)
        {
            return YamlSerializer.SerializeToString(value, options);
        }

        protected static T Deserialize<T>(string yaml, YamlSerializerOptions? options = null)
        {
            var bytes = StringEncoding.Utf8.GetBytes(yaml);
            var result = YamlSerializer.Deserialize<T>(bytes, options);
            Assert.That(result, Is.InstanceOf<T>());
            return result;
        }
    }
}