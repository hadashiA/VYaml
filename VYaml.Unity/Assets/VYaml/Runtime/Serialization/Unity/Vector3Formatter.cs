using UnityEngine;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Unity
{
    public class Vector3Formatter : IYamlFormatter<Vector3>
    {
        public static readonly Vector3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Vector3 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.WriteFloat(value.z);
            emitter.EndSequence();
        }

        public Vector3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }
            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            var z = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new Vector3(x, y, z);
        }
    }
}