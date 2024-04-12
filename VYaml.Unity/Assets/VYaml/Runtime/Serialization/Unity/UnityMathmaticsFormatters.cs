#if VYAML_UNITY_MATHEMATICS_INTEGRATION
using Unity.Mathematics;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class UnityMathQuaternionFormatter : IYamlFormatter<quaternion>
    {
        public static readonly UnityMathQuaternionFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, quaternion value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.value.x);
            emitter.WriteFloat(value.value.y);
            emitter.WriteFloat(value.value.z);
            emitter.WriteFloat(value.value.w);
            emitter.EndSequence();
        }

        public quaternion Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            var z = parser.ReadScalarAsFloat();
            var w = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new quaternion(x, y, z, w);
        }
    }

    public class UnityMathBool2Formatter : IYamlFormatter<bool2>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, bool2 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteBool(value.x);
            emitter.WriteBool(value.y);
            emitter.EndSequence();
        }

        public bool2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsBool();
            var y = parser.ReadScalarAsBool();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new bool2(x, y);
        }
    }
}
#endif