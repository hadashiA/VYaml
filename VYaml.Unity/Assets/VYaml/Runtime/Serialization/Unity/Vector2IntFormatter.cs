using UnityEngine;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Unity
{
    public class Vector2IntFormatter : IYamlFormatter<Vector2Int>
    {
        public static readonly Vector2IntFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Vector2Int value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteInt32(value.x);
            emitter.WriteInt32(value.y);
            emitter.EndSequence();
        }

        public Vector2Int Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsInt32();
            var y = parser.ReadScalarAsInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new Vector2Int(x, y);
        }
    }
}