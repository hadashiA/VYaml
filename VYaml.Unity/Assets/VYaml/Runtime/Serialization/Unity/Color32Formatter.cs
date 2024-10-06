using UnityEngine;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Unity
{
    public class Color32Formatter : IYamlFormatter<Color32>
    {
        public static readonly Color32Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Color32 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteInt32(value.r);
            emitter.WriteInt32(value.g);
            emitter.WriteInt32(value.b);
            emitter.WriteInt32(value.a);
            emitter.EndSequence();
        }

        public Color32 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r = (byte)parser.ReadScalarAsInt32();
            var g = (byte)parser.ReadScalarAsInt32();
            var b = (byte)parser.ReadScalarAsInt32();
            var a = (byte)parser.ReadScalarAsInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new Color32(r, g, b, a);
        }
    }
}