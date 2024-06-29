using UnityEngine;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Unity
{
    public class Vector4Formatter : IYamlFormatter<Vector4>
    {
        public static readonly Vector4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Vector4 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.WriteFloat(value.z);
            emitter.WriteFloat(value.w);
            emitter.EndSequence();
        }

        public Vector4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
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
            var w = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new Vector4(x, y, z, w);
        }
    }
}