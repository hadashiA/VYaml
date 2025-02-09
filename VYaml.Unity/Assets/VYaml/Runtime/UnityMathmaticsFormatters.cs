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

    public class Bool2Formatter : IYamlFormatter<bool2>
    {
        public static readonly Bool2Formatter Instance = new();

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

    public class Bool3Formatter : IYamlFormatter<bool3>
    {
        public static readonly Bool3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool3 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteBool(value.x);
            emitter.WriteBool(value.y);
            emitter.WriteBool(value.z);
            emitter.EndSequence();
        }

        public bool3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsBool();
            var y = parser.ReadScalarAsBool();
            var z = parser.ReadScalarAsBool();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new bool3(x, y, z);
        }
    }

    public class Bool4Formatter : IYamlFormatter<bool4>
    {
        public static readonly Bool4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool4 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteBool(value.x);
            emitter.WriteBool(value.y);
            emitter.WriteBool(value.z);
            emitter.WriteBool(value.w);
            emitter.EndSequence();
        }

        public bool4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsBool();
            var y = parser.ReadScalarAsBool();
            var z = parser.ReadScalarAsBool();
            var w = parser.ReadScalarAsBool();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new bool4(x, y, z, w);
        }
    }

    public class Double2Formatter : IYamlFormatter<double2>
    {
        public static readonly Double2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double2 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteDouble(value.x);
            emitter.WriteDouble(value.y);
            emitter.EndSequence();
        }

        public double2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsDouble();
            var y = parser.ReadScalarAsDouble();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new double2(x, y);
        }
    }

    public class Double3Formatter : IYamlFormatter<double3>
    {
        public static readonly Double3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double3 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteDouble(value.x);
            emitter.WriteDouble(value.y);
            emitter.WriteDouble(value.z);
            emitter.EndSequence();
        }

        public double3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsDouble();
            var y = parser.ReadScalarAsDouble();
            var z = parser.ReadScalarAsDouble();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new double3(x, y, z);
        }
    }

    public class Double4Formatter : IYamlFormatter<double4>
    {
        public static readonly Double4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double4 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteDouble(value.x);
            emitter.WriteDouble(value.y);
            emitter.WriteDouble(value.z);
            emitter.WriteDouble(value.w);
            emitter.EndSequence();
        }

        public double4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsDouble();
            var y = parser.ReadScalarAsDouble();
            var z = parser.ReadScalarAsDouble();
            var w = parser.ReadScalarAsDouble();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new double4(x, y, z, w);
        }
    }

    public class Float2Formatter : IYamlFormatter<float2>
    {
        public static readonly Float2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float2 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.EndSequence();
        }

        public float2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new float2(x, y);
        }
    }

    public class Float3Formatter : IYamlFormatter<float3>
    {
        public static readonly Float3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float3 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.WriteFloat(value.z);
            emitter.EndSequence();
        }

        public float3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            var z = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new float3(x, y, z);
        }
    }

    public class Float4Formatter : IYamlFormatter<float4>
    {
        public static readonly Float4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float4 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.WriteFloat(value.z);
            emitter.WriteFloat(value.w);
            emitter.EndSequence();
        }

        public float4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            var z = parser.ReadScalarAsFloat();
            var w = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new float4(x, y, z, w);
        }
    }

    public class Half2Formatter : IYamlFormatter<half2>
    {
        public static readonly Half2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, half2 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.EndSequence();
        }

        public half2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new half2(new half(x), new half(y));
        }
    }

    public class Half3Formatter : IYamlFormatter<half3>
    {
        public static readonly Half3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, half3 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.WriteFloat(value.z);
            emitter.EndSequence();
        }

        public half3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            var z = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new half3(new half(x), new half(y), new half(z));
        }
    }


    public class Half4Formatter : IYamlFormatter<half4>
    {
        public static readonly Half4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, half4 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteFloat(value.x);
            emitter.WriteFloat(value.y);
            emitter.WriteFloat(value.z);
            emitter.WriteFloat(value.w);
            emitter.EndSequence();
        }

        public half4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsFloat();
            var y = parser.ReadScalarAsFloat();
            var z = parser.ReadScalarAsFloat();
            var w = parser.ReadScalarAsFloat();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new half4(new half(x), new half(y), new half(z), new half(w));
        }
    }

    public class Int2Formatter : IYamlFormatter<int2>
    {
        public static readonly Int2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int2 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteInt32(value.x);
            emitter.WriteInt32(value.y);
            emitter.EndSequence();
        }

        public int2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsInt32();
            var y = parser.ReadScalarAsInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new int2(x, y);
        }
    }

    public class Int3Formatter : IYamlFormatter<int3>
    {
        public static readonly Int3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int3 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteInt32(value.x);
            emitter.WriteInt32(value.y);
            emitter.WriteInt32(value.z);
            emitter.EndSequence();
        }

        public int3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsInt32();
            var y = parser.ReadScalarAsInt32();
            var z = parser.ReadScalarAsInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new int3(x, y, z);
        }
    }

    public class Int4Formatter : IYamlFormatter<int4>
    {
        public static readonly Int4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int4 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteInt32(value.x);
            emitter.WriteInt32(value.y);
            emitter.WriteInt32(value.z);
            emitter.WriteInt32(value.w);
            emitter.EndSequence();
        }

        public int4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsInt32();
            var y = parser.ReadScalarAsInt32();
            var z = parser.ReadScalarAsInt32();
            var w = parser.ReadScalarAsInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new int4(x, y, z, w);
        }
    }

    public class UInt2Formatter : IYamlFormatter<uint2>
    {
        public static readonly UInt2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint2 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteUInt32(value.x);
            emitter.WriteUInt32(value.y);
            emitter.EndSequence();
        }

        public uint2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsUInt32();
            var y = parser.ReadScalarAsUInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new uint2(x, y);
        }
    }

    public class UInt3Formatter : IYamlFormatter<uint3>
    {
        public static readonly UInt3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint3 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteUInt32(value.x);
            emitter.WriteUInt32(value.y);
            emitter.WriteUInt32(value.z);
            emitter.EndSequence();
        }

        public uint3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsUInt32();
            var y = parser.ReadScalarAsUInt32();
            var z = parser.ReadScalarAsUInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new uint3(x, y, z);
        }
    }

    public class UInt4Formatter : IYamlFormatter<uint4>
    {
        public static readonly UInt4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint4 value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            emitter.WriteUInt32(value.x);
            emitter.WriteUInt32(value.y);
            emitter.WriteUInt32(value.z);
            emitter.WriteUInt32(value.w);
            emitter.EndSequence();
        }

        public uint4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var x = parser.ReadScalarAsUInt32();
            var y = parser.ReadScalarAsUInt32();
            var z = parser.ReadScalarAsUInt32();
            var w = parser.ReadScalarAsUInt32();
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new uint4(x, y, z, w);
        }
    }

    public class Bool2x2Formatter : IYamlFormatter<bool2x2>
    {
        public static readonly Bool2x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool2x2 value, YamlSerializationContext context)
        {
            var bool2Formatter = context.Resolver.GetFormatterWithVerify<bool2>();
            var r0 = new bool2(value.c0.x, value.c1.x);
            var r1 = new bool2(value.c0.y, value.c1.y);

            emitter.BeginSequence();
            bool2Formatter.Serialize(ref emitter, r0, context);
            bool2Formatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public bool2x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var bool2Formatter = context.Resolver.GetFormatterWithVerify<bool2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = bool2Formatter.Deserialize(ref parser, context);
            var r1 = bool2Formatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new bool2x2(r0.x, r0.y, r1.x, r1.y);
        }
    }

    public class Bool2x3Formatter : IYamlFormatter<bool2x3>
    {
        public static readonly Bool2x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool2x3 value, YamlSerializationContext context)
        {
            var bool3Formatter = context.Resolver.GetFormatterWithVerify<bool3>();
            var r0 = new bool3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new bool3(value.c0.y, value.c1.y, value.c2.y);

            emitter.BeginSequence();
            bool3Formatter.Serialize(ref emitter, r0, context);
            bool3Formatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public bool2x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var bool3Formatter = context.Resolver.GetFormatterWithVerify<bool3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = bool3Formatter.Deserialize(ref parser, context);
            var r1 = bool3Formatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new bool2x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z);
        }
    }

    public class Bool2x4Formatter : IYamlFormatter<bool2x4>
    {
        public static readonly Bool2x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool2x4 value, YamlSerializationContext context)
        {
            var bool4Formatter = context.Resolver.GetFormatterWithVerify<bool4>();
            var r0 = new bool4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new bool4(value.c0.x, value.c1.y, value.c2.y, value.c3.y);

            emitter.BeginSequence();
            bool4Formatter.Serialize(ref emitter, r0, context);
            bool4Formatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public bool2x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var bool3Formatter = context.Resolver.GetFormatterWithVerify<bool4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = bool3Formatter.Deserialize(ref parser, context);
            var r1 = bool3Formatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new bool2x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w);
        }
    }

    public class Bool3x2Formatter : IYamlFormatter<bool3x2>
    {
        public static readonly Bool3x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool3x2 value, YamlSerializationContext context)
        {
            var bool2Formatter = context.Resolver.GetFormatterWithVerify<bool2>();
            var r0 = new bool2(value.c0.x, value.c1.x);
            var r1 = new bool2(value.c0.y, value.c1.y);
            var r2 = new bool2(value.c0.z, value.c1.z);

            emitter.BeginSequence();
            bool2Formatter.Serialize(ref emitter, r0, context);
            bool2Formatter.Serialize(ref emitter, r1, context);
            bool2Formatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public bool3x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var bool2Formatter = context.Resolver.GetFormatterWithVerify<bool2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = bool2Formatter.Deserialize(ref parser, context);
            var r1 = bool2Formatter.Deserialize(ref parser, context);
            var r2 = bool2Formatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new bool3x2(
                r0.x, r0.y,
                r1.x, r1.y,
                r2.x, r2.y);
        }
    }

    public class Bool3x3Formatter : IYamlFormatter<bool3x3>
    {
        public static readonly Bool3x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool3x3 value, YamlSerializationContext context)
        {
            var bool3Formatter = context.Resolver.GetFormatterWithVerify<bool3>();
            var r0 = new bool3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new bool3(value.c0.y, value.c1.y, value.c2.y);
            var r2 = new bool3(value.c0.z, value.c2.z, value.c2.z);

            emitter.BeginSequence();
            bool3Formatter.Serialize(ref emitter, r0, context);
            bool3Formatter.Serialize(ref emitter, r1, context);
            bool3Formatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public bool3x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var bool3Formatter = context.Resolver.GetFormatterWithVerify<bool3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = bool3Formatter.Deserialize(ref parser, context);
            var r1 = bool3Formatter.Deserialize(ref parser, context);
            var r2 = bool3Formatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new bool3x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z,
                r2.x, r2.y, r2.z);
        }
    }

    public class Bool3x4Formatter : IYamlFormatter<bool3x4>
    {
        public static readonly Bool3x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool3x4 value, YamlSerializationContext context)
        {
            var bool4Formatter = context.Resolver.GetFormatterWithVerify<bool4>();
            var r0 = new bool4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new bool4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);
            var r2 = new bool4(value.c0.z, value.c2.z, value.c2.z, value.c3.z);

            emitter.BeginSequence();
            bool4Formatter.Serialize(ref emitter, r0, context);
            bool4Formatter.Serialize(ref emitter, r1, context);
            bool4Formatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public bool3x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var bool4Formatter = context.Resolver.GetFormatterWithVerify<bool4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = bool4Formatter.Deserialize(ref parser, context);
            var r1 = bool4Formatter.Deserialize(ref parser, context);
            var r2 = bool4Formatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new bool3x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w,
                r2.x, r2.y, r2.z, r2.w);
        }
    }

    public class Bool4x2Formatter : IYamlFormatter<bool4x2>
    {
        public static readonly Bool4x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool4x2 value, YamlSerializationContext context)
        {
            var r0 = new bool2(value.c0.x, value.c1.x);
            var r1 = new bool2(value.c0.y, value.c1.y);
            var r2 = new bool2(value.c0.z, value.c1.z);
            var r3 = new bool2(value.c0.w, value.c1.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<bool2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public bool4x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<bool2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new bool4x2(
                r0.x, r0.y,
                r1.x, r1.y,
                r2.x, r2.y,
                r3.x, r3.y);
        }
    }

    public class Bool4x3Formatter : IYamlFormatter<bool4x3>
    {
        public static readonly Bool4x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool4x3 value, YamlSerializationContext context)
        {
            var r0 = new bool3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new bool3(value.c0.y, value.c1.y, value.c2.y);
            var r2 = new bool3(value.c0.z, value.c1.z, value.c2.z);
            var r3 = new bool3(value.c0.w, value.c1.w, value.c2.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<bool3>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public bool4x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<bool3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new bool4x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z,
                r2.x, r2.y, r2.z,
                r3.x, r3.y, r3.z);
        }
    }

    public class Bool4x4Formatter : IYamlFormatter<bool4x4>
    {
        public static readonly Bool4x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, bool4x4 value, YamlSerializationContext context)
        {
            var r0 = new bool4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new bool4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);
            var r2 = new bool4(value.c0.z, value.c1.z, value.c2.z, value.c3.z);
            var r3 = new bool4(value.c0.w, value.c1.w, value.c2.w, value.c3.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<bool4>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public bool4x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<bool4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new bool4x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w,
                r2.x, r2.y, r2.z, r2.w,
                r3.x, r3.y, r3.z, r3.w);
        }
    }

    public class Double2x2Formatter : IYamlFormatter<double2x2>
    {
        public static readonly Double2x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double2x2 value, YamlSerializationContext context)
        {
            var r0 = new double2(value.c0.x, value.c1.x);
            var r1 = new double2(value.c0.y, value.c1.y);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<double2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public double2x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<double2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new double2x2(
                r0.x, r0.y,
                r1.x, r1.y);
        }
    }

    public class Double2x3Formatter : IYamlFormatter<double2x3>
    {
        public static readonly Double2x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double2x3 value, YamlSerializationContext context)
        {
            var rowFormatter = context.Resolver.GetFormatterWithVerify<double3>();
            var r0 = new double3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new double3(value.c0.y, value.c1.y, value.c2.y);

            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public double2x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<double3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new double2x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z);
        }
    }

    public class Double2x4Formatter : IYamlFormatter<double2x4>
    {
        public static readonly Double2x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double2x4 value, YamlSerializationContext context)
        {
            var rowFormatter = context.Resolver.GetFormatterWithVerify<double4>();
            var r0 = new double4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new double4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);

            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public double2x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<double4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new double2x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w);
        }
    }

    public class Double3x2Formatter : IYamlFormatter<double3x2>
    {
        public static readonly Double3x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double3x2 value, YamlSerializationContext context)
        {
            var rowFormatter = context.Resolver.GetFormatterWithVerify<double2>();
            var r0 = new double2(value.c0.x, value.c1.x);
            var r1 = new double2(value.c0.y, value.c1.y);
            var r2 = new double2(value.c0.z, value.c1.z);

            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public double3x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<double2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new double3x2(
                r0.x, r0.y,
                r1.x, r1.y,
                r2.x, r2.y);
        }
    }

    public class Double3x3Formatter : IYamlFormatter<double3x3>
    {
        public static readonly Double3x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double3x3 value, YamlSerializationContext context)
        {
            var rowFormatter = context.Resolver.GetFormatterWithVerify<double3>();
            var r0 = new double3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new double3(value.c0.y, value.c1.y, value.c2.y);
            var r2 = new double3(value.c0.z, value.c1.z, value.c2.z);

            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public double3x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<double3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new double3x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z,
                r2.x, r2.y, r2.z);
        }
    }

    public class Double3x4Formatter : IYamlFormatter<double3x4>
    {
        public static readonly Double3x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double3x4 value, YamlSerializationContext context)
        {
            var rowFormatter = context.Resolver.GetFormatterWithVerify<double4>();
            var r0 = new double4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new double4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);
            var r2 = new double4(value.c0.z, value.c1.z, value.c2.z, value.c3.z);

            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public double3x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<double4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new double3x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w,
                r2.x, r2.y, r2.z, r2.w);
        }
    }

    public class Double4x2Formatter : IYamlFormatter<double4x2>
    {
        public static readonly Double4x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double4x2 value, YamlSerializationContext context)
        {
            var rowFormatter = context.Resolver.GetFormatterWithVerify<double2>();
            var r0 = new double2(value.c0.x, value.c1.x);
            var r1 = new double2(value.c0.y, value.c1.y);
            var r2 = new double2(value.c0.z, value.c1.z);
            var r3 = new double2(value.c0.w, value.c1.w);

            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public double4x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<double2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new double4x2(
                r0.x, r0.y,
                r1.x, r1.y,
                r2.x, r2.y,
                r3.x, r3.y);
        }
    }

    public class Double4x3Formatter : IYamlFormatter<double4x3>
    {
        public static readonly Double4x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double4x3 value, YamlSerializationContext context)
        {
            var rowFormatter = context.Resolver.GetFormatterWithVerify<double3>();
            var r0 = new double3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new double3(value.c0.y, value.c1.y, value.c2.y);
            var r2 = new double3(value.c0.z, value.c1.z, value.c2.z);
            var r3 = new double3(value.c0.w, value.c1.w, value.c2.w);

            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public double4x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<double3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new double4x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z,
                r2.x, r2.y, r2.z,
                r3.x, r3.y, r3.z);
        }
    }

    public class Double4x4Formatter : IYamlFormatter<double4x4>
    {
        public static readonly Double4x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double4x4 value, YamlSerializationContext context)
        {
            var rowFormatter = context.Resolver.GetFormatterWithVerify<double4>();
            var r0 = new double4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new double4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);
            var r2 = new double4(value.c0.z, value.c1.z, value.c2.z, value.c3.z);
            var r3 = new double4(value.c0.w, value.c1.w, value.c2.w, value.c3.w);

            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public double4x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<double4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new double4x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w,
                r2.x, r2.y, r2.z, r2.w,
                r3.x, r3.y, r3.z, r3.w);
        }
    }

    public class Float2x2Formatter : IYamlFormatter<float2x2>
    {
        public static readonly Float2x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float2x2 value, YamlSerializationContext context)
        {
            var r0 = new float2(value.c0.x, value.c1.x);
            var r1 = new float2(value.c0.y, value.c1.y);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public float2x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new float2x2(
                r0.x, r0.y,
                r1.x, r1.y);
        }
    }

    public class Float2x3Formatter : IYamlFormatter<float2x3>
    {
        public static readonly Float2x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float2x3 value, YamlSerializationContext context)
        {
            var r0 = new float3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new float3(value.c0.y, value.c1.y, value.c2.y);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float3>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public float2x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new float2x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z);
        }
    }

    public class Float2x4Formatter : IYamlFormatter<float2x4>
    {
        public static readonly Float2x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float2x4 value, YamlSerializationContext context)
        {
            var r0 = new float4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new float4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float4>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public float2x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new float2x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w);
        }
    }

    public class Float3x2Formatter : IYamlFormatter<float3x2>
    {
        public static readonly Float3x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float3x2 value, YamlSerializationContext context)
        {
            var r0 = new float2(value.c0.x, value.c1.x);
            var r1 = new float2(value.c0.y, value.c1.y);
            var r2 = new float2(value.c0.z, value.c1.z);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public float3x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new float3x2(
                r0.x, r0.y,
                r1.x, r1.y,
                r2.x, r2.y);
        }
    }

    public class Float3x3Formatter : IYamlFormatter<float3x3>
    {
        public static readonly Float3x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float3x3 value, YamlSerializationContext context)
        {
            var r0 = new float3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new float3(value.c0.y, value.c1.y, value.c2.y);
            var r2 = new float3(value.c0.z, value.c1.z, value.c2.z);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float3>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public float3x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new float3x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z,
                r2.x, r2.y, r2.z);
        }
    }

    public class Float3x4Formatter : IYamlFormatter<float3x4>
    {
        public static readonly Float3x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float3x4 value, YamlSerializationContext context)
        {
            var r0 = new float4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new float4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);
            var r2 = new float4(value.c0.z, value.c1.z, value.c2.z, value.c3.z);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float4>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public float3x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new float3x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w,
                r2.x, r2.y, r2.z, r2.w);
        }
    }

    public class Float4x2Formatter : IYamlFormatter<float4x2>
    {
        public static readonly Float4x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float4x2 value, YamlSerializationContext context)
        {
            var r0 = new float2(value.c0.x, value.c1.x);
            var r1 = new float2(value.c0.y, value.c1.y);
            var r2 = new float2(value.c0.z, value.c1.z);
            var r3 = new float2(value.c0.w, value.c1.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public float4x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new float4x2(
                r0.x, r0.y,
                r1.x, r1.y,
                r2.x, r2.y,
                r3.x, r3.y);
        }
    }

    public class Float4x3Formatter : IYamlFormatter<float4x3>
    {
        public static readonly Float4x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float4x3 value, YamlSerializationContext context)
        {
            var r0 = new float3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new float3(value.c0.y, value.c1.y, value.c2.y);
            var r2 = new float3(value.c0.z, value.c1.z, value.c2.z);
            var r3 = new float3(value.c0.w, value.c1.w, value.c2.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float3>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public float4x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new float4x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z,
                r2.x, r2.y, r2.z,
                r3.x, r3.y, r3.z);
        }
    }

    public class Float4x4Formatter : IYamlFormatter<float4x4>
    {
        public static readonly Float4x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, float4x4 value, YamlSerializationContext context)
        {
            var r0 = new float4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new float4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);
            var r2 = new float4(value.c0.z, value.c1.z, value.c2.z, value.c3.z);
            var r3 = new float4(value.c0.w, value.c1.w, value.c2.w, value.c3.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float4>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public float4x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<float4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new float4x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w,
                r2.x, r2.y, r2.z, r2.w,
                r3.x, r3.y, r3.z, r3.w);
        }
    }

    public class Int2x2Formatter : IYamlFormatter<int2x2>
    {
        public static readonly Int2x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int2x2 value, YamlSerializationContext context)
        {
            var r0 = new int2(value.c0.x, value.c1.x);
            var r1 = new int2(value.c0.y, value.c1.y);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public int2x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new int2x2(
                r0.x, r0.y,
                r1.x, r1.y);
        }
    }

    public class Int2x3Formatter : IYamlFormatter<int2x3>
    {
        public static readonly Int2x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int2x3 value, YamlSerializationContext context)
        {
            var r0 = new int3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new int3(value.c0.y, value.c1.y, value.c2.y);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int3>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public int2x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new int2x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z);
        }
    }

    public class Int2x4Formatter : IYamlFormatter<int2x4>
    {
        public static readonly Int2x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int2x4 value, YamlSerializationContext context)
        {
            var r0 = new int4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new int4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int4>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public int2x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new int2x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w);
        }
    }

    public class Int3x2Formatter : IYamlFormatter<int3x2>
    {
        public static readonly Int3x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int3x2 value, YamlSerializationContext context)
        {
            var r0 = new int2(value.c0.x, value.c1.x);
            var r1 = new int2(value.c0.y, value.c1.y);
            var r2 = new int2(value.c0.z, value.c1.z);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public int3x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new int3x2(
                r0.x, r0.y,
                r1.x, r1.y,
                r2.x, r2.y);
        }
    }

    public class Int3x3Formatter : IYamlFormatter<int3x3>
    {
        public static readonly Int3x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int3x3 value, YamlSerializationContext context)
        {
            var r0 = new int3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new int3(value.c0.y, value.c1.y, value.c2.y);
            var r2 = new int3(value.c0.z, value.c1.z, value.c2.z);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int3>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public int3x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new int3x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z,
                r2.x, r2.y, r2.z);
        }
    }

    public class Int3x4Formatter : IYamlFormatter<int3x4>
    {
        public static readonly Int3x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int3x4 value, YamlSerializationContext context)
        {
            var r0 = new int4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new int4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);
            var r2 = new int4(value.c0.z, value.c1.z, value.c2.z, value.c3.z);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int4>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public int3x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new int3x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w,
                r2.x, r2.y, r2.z, r2.w);
        }
    }

    public class Int4x2Formatter : IYamlFormatter<int4x2>
    {
        public static readonly Int4x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int4x2 value, YamlSerializationContext context)
        {
            var r0 = new int2(value.c0.x, value.c1.x);
            var r1 = new int2(value.c0.y, value.c1.y);
            var r2 = new int2(value.c0.z, value.c1.z);
            var r3 = new int2(value.c0.z, value.c1.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public int4x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new int4x2(
                r0.x, r0.y,
                r1.x, r1.y,
                r2.x, r2.y,
                r3.x, r3.y);
        }
    }

    public class Int4x3Formatter : IYamlFormatter<int4x3>
    {
        public static readonly Int4x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int4x3 value, YamlSerializationContext context)
        {
            var r0 = new int3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new int3(value.c0.y, value.c1.y, value.c2.y);
            var r2 = new int3(value.c0.z, value.c1.z, value.c2.z);
            var r3 = new int3(value.c0.z, value.c1.w, value.c2.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int3>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public int4x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new int4x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z,
                r2.x, r2.y, r2.z,
                r3.x, r3.y, r3.z);
        }
    }

    public class Int4x4Formatter : IYamlFormatter<int4x4>
    {
        public static readonly Int4x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, int4x4 value, YamlSerializationContext context)
        {
            var r0 = new int4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new int4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);
            var r2 = new int4(value.c0.z, value.c1.z, value.c2.z, value.c3.z);
            var r3 = new int4(value.c0.z, value.c1.w, value.c2.w, value.c3.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int4>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public int4x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<int4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new int4x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w,
                r2.x, r2.y, r2.z, r2.w,
                r3.x, r3.y, r3.z, r3.w);
        }
    }

    public class UInt2x2Formatter : IYamlFormatter<uint2x2>
    {
        public static readonly UInt2x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint2x2 value, YamlSerializationContext context)
        {
            var r0 = new uint2(value.c0.x, value.c1.x);
            var r1 = new uint2(value.c0.y, value.c1.y);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public uint2x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new uint2x2(
                r0.x, r0.y,
                r1.x, r1.y);
        }
    }

    public class UInt2x3Formatter : IYamlFormatter<uint2x3>
    {
        public static readonly UInt2x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint2x3 value, YamlSerializationContext context)
        {
            var r0 = new uint3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new uint3(value.c0.y, value.c1.y, value.c2.y);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint3>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public uint2x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new uint2x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z);
        }
    }

    public class UInt2x4Formatter : IYamlFormatter<uint2x4>
    {
        public static readonly UInt2x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint2x4 value, YamlSerializationContext context)
        {
            var r0 = new uint4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new uint4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint4>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            emitter.EndSequence();
        }

        public uint2x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new uint2x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w);
        }
    }

    public class UInt3x2Formatter : IYamlFormatter<uint3x2>
    {
        public static readonly UInt3x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint3x2 value, YamlSerializationContext context)
        {
            var r0 = new uint2(value.c0.x, value.c1.x);
            var r1 = new uint2(value.c0.y, value.c1.y);
            var r2 = new uint2(value.c0.z, value.c1.z);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public uint3x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new uint3x2(
                r0.x, r0.y,
                r1.x, r1.y,
                r2.x, r2.y);
        }
    }

    public class UInt3x3Formatter : IYamlFormatter<uint3x3>
    {
        public static readonly UInt3x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint3x3 value, YamlSerializationContext context)
        {
            var r0 = new uint3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new uint3(value.c0.y, value.c1.y, value.c2.y);
            var r2 = new uint3(value.c0.z, value.c1.z, value.c2.z);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint3>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public uint3x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new uint3x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z,
                r2.x, r2.y, r2.z);
        }
    }

    public class UInt3x4Formatter : IYamlFormatter<uint3x4>
    {
        public static readonly UInt3x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint3x4 value, YamlSerializationContext context)
        {
            var r0 = new uint4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new uint4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);
            var r2 = new uint4(value.c0.z, value.c1.z, value.c2.z, value.c3.z);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint4>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            emitter.EndSequence();
        }

        public uint3x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new uint3x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w,
                r2.x, r2.y, r2.z, r2.w);
        }
    }

    public class UInt4x2Formatter : IYamlFormatter<uint4x2>
    {
        public static readonly UInt4x2Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint4x2 value, YamlSerializationContext context)
        {
            var r0 = new uint2(value.c0.x, value.c1.x);
            var r1 = new uint2(value.c0.y, value.c1.y);
            var r2 = new uint2(value.c0.z, value.c1.z);
            var r3 = new uint2(value.c0.w, value.c1.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint2>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public uint4x2 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint2>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new uint4x2(
                r0.x, r0.y,
                r1.x, r1.y,
                r2.x, r2.y,
                r3.x, r3.y);
        }
    }

    public class UInt4x3Formatter : IYamlFormatter<uint4x3>
    {
        public static readonly UInt4x3Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint4x3 value, YamlSerializationContext context)
        {
            var r0 = new uint3(value.c0.x, value.c1.x, value.c2.x);
            var r1 = new uint3(value.c0.y, value.c1.y, value.c2.y);
            var r2 = new uint3(value.c0.z, value.c1.z, value.c2.z);
            var r3 = new uint3(value.c0.w, value.c1.w, value.c2.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint3>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public uint4x3 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint3>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new uint4x3(
                r0.x, r0.y, r0.z,
                r1.x, r1.y, r1.z,
                r2.x, r2.y, r2.z,
                r3.x, r3.y, r3.z);
        }
    }

    public class UInt4x4Formatter : IYamlFormatter<uint4x4>
    {
        public static readonly UInt4x4Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, uint4x4 value, YamlSerializationContext context)
        {
            var r0 = new uint4(value.c0.x, value.c1.x, value.c2.x, value.c3.x);
            var r1 = new uint4(value.c0.y, value.c1.y, value.c2.y, value.c3.y);
            var r2 = new uint4(value.c0.z, value.c1.z, value.c2.z, value.c3.z);
            var r3 = new uint4(value.c0.w, value.c1.w, value.c2.w, value.c3.w);

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint4>();
            emitter.BeginSequence();
            rowFormatter.Serialize(ref emitter, r0, context);
            rowFormatter.Serialize(ref emitter, r1, context);
            rowFormatter.Serialize(ref emitter, r2, context);
            rowFormatter.Serialize(ref emitter, r3, context);
            emitter.EndSequence();
        }

        public uint4x4 Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar()) return default;

            var rowFormatter = context.Resolver.GetFormatterWithVerify<uint4>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var r0 = rowFormatter.Deserialize(ref parser, context);
            var r1 = rowFormatter.Deserialize(ref parser, context);
            var r2 = rowFormatter.Deserialize(ref parser, context);
            var r3 = rowFormatter.Deserialize(ref parser, context);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            return new uint4x4(
                r0.x, r0.y, r0.z, r0.w,
                r1.x, r1.y, r1.z, r1.w,
                r2.x, r2.y, r2.z, r2.w,
                r3.x, r3.y, r3.z, r3.w);
        }
    }
}
#endif
