using System;
using System.Runtime.CompilerServices;

namespace VYaml.Parser
{
    public ref partial struct YamlParser
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNullScalar()
        {
            if (currentScalar is { } scalar)
                return scalar.IsNull();
            return false;
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
            if (currentScalar is { } scalar)
            {
                scalar.TryGetBool(out var value);
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect scalar value : {CurrentEventType}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly short GetScalarAsInt16()
        {
            if (currentScalar is { } scalar)
            {
                scalar.TryGetInt16(out var value);
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect scalar value : {CurrentEventType}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int GetScalarAsInt32()
        {
            if (currentScalar is { } scalar)
            {
                scalar.TryGetInt32(out var value);
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect scalar value : {CurrentEventType}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly long GetScalarAsInt64()
        {
            if (currentScalar is { } scalar)
            {
                scalar.TryGetInt64(out var value);
                return value;
            }
            throw new YamlParserException(CurrentMark, $"Cannot detect scalar value : {CurrentEventType}");
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
        public readonly bool TryGetScalarAsInt64(out long value)
        {
            if (currentScalar is { } scalar)
                return scalar.TryGetInt64(out value);
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