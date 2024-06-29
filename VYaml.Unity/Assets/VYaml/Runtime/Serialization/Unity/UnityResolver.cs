using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
#if VYAML_UNITY_RESOLVERS_ENABLE_MATHEMATICS
using Unity.Mathematics;
# endif
using VYaml.Serialization.Unity;

namespace VYaml.Serialization.Unity
{
    public class UnityResolver : IYamlFormatterResolver
    {
        public static readonly UnityResolver Instance = new();

        public static readonly Dictionary<Type, IYamlFormatter> FormatterMap = new()
        {
            { typeof(Color), ColorFormatter.Instance },
            { typeof(Color?), new StaticNullableFormatter<Color>(ColorFormatter.Instance) },
            { typeof(Color32), Color32Formatter.Instance },
            { typeof(Color32?), new StaticNullableFormatter<Color32>(Color32Formatter.Instance) },
            { typeof(Vector2), Vector2Formatter.Instance },
            { typeof(Vector2?), new StaticNullableFormatter<Vector2>(Vector2Formatter.Instance) },
            { typeof(Vector2Int), Vector2IntFormatter.Instance },
            { typeof(Vector2Int?), new StaticNullableFormatter<Vector2Int>(Vector2IntFormatter.Instance) },
            { typeof(Vector3), Vector3Formatter.Instance },
            { typeof(Vector3?), new StaticNullableFormatter<Vector3>(Vector3Formatter.Instance) },
            { typeof(Vector3Int), Vector3IntFormatter.Instance },
            { typeof(Vector3Int?), new StaticNullableFormatter<Vector3Int>(Vector3IntFormatter.Instance) },
            { typeof(Vector4), Vector4Formatter.Instance },
            { typeof(Vector4?), new StaticNullableFormatter<Vector4>(Vector4Formatter.Instance) },
            { typeof(Matrix4x4), Matrix4x4Formatter.Instance },
            { typeof(Matrix4x4?), new StaticNullableFormatter<Matrix4x4>(Matrix4x4Formatter.Instance) },
            { typeof(Quaternion), QuaternionFormatter.Instance },
            { typeof(Quaternion?), new StaticNullableFormatter<Quaternion>(QuaternionFormatter.Instance) },

#if UNITY_2022_2_OR_NEWER
            { typeof(RefreshRate), RefreshRateFormatter.Instance },
            { typeof(RefreshRate?), new StaticNullableFormatter<RefreshRate>(RefreshRateFormatter.Instance) },
#endif
            { typeof(Resolution), ResolutionFormatter.Instance },
            { typeof(Resolution?), new StaticNullableFormatter<Resolution>(ResolutionFormatter.Instance) },
            { typeof(Texture3D), Texture3DFormatter.Instance },

            { typeof(Hash128), Hash128Formatter.Instance },
            { typeof(Hash128?), new StaticNullableFormatter<Hash128>(Hash128Formatter.Instance) },

            { typeof(Bounds), BoundsFormatter.Instance },
            { typeof(Bounds?), new StaticNullableFormatter<Bounds>(BoundsFormatter.Instance) },
            { typeof(BoundsInt), BoundsIntFormatter.Instance },
            { typeof(BoundsInt?), new StaticNullableFormatter<BoundsInt>(BoundsIntFormatter.Instance) },
            { typeof(Plane), PlaneFormatter.Instance },
            { typeof(Plane?), new StaticNullableFormatter<Plane>(PlaneFormatter.Instance) },
            { typeof(Rect), RectFormatter.Instance },
            { typeof(Rect?), new StaticNullableFormatter<Rect>(RectFormatter.Instance) },
            { typeof(RectInt), RectIntFormatter.Instance },
            { typeof(RectInt?), new StaticNullableFormatter<RectInt>(RectIntFormatter.Instance) },
            { typeof(RectOffset), RectOffsetFormatter.Instance },

            { typeof(NativeArray<byte>), NativeByteArrayFormatter.Instance },
            { typeof(NativeArray<byte>?), new StaticNullableFormatter<NativeArray<byte>>(NativeByteArrayFormatter.Instance) },

#if VYAML_UNITY_RESOLVERS_ENABLE_ULID
      { typeof(Ulid), UlidFormatter.Instance },
      { typeof(Ulid?), new StaticNullableFormatter<Ulid>(UlidFormatter.Instance) },
#endif

#if VYAML_UNITY_RESOLVERS_ENABLE_MATHEMATICS
      { typeof(quaternion), Formatters.Mathematics.QuaternionFormatter.Instance },
      {
        typeof(quaternion?),
        new StaticNullableFormatter<quaternion>(Formatters.Mathematics.QuaternionFormatter.Instance)
      },

      { typeof(bool2), Bool2Formatter.Instance },
      { typeof(bool2?), new StaticNullableFormatter<bool2>(Bool2Formatter.Instance) },
      { typeof(bool3), Bool3Formatter.Instance },
      { typeof(bool3?), new StaticNullableFormatter<bool3>(Bool3Formatter.Instance) },
      { typeof(bool4), Bool4Formatter.Instance },
      { typeof(bool4?), new StaticNullableFormatter<bool4>(Bool4Formatter.Instance) },
      { typeof(double2), Double2Formatter.Instance },
      { typeof(double2?), new StaticNullableFormatter<double2>(Double2Formatter.Instance) },
      { typeof(double3), Double3Formatter.Instance },
      { typeof(double3?), new StaticNullableFormatter<double3>(Double3Formatter.Instance) },
      { typeof(double4), Double4Formatter.Instance },
      { typeof(double4?), new StaticNullableFormatter<double4>(Double4Formatter.Instance) },
      { typeof(float2), Float2Formatter.Instance },
      { typeof(float2?), new StaticNullableFormatter<float2>(Float2Formatter.Instance) },
      { typeof(float3), Float3Formatter.Instance },
      { typeof(float3?), new StaticNullableFormatter<float3>(Float3Formatter.Instance) },
      { typeof(float4), Float4Formatter.Instance },
      { typeof(float4?), new StaticNullableFormatter<float4>(Float4Formatter.Instance) },
      { typeof(half2), Half2Formatter.Instance },
      { typeof(half2?), new StaticNullableFormatter<half2>(Half2Formatter.Instance) },
      { typeof(half3), Half3Formatter.Instance },
      { typeof(half3?), new StaticNullableFormatter<half3>(Half3Formatter.Instance) },
      { typeof(half4), Half4Formatter.Instance },
      { typeof(half4?), new StaticNullableFormatter<half4>(Half4Formatter.Instance) },
      { typeof(int2), Int2Formatter.Instance },
      { typeof(int2?), new StaticNullableFormatter<int2>(Int2Formatter.Instance) },
      { typeof(int3), Int3Formatter.Instance },
      { typeof(int3?), new StaticNullableFormatter<int3>(Int3Formatter.Instance) },
      { typeof(int4), Int4Formatter.Instance },
      { typeof(int4?), new StaticNullableFormatter<int4>(Int4Formatter.Instance) },
      { typeof(uint2), UInt2Formatter.Instance },
      { typeof(uint2?), new StaticNullableFormatter<uint2>(UInt2Formatter.Instance) },
      { typeof(uint3), UInt3Formatter.Instance },
      { typeof(uint3?), new StaticNullableFormatter<uint3>(UInt3Formatter.Instance) },
      { typeof(uint4), UInt4Formatter.Instance },
      { typeof(uint4?), new StaticNullableFormatter<uint4>(UInt4Formatter.Instance) },

      { typeof(bool2x2), Bool2x2Formatter.Instance },
      { typeof(bool2x2?), new StaticNullableFormatter<bool2x2>(Bool2x2Formatter.Instance) },
      { typeof(bool2x3), Bool2x3Formatter.Instance },
      { typeof(bool2x3?), new StaticNullableFormatter<bool2x3>(Bool2x3Formatter.Instance) },
      { typeof(bool2x4), Bool2x4Formatter.Instance },
      { typeof(bool2x4?), new StaticNullableFormatter<bool2x4>(Bool2x4Formatter.Instance) },
      { typeof(bool3x2), Bool3x2Formatter.Instance },
      { typeof(bool3x2?), new StaticNullableFormatter<bool3x2>(Bool3x2Formatter.Instance) },
      { typeof(bool3x3), Bool3x3Formatter.Instance },
      { typeof(bool3x3?), new StaticNullableFormatter<bool3x3>(Bool3x3Formatter.Instance) },
      { typeof(bool3x4), Bool3x4Formatter.Instance },
      { typeof(bool3x4?), new StaticNullableFormatter<bool3x4>(Bool3x4Formatter.Instance) },
      { typeof(bool4x2), Bool4x2Formatter.Instance },
      { typeof(bool4x2?), new StaticNullableFormatter<bool4x2>(Bool4x2Formatter.Instance) },
      { typeof(bool4x3), Bool4x3Formatter.Instance },
      { typeof(bool4x3?), new StaticNullableFormatter<bool4x3>(Bool4x3Formatter.Instance) },
      { typeof(bool4x4), Bool4x4Formatter.Instance },
      { typeof(bool4x4?), new StaticNullableFormatter<bool4x4>(Bool4x4Formatter.Instance) },
      { typeof(double2x2), Double2x2Formatter.Instance },
      { typeof(double2x2?), new StaticNullableFormatter<double2x2>(Double2x2Formatter.Instance) },
      { typeof(double2x3), Double2x3Formatter.Instance },
      { typeof(double2x3?), new StaticNullableFormatter<double2x3>(Double2x3Formatter.Instance) },
      { typeof(double2x4), Double2x4Formatter.Instance },
      { typeof(double2x4?), new StaticNullableFormatter<double2x4>(Double2x4Formatter.Instance) },
      { typeof(double3x2), Double3x2Formatter.Instance },
      { typeof(double3x2?), new StaticNullableFormatter<double3x2>(Double3x2Formatter.Instance) },
      { typeof(double3x3), Double3x3Formatter.Instance },
      { typeof(double3x3?), new StaticNullableFormatter<double3x3>(Double3x3Formatter.Instance) },
      { typeof(double3x4), Double3x4Formatter.Instance },
      { typeof(double3x4?), new StaticNullableFormatter<double3x4>(Double3x4Formatter.Instance) },
      { typeof(double4x2), Double4x2Formatter.Instance },
      { typeof(double4x2?), new StaticNullableFormatter<double4x2>(Double4x2Formatter.Instance) },
      { typeof(double4x3), Double4x3Formatter.Instance },
      { typeof(double4x3?), new StaticNullableFormatter<double4x3>(Double4x3Formatter.Instance) },
      { typeof(double4x4), Double4x4Formatter.Instance },
      { typeof(double4x4?), new StaticNullableFormatter<double4x4>(Double4x4Formatter.Instance) },
      { typeof(float2x2), Float2x2Formatter.Instance },
      { typeof(float2x2?), new StaticNullableFormatter<float2x2>(Float2x2Formatter.Instance) },
      { typeof(float2x3), Float2x3Formatter.Instance },
      { typeof(float2x3?), new StaticNullableFormatter<float2x3>(Float2x3Formatter.Instance) },
      { typeof(float2x4), Float2x4Formatter.Instance },
      { typeof(float2x4?), new StaticNullableFormatter<float2x4>(Float2x4Formatter.Instance) },
      { typeof(float3x2), Float3x2Formatter.Instance },
      { typeof(float3x2?), new StaticNullableFormatter<float3x2>(Float3x2Formatter.Instance) },
      { typeof(float3x3), Float3x3Formatter.Instance },
      { typeof(float3x3?), new StaticNullableFormatter<float3x3>(Float3x3Formatter.Instance) },
      { typeof(float3x4), Float3x4Formatter.Instance },
      { typeof(float3x4?), new StaticNullableFormatter<float3x4>(Float3x4Formatter.Instance) },
      { typeof(float4x2), Float4x2Formatter.Instance },
      { typeof(float4x2?), new StaticNullableFormatter<float4x2>(Float4x2Formatter.Instance) },
      { typeof(float4x3), Float4x3Formatter.Instance },
      { typeof(float4x3?), new StaticNullableFormatter<float4x3>(Float4x3Formatter.Instance) },
      { typeof(float4x4), Float4x4Formatter.Instance },
      { typeof(float4x4?), new StaticNullableFormatter<float4x4>(Float4x4Formatter.Instance) },
      { typeof(int2x2), Int2x2Formatter.Instance },
      { typeof(int2x2?), new StaticNullableFormatter<int2x2>(Int2x2Formatter.Instance) },
      { typeof(int2x3), Int2x3Formatter.Instance },
      { typeof(int2x3?), new StaticNullableFormatter<int2x3>(Int2x3Formatter.Instance) },
      { typeof(int2x4), Int2x4Formatter.Instance },
      { typeof(int2x4?), new StaticNullableFormatter<int2x4>(Int2x4Formatter.Instance) },
      { typeof(int3x2), Int3x2Formatter.Instance },
      { typeof(int3x2?), new StaticNullableFormatter<int3x2>(Int3x2Formatter.Instance) },
      { typeof(int3x3), Int3x3Formatter.Instance },
      { typeof(int3x3?), new StaticNullableFormatter<int3x3>(Int3x3Formatter.Instance) },
      { typeof(int3x4), Int3x4Formatter.Instance },
      { typeof(int3x4?), new StaticNullableFormatter<int3x4>(Int3x4Formatter.Instance) },
      { typeof(int4x2), Int4x2Formatter.Instance },
      { typeof(int4x2?), new StaticNullableFormatter<int4x2>(Int4x2Formatter.Instance) },
      { typeof(int4x3), Int4x3Formatter.Instance },
      { typeof(int4x3?), new StaticNullableFormatter<int4x3>(Int4x3Formatter.Instance) },
      { typeof(int4x4), Int4x4Formatter.Instance },
      { typeof(int4x4?), new StaticNullableFormatter<int4x4>(Int4x4Formatter.Instance) },
      { typeof(uint2x2), UInt2x2Formatter.Instance },
      { typeof(uint2x2?), new StaticNullableFormatter<uint2x2>(UInt2x2Formatter.Instance) },
      { typeof(uint2x3), UInt2x3Formatter.Instance },
      { typeof(uint2x3?), new StaticNullableFormatter<uint2x3>(UInt2x3Formatter.Instance) },
      { typeof(uint2x4), UInt2x4Formatter.Instance },
      { typeof(uint2x4?), new StaticNullableFormatter<uint2x4>(UInt2x4Formatter.Instance) },
      { typeof(uint3x2), UInt3x2Formatter.Instance },
      { typeof(uint3x2?), new StaticNullableFormatter<uint3x2>(UInt3x2Formatter.Instance) },
      { typeof(uint3x3), UInt3x3Formatter.Instance },
      { typeof(uint3x3?), new StaticNullableFormatter<uint3x3>(UInt3x3Formatter.Instance) },
      { typeof(uint3x4), UInt3x4Formatter.Instance },
      { typeof(uint3x4?), new StaticNullableFormatter<uint3x4>(UInt3x4Formatter.Instance) },
      { typeof(uint4x2), UInt4x2Formatter.Instance },
      { typeof(uint4x2?), new StaticNullableFormatter<uint4x2>(UInt4x2Formatter.Instance) },
      { typeof(uint4x3), UInt4x3Formatter.Instance },
      { typeof(uint4x3?), new StaticNullableFormatter<uint4x3>(UInt4x3Formatter.Instance) },
      { typeof(uint4x4), UInt4x4Formatter.Instance },
      { typeof(uint4x4?), new StaticNullableFormatter<uint4x4>(UInt4x4Formatter.Instance) },

#endif
        };

        public static readonly Dictionary<Type, Type> KnownGenericTypes = new()
        {
            { typeof(NativeArray<>), typeof(NativeArrayFormatter<>) }
        };

        public IYamlFormatter<T>? GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        static object? TryCreateGenericFormatter(Type type)
        {
            Type? formatterType = null;

            if (type.IsArray)
            {
                if (type.IsSZArray)
                {
                    formatterType = typeof(ArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                }
                else
                {
                    var rank = type.GetArrayRank();
                    //switch (rank)
                    //{
                    // case 2:
                    //     formatterType = typeof(TwoDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                    //     break;
                    // case 3:
                    //     formatterType = typeof(ThreeDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                    //     break;
                    // case 4:
                    //     formatterType = typeof(FourDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                    //     break;
                    // default:
                    //     break; // not supported
                    //}
                }
            }
            else if (type.IsEnum)
            {
                formatterType = typeof(EnumAsStringFormatter<>).MakeGenericType(type);
            }
            else
            {
                formatterType = TryCreateGenericFormatterType(type, KnownGenericTypes);
            }

            if (formatterType != null) return Activator.CreateInstance(formatterType);
            return null;
        }

        private static Type? TryCreateGenericFormatterType(Type type, IDictionary<Type, Type> knownTypes)
        {
            if (type.IsGenericType)
            {
                var genericDefinition = type.GetGenericTypeDefinition();

                if (knownTypes.TryGetValue(genericDefinition, out var formatterType))
                    return formatterType.MakeGenericType(type.GetGenericArguments());
            }

            return null;
        }

        private static class FormatterCache<T>
        {
            public static readonly IYamlFormatter<T>? Formatter;

            static FormatterCache()
            {
                if (FormatterMap.TryGetValue(typeof(T), out var formatter) && formatter is IYamlFormatter<T> value)
                {
                    Formatter = value;
                    return;
                }

                if (TryCreateGenericFormatter(typeof(T)) is IYamlFormatter<T> f)
                {
                    Formatter = f;
                    return;
                }

                Formatter = null;
            }
        }
    }
}
