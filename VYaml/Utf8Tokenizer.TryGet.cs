using System.Runtime.CompilerServices;

namespace VYaml
{
    public ref partial struct Utf8Tokenizer
    {
        /// <summary>
        /// </summary>
        /// <remarks>
        /// null | Null | NULL | ~
        /// </remarks>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNull() => currentToken.Scalar?.IsNull() ?? true;

        /// <summary>
        /// </summary>
        /// <remarks>
        /// tag:yaml.org,2002:bool
        /// true | True | TRUE | false | False | FALSE
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetBool(out bool value)
        {
            if (currentToken.Scalar is { } scalar)
                return scalar.TryGetBool(out value);

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetInt64(out long value)
        {
            if (currentToken.Scalar is { } scalar)
                return scalar.TryGetInt64(out value);

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetInt32(out int value)
        {
            if (currentToken.Scalar is { } scalar)
                return scalar.TryGetInt32(out value);

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetDouble(out double value)
        {
            if (currentToken.Scalar is { } scalar)
                return scalar.TryGetDouble(out value);

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string GetString()
        {
            if (currentToken.Scalar is { } scalar)
            {
                return scalar.ToString();
            }
            return "";
        }
    }
}
