using UnityEngine;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Unity
{
    public class QuaternionFormatter : IYamlFormatter<Quaternion>
    {
        public static readonly QuaternionFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Quaternion value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.WriteFloat(value.z);
            emitter.WriteFloat(value.w);
            emitter.EndSequence();
        }

        public Quaternion Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            var z = parser.ReadScalarAsFloat();
            var w = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new Quaternion(x, y, z, w);
        }
    }
}