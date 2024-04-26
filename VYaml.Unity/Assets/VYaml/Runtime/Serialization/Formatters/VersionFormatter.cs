using System;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class VersionFormatter : IYamlFormatter<Version?>
    {
        public static readonly VersionFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Version? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
            }
            else
            {
                emitter.WriteString(value.ToString());
            }
        }

        public Version? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            return parser.IsNullScalar() ? null : new Version(parser.ReadScalarAsString()!);
        }
    }
}