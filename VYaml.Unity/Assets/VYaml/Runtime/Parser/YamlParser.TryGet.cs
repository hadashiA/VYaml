#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace VYaml.Parser
{
    public ref partial struct YamlParser
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNullScalar()
        {
            return CurrentEventType == ParseEventType.Scalar &&
                   (currentScalar == null ||
                    currentScalar.IsNull());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string? GetScalarAsString()
        {
            return currentScalar?.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetScalarAsSpan(out ReadOnlySpan<byte> span)
        {
            if (currentScalar is null)
            {
                span = default;
                return false;
            }
            span = currentScalar.AsSpan();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool GetScalarAsBool()
        {
            if (currentScalar is { } scalar && scalar.TryGetBool(out var value))
            {
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect a scalar value as bool : {CurrentEventType} {currentScalar}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int GetScalarAsInt32()
        {
            if (currentScalar is { } scalar && scalar.TryGetInt32(out var value))
            {
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect a scalar value as Int32: {CurrentEventType} {currentScalar}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly long GetScalarAsInt64()
        {
            if (currentScalar is { } scalar && scalar.TryGetInt64(out var value))
            {
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect a scalar value as Int64: {CurrentEventType} {currentScalar}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly uint GetScalarAsUInt32()
        {
            if (currentScalar is { } scalar && scalar.TryGetUInt32(out var value))
            {
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect a scalar value as UInt32 : {CurrentEventType} {currentScalar}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ulong GetScalarAsUInt64()
        {
            if (currentScalar is { } scalar && scalar.TryGetUInt64(out var value))
            {
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect a scalar value as UInt64 : {CurrentEventType} ({currentScalar})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float GetScalarAsFloat()
        {
            if (currentScalar is { } scalar && scalar.TryGetFloat(out var value))
            {
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect scalar value as float : {CurrentEventType} {currentScalar}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly double GetScalarAsDouble()
        {
            if (currentScalar is { } scalar && scalar.TryGetDouble(out var value))
            {
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect a scalar value as double : {CurrentEventType} {currentScalar}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? ReadScalarAsString()
        {
            var result = currentScalar?.ToString();
            ReadWithVerify(ParseEventType.Scalar);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadScalarAsBool()
        {
            var result = GetScalarAsBool();
            ReadWithVerify(ParseEventType.Scalar);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadScalarAsInt32()
        {
            var result = GetScalarAsInt32();
            ReadWithVerify(ParseEventType.Scalar);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadScalarAsInt64()
        {
            var result = GetScalarAsInt64();
            ReadWithVerify(ParseEventType.Scalar);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadScalarAsUInt32()
        {
            var result = GetScalarAsUInt32();
            ReadWithVerify(ParseEventType.Scalar);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadScalarAsUInt64()
        {
            var result = GetScalarAsUInt64();
            ReadWithVerify(ParseEventType.Scalar);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadScalarAsFloat()
        {
            var result = GetScalarAsFloat();
            ReadWithVerify(ParseEventType.Scalar);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadScalarAsDouble()
        {
            var result = GetScalarAsDouble();
            ReadWithVerify(ParseEventType.Scalar);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetScalarAsString(out string? value)
        {
            if (currentScalar is { } scalar)
            {
                value = scalar.IsNull() ? null : scalar.ToString();
                return true;
            }
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetScalarAsBool(out bool value)
        {
            if (currentScalar is { } scalar)
                return scalar.TryGetBool(out value);
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetScalarAsInt32(out int value)
        {
            if (currentScalar is { } scalar)
                return scalar.TryGetInt32(out value);
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetScalarAsInt64(out long value)
        {
            if (currentScalar is { } scalar)
                return scalar.TryGetInt64(out value);
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetScalarAsFloat(out float value)
        {
            if (currentScalar is { } scalar)
                return scalar.TryGetFloat(out value);
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetScalarAsDouble(out double value)
        {
            if (currentScalar is { } scalar)
                return scalar.TryGetDouble(out value);
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetCurrentTag(out Tag tag)
        {
            if (currentTag != null)
            {
                tag = currentTag;
                return true;
            }
            tag = default!;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetCurrentAnchor(out Anchor anchor)
        {
            if (currentAnchor != null)
            {
                anchor = currentAnchor;
                return true;
            }
            anchor = default!;
            return false;
        }
    }
}
