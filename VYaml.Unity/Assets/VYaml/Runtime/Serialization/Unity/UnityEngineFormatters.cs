using UnityEngine;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class UnityEngineFormatters
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

    public class RectFormatter : IYamlFormatter<Rect>
    {
        public static readonly RectFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Rect value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.WriteFloat(value.width);
            emitter.WriteFloat(value.height);
            emitter.EndSequence();
        }

        public Rect Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;
            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            var w = parser.ReadScalarAsFloat();
            var h = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new Rect(x, y, w, h);
        }
    }

    public class RectIntFormatter : IYamlFormatter<RectInt>
    {
        public static readonly RectIntFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, RectInt value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteInt32(value.x);
            emitter.WriteInt32(value.y);
            emitter.WriteInt32(value.width);
            emitter.WriteInt32(value.height);
            emitter.EndSequence();
        }

        public RectInt Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsInt32();
            var y = parser.ReadScalarAsInt32();
            var w = parser.ReadScalarAsInt32();
            var h = parser.ReadScalarAsInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new RectInt(x, y, w, h);
        }
    }

    public class RectOffsetFormatter : IYamlFormatter<RectOffset>
    {
        public static readonly RectOffsetFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, RectOffset value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteInt32(value.left);
            emitter.WriteInt32(value.right);
            emitter.WriteInt32(value.top);
            emitter.WriteInt32(value.bottom);
            emitter.EndSequence();
        }

        public RectOffset Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var l = parser.ReadScalarAsInt32();
            var r = parser.ReadScalarAsInt32();
            var t = parser.ReadScalarAsInt32();
            var b = parser.ReadScalarAsInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new RectOffset(l, r, t, b);
        }
    }

    public class Vector2Formatter : IYamlFormatter<Vector2>
    {
        public static readonly Vector2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Vector2 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.EndSequence();
        }

        public Vector2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }
            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new Vector2(x, y);
        }
    }

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