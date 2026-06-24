#nullable enable
using System;
using System.Globalization;
using VYaml.Emitter;
using VYaml.Internal;

namespace VYaml.Parser
{
    /// <summary>
    /// A scalar leaf of a <see cref="YamlNode"/> tree.
    /// </summary>
    /// <remarks>
    /// The raw UTF-8 content is retained so typed values (<see cref="GetInt32"/>, <see cref="GetBool"/>, ...)
    /// are resolved lazily using the same rules as the streaming parser.
    /// </remarks>
    public sealed class YamlScalarNode : YamlNode
    {
        // Owns its bytes (a private copy, never returned to any pool).
        readonly Scalar scalar;

        internal YamlScalarNode(ReadOnlySpan<byte> utf8, TokenType tokenType)
        {
            scalar = new Scalar(utf8) { Type = tokenType };
        }

        public YamlScalarNode(string? value, ScalarStyle style = ScalarStyle.Any)
            : this(value is null ? default : StringEncoding.Utf8.GetBytes(value), ToTokenType(style))
        {
        }

        public YamlScalarNode(bool value) : this(value ? "true" : "false", ScalarStyle.Plain)
        {
        }

        public YamlScalarNode(int value) : this(value.ToString(CultureInfo.InvariantCulture), ScalarStyle.Plain)
        {
        }

        public YamlScalarNode(long value) : this(value.ToString(CultureInfo.InvariantCulture), ScalarStyle.Plain)
        {
        }

        public YamlScalarNode(uint value) : this(value.ToString(CultureInfo.InvariantCulture), ScalarStyle.Plain)
        {
        }

        public YamlScalarNode(ulong value) : this(value.ToString(CultureInfo.InvariantCulture), ScalarStyle.Plain)
        {
        }

        public YamlScalarNode(float value) : this(value.ToString("R", CultureInfo.InvariantCulture), ScalarStyle.Plain)
        {
        }

        public YamlScalarNode(double value) : this(value.ToString("R", CultureInfo.InvariantCulture), ScalarStyle.Plain)
        {
        }

        public override YamlNodeType NodeType => YamlNodeType.Scalar;

        /// <summary>
        /// The presentation style this scalar was parsed with (or will be emitted with).
        /// </summary>
        public ScalarStyle Style
        {
            get => ToStyle(scalar.Type);
            set => scalar.Type = ToTokenType(value);
        }

        /// <summary>
        /// The decoded textual value of this scalar.
        /// </summary>
        public string Value => scalar.ToString();

        /// <summary>
        /// True when this scalar represents YAML <c>null</c> (<c>null</c>, <c>~</c>, or empty, unquoted).
        /// </summary>
        public bool IsNull => scalar.IsNull();

        internal Scalar Scalar => scalar;

        public bool TryGetValue(out bool value) => scalar.TryGetBool(out value);
        public bool TryGetValue(out int value) => scalar.TryGetInt32(out value);
        public bool TryGetValue(out long value) => scalar.TryGetInt64(out value);
        public bool TryGetValue(out uint value) => scalar.TryGetUInt32(out value);
        public bool TryGetValue(out ulong value) => scalar.TryGetUInt64(out value);
        public bool TryGetValue(out float value) => scalar.TryGetFloat(out value);
        public bool TryGetValue(out double value) => scalar.TryGetDouble(out value);

        public bool GetBool() => Get<bool>(scalar.TryGetBool);
        public int GetInt32() => Get<int>(scalar.TryGetInt32);
        public long GetInt64() => Get<long>(scalar.TryGetInt64);
        public uint GetUInt32() => Get<uint>(scalar.TryGetUInt32);
        public ulong GetUInt64() => Get<ulong>(scalar.TryGetUInt64);
        public float GetSingle() => Get<float>(scalar.TryGetFloat);
        public double GetDouble() => Get<double>(scalar.TryGetDouble);
        public string? GetString() => IsNull ? null : Value;

        /// <summary>
        /// Resolves this scalar as <typeparamref name="T"/> using YAML scalar rules.
        /// Supported: <see cref="string"/>, <see cref="bool"/>, the integer types, <see cref="float"/>, <see cref="double"/>.
        /// </summary>
        public T GetValue<T>()
        {
            if (typeof(T) == typeof(string)) return (T)(object)Value;
            if (typeof(T) == typeof(bool)) return (T)(object)GetBool();
            if (typeof(T) == typeof(int)) return (T)(object)GetInt32();
            if (typeof(T) == typeof(long)) return (T)(object)GetInt64();
            if (typeof(T) == typeof(uint)) return (T)(object)GetUInt32();
            if (typeof(T) == typeof(ulong)) return (T)(object)GetUInt64();
            if (typeof(T) == typeof(float)) return (T)(object)GetSingle();
            if (typeof(T) == typeof(double)) return (T)(object)GetDouble();
            throw new InvalidOperationException($"Cannot get a scalar value of type: {typeof(T)}");
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj)
        {
            return obj is YamlScalarNode other && scalar.SequenceEqual(other.scalar);
        }

        public override int GetHashCode()
        {
            return ByteSequenceHash.GetHashCode(scalar.AsSpan());
        }

        delegate bool TryParse<T>(out T value);

        static T Get<T>(TryParse<T> tryParse)
        {
            if (tryParse(out var value))
            {
                return value;
            }
            throw new InvalidOperationException($"Cannot get a scalar value of type: {typeof(T)}");
        }

        static ScalarStyle ToStyle(TokenType tokenType) => tokenType switch
        {
            TokenType.SingleQuotedScaler => ScalarStyle.SingleQuoted,
            TokenType.DoubleQuotedScaler => ScalarStyle.DoubleQuoted,
            TokenType.LiteralScalar => ScalarStyle.Literal,
            TokenType.FoldedScalar => ScalarStyle.Folded,
            _ => ScalarStyle.Plain,
        };

        static TokenType ToTokenType(ScalarStyle style) => style switch
        {
            ScalarStyle.SingleQuoted => TokenType.SingleQuotedScaler,
            ScalarStyle.DoubleQuoted => TokenType.DoubleQuotedScaler,
            ScalarStyle.Literal => TokenType.LiteralScalar,
            ScalarStyle.Folded => TokenType.FoldedScalar,
            _ => TokenType.PlainScalar,
        };
    }
}
