using UnityEngine;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Unity
{
    public class Vector3IntFormatter : IYamlFormatter<Vector3Int>
    {
        public static readonly Vector3IntFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Vector3Int value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteInt32(value.x);
            emitter.WriteInt32(value.y);
            emitter.WriteInt32(value.z);
            emitter.EndSequence();
        }

        public Vector3Int Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsInt32();
            var y = parser.ReadScalarAsInt32();
            var z = parser.ReadScalarAsInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new Vector3Int(x, y, z);
        }
    }
}