using UnityEngine;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Unity
{
    public class ColorFormatter : IYamlFormatter<Color>
    {
        public static readonly ColorFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Color value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.r);
            emitter.WriteFloat(value.g);
            emitter.WriteFloat(value.b);
            emitter.WriteFloat(value.a);
            emitter.EndSequence();
        }

        public Color Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r = parser.ReadScalarAsFloat();
            var g = parser.ReadScalarAsFloat();
            var b = parser.ReadScalarAsFloat();
            var a = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new Color(r, g, b, a);
        }
    }
}