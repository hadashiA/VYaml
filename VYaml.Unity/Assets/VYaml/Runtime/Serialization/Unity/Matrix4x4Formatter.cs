using UnityEngine;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Unity
{
    public class Matrix4x4Formatter : IYamlFormatter<Matrix4x4>
    {
        public static readonly Matrix4x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Matrix4x4 value, YamlSerializationContext context)
        {
            emitter.BeginSequence();
            Vector4Formatter.Instance.Serialize(ref emitter, value.GetColumn(0), context);
            Vector4Formatter.Instance.Serialize(ref emitter, value.GetColumn(1), context);
            Vector4Formatter.Instance.Serialize(ref emitter, value.GetColumn(2), context);
            Vector4Formatter.Instance.Serialize(ref emitter, value.GetColumn(3), context);
            emitter.EndSequence();
        }

        public Matrix4x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var col0 = Vector4Formatter.Instance.Deserialize(ref parser, context);
            var col1 = Vector4Formatter.Instance.Deserialize(ref parser, context);
            var col2 = Vector4Formatter.Instance.Deserialize(ref parser, context);
            var col3 = Vector4Formatter.Instance.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new Matrix4x4(col0, col1, col2, col3);
        }
    }
}