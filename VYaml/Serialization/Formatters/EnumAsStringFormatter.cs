using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Internal;
using VYaml.Parser;

namespace VYaml.Serialization
{
    // TODO:
    static class EnumAsStringNonGenericHelper
    {
        static readonly ConcurrentDictionary<object, string?> AliasStringValues = new();
        static readonly ConcurrentDictionary<Type, NamingConvention?> NamingConventionsByType = new();

        static readonly Func<object, Type, string?> AliasStringValueFactory = AnalyzeAliasStringValue;
        static readonly Func<Type, NamingConvention?> NamingConventionFactory = AnalyzeNamingConventionByType;

        public static string? GetAliasStringValue(Type type, object value) => AliasStringValues.GetOrAdd(value, AliasStringValueFactory!, type);
        public static NamingConvention? GetNamingConventionByType(Type type) => NamingConventionsByType.GetOrAdd(type, NamingConventionFactory);

        public static void Serialize(ref Utf8YamlEmitter emitter, Type type, object value, YamlSerializationContext context)
        {
            var aliasStringValue = GetAliasStringValue(type, value);
            if (aliasStringValue != null)
            {
                emitter.WriteString(aliasStringValue);
                return;
            }

            var name = Enum.GetName(type, value)!;
            var namingConvention = GetNamingConventionByType(type) ?? context.Options.NamingConvention;
            var mutator = NamingConventionMutator.Of(namingConvention);
            Span<char> destination = stackalloc char[name.Length * 2];
            int written;
            while (!mutator.TryMutate(name.AsSpan(), destination, out written))
            {
                // ReSharper disable once StackAllocInsideLoop
                destination = stackalloc char[destination.Length * 2];
            }

            emitter.WriteString(destination[..written].ToString());
        }

        static NamingConvention? AnalyzeNamingConventionByType(Type type)
        {
            return type.GetCustomAttribute<YamlObjectAttribute>()?.NamingConvention;
        }

        static string? AnalyzeAliasStringValue(object value, Type type)
        {
            var name = Enum.GetName(type, value)!;
            var fieldInfo = type.GetField(name)!;

            var attributes = fieldInfo.GetCustomAttributes(inherit: true);
            if (attributes.OfType<EnumMemberAttribute>().FirstOrDefault() is { Value: { } enumMemberValue })
            {
                return enumMemberValue;
            }
            if (attributes.OfType<DataMemberAttribute>().FirstOrDefault() is { Name: { } dataMemberName })
            {
                return dataMemberName;
            }
            return null;
        }
    }

    public class EnumAsStringFormatter<T> : IYamlFormatter<T> where T : unmanaged, Enum
    {
        // ReSharper disable once StaticMemberInGenericType
        static readonly NamingConvention? NamingConventionByType;

        // ReSharper disable once StaticMemberInGenericType
        static readonly bool IsFlags = typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false);

        static readonly Dictionary<T, (string Value, bool Alias, byte[]? Utf8Plain)> StringValues = new();
        static readonly Dictionary<string, T> Values = new();

        // Defined members carrying at least one bit, sorted by bit value descending.
        // Used to decompose a composite [Flags] value into its named members.
        static readonly (ulong Bits, T Value)[] FlagsDescending;

        static EnumAsStringFormatter()
        {
            var type = typeof(T);
            NamingConventionByType = EnumAsStringNonGenericHelper.GetNamingConventionByType(type);

            foreach (var item in type.GetFields())
            {
                if (item.FieldType != type) continue;

                var value = item.GetValue(null)!;
                var aliasValue = EnumAsStringNonGenericHelper.GetAliasStringValue(type, value);
                if (aliasValue != null)
                {
                    StringValues.Add((T)value, (aliasValue, true, TryEncodePlainUtf8(aliasValue)));
                    Values.Add(aliasValue, (T)value);
                }
                else
                {
                    var mutator = NamingConventionMutator.Of(NamingConventionByType ?? YamlSerializerOptions.DefaultNamingConvention);
                    var name = Enum.GetName(type, value)!;
                    // ReSharper disable once StackAllocInsideLoop
                    Span<char> destination = stackalloc char[name.Length];
                    int written;
                    while (!mutator.TryMutate(name.AsSpan(), destination, out written))
                    {
                        // ReSharper disable once StackAllocInsideLoop
                        destination = stackalloc char[destination.Length * 2];
                    }

                    var stringValue = destination[..written].ToString();
                    StringValues.Add((T)value, (stringValue, false, TryEncodePlainUtf8(stringValue)));
                    Values.Add(stringValue, (T)value);
                }
            }

            if (IsFlags)
            {
                var ordered = new List<(ulong Bits, T Value)>(StringValues.Count);
                foreach (var entry in StringValues)
                {
                    var bits = ToUInt64(entry.Key);
                    if (bits != 0)
                    {
                        ordered.Add((bits, entry.Key));
                    }
                }
                ordered.Sort((x, y) => y.Bits.CompareTo(x.Bits));
                FlagsDescending = ordered.ToArray();
            }
            else
            {
                FlagsDescending = [];
            }
        }

        public void Serialize(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
        {
            if (!StringValues.TryGetValue(value, out var t))
            {
                if (IsFlags)
                {
                    SerializeFlags(ref emitter, value, context);
                    return;
                }

                YamlSerializerException.ThrowInvalidType<T>(value.ToString());
                return;
            }

            var (stringValue, alias, utf8Plain) = t;
            if (alias || context.Options.NamingConvention == (NamingConventionByType ?? YamlSerializerOptions.DefaultNamingConvention))
            {
                if (utf8Plain != null)
                {
                    emitter.WriteScalar(utf8Plain);
                }
                else
                {
                    // Needs quoting/escaping; let the emitter analyze and quote it.
                    emitter.WriteString(stringValue);
                }
                return;
            }

            var mutator = NamingConventionMutator.Of(NamingConventionByType ?? context.Options.NamingConvention);
            Span<char> buffer = stackalloc char[stringValue.Length];

            int bytesWritten;
            while (!mutator.TryMutate(stringValue.AsSpan(), buffer, out bytesWritten))
            {
                // ReSharper disable once StackAllocInsideLoop
                buffer = stackalloc char[buffer.Length * 2];
            }

            unsafe
            {
                fixed (char* ptr = buffer)
                {
                    emitter.WriteString(ptr, bytesWritten);
                }
            }
        }

        public T Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            // ReadScalarAsString advances the parser past the scalar event.
            var scalar = parser.ReadScalarAsString();
            if (scalar == null)
            {
                YamlSerializerException.ThrowInvalidType<T>("null");
                return default!;
            }

            if (Values.TryGetValue(scalar, out var value))
            {
                return value;
            }

            var mutator = NamingConventionMutator.Of(NamingConventionByType ?? YamlSerializerOptions.DefaultNamingConvention);
            Span<char> buffer = stackalloc char[scalar.Length];
            int bytesWritten;
            while (!mutator.TryMutate(scalar.AsSpan(), buffer, out bytesWritten))
            {
                // ReSharper disable once StackAllocInsideLoop
                buffer = stackalloc char[buffer.Length * 2];
            }

            var mutatedScalar = buffer[..bytesWritten].ToString();
            if (Values.TryGetValue(mutatedScalar, out value) ||
                IsFlags && TryDeserializeFlags(scalar, out value))
            {
                return value;
            }

            YamlSerializerException.ThrowInvalidType<T>(scalar);
            return default!;
        }

        static void SerializeFlags(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
        {
            var remaining = ToUInt64(value);

            // No bit set and no exact name match: emit the underlying number, matching Enum.ToString().
            if (remaining == 0)
            {
                emitter.WriteString("0");
                return;
            }

            // Greedily cover the bits with the largest defined members first, recording
            // which members were selected (indices into FlagsDescending) on the stack.
            var matched = FlagsDescending.Length <= 64
                ? stackalloc int[FlagsDescending.Length]
                : new int[FlagsDescending.Length];
            var count = 0;
            for (var i = 0; i < FlagsDescending.Length; i++)
            {
                var bits = FlagsDescending[i].Bits;
                if ((remaining & bits) == bits)
                {
                    matched[count++] = i;
                    remaining &= ~bits;
                    if (remaining == 0)
                    {
                        break;
                    }
                }
            }

            // Leftover bits that no defined flag covers cannot be represented as names.
            if (remaining != 0)
            {
                YamlSerializerException.ThrowInvalidType<T>(value.ToString());
                return;
            }

            // Assemble "a | b | c" into a pooled buffer (no intermediate string allocations).
            // The " | " separator keeps the scalar plain (a ',' would force quoting).
            // ResolveName returns the cached member string in the common case.
            var length = 3 * (count - 1);
            for (var k = 0; k < count; k++)
            {
                length += ResolveName(FlagsDescending[matched[k]].Value, context).Length;
            }

            var rented = ArrayPool<char>.Shared.Rent(length);
            try
            {
                var buffer = rented.AsSpan(0, length);
                var position = 0;
                // Emit in ascending bit order (reverse of the greedy selection) to match
                // Enum.ToString() conventions.
                for (var k = count - 1; k >= 0; k--)
                {
                    if (position > 0)
                    {
                        buffer[position++] = ' ';
                        buffer[position++] = '|';
                        buffer[position++] = ' ';
                    }
                    var name = ResolveName(FlagsDescending[matched[k]].Value, context);
                    name.AsSpan().CopyTo(buffer[position..]);
                    position += name.Length;
                }
                emitter.WriteString(buffer);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }

        // Resolves the serialized name of a single defined member, honoring its alias
        // and the active naming convention (mirrors the scalar Serialize path).
        static string ResolveName(T value, YamlSerializationContext context)
        {
            var (stringValue, alias, _) = StringValues[value];
            if (alias || context.Options.NamingConvention == (NamingConventionByType ?? YamlSerializerOptions.DefaultNamingConvention))
            {
                return stringValue;
            }

            var mutator = NamingConventionMutator.Of(NamingConventionByType ?? context.Options.NamingConvention);
            Span<char> buffer = stackalloc char[stringValue.Length];
            int written;
            while (!mutator.TryMutate(stringValue.AsSpan(), buffer, out written))
            {
                // ReSharper disable once StackAllocInsideLoop
                buffer = stackalloc char[buffer.Length * 2];
            }
            return buffer[..written].ToString();
        }

        // Parses a composite flags scalar such as "A, B" or "A | B" by OR-ing each component.
        static bool TryDeserializeFlags(string scalar, out T result)
        {
            ulong accumulated = 0;
            var start = 0;
            for (var i = 0; i <= scalar.Length; i++)
            {
                if (i < scalar.Length && scalar[i] != ',' && scalar[i] != '|')
                {
                    continue;
                }

                var part = scalar.AsSpan(start, i - start).Trim();
                start = i + 1;
                if (part.IsEmpty)
                {
                    continue;
                }

                if (!TryResolveFlagComponent(part, out var bits))
                {
                    result = default!;
                    return false;
                }
                accumulated |= bits;
            }

            result = FromUInt64(accumulated);
            return true;
        }

        static bool TryResolveFlagComponent(ReadOnlySpan<char> part, out ulong bits)
        {
            var partString = part.ToString();
            if (Values.TryGetValue(partString, out var value))
            {
                bits = ToUInt64(value);
                return true;
            }

            var mutator = NamingConventionMutator.Of(NamingConventionByType ?? YamlSerializerOptions.DefaultNamingConvention);
            Span<char> buffer = stackalloc char[partString.Length];
            int written;
            while (!mutator.TryMutate(partString.AsSpan(), buffer, out written))
            {
                // ReSharper disable once StackAllocInsideLoop
                buffer = stackalloc char[buffer.Length * 2];
            }
            if (Values.TryGetValue(buffer[..written].ToString(), out value))
            {
                bits = ToUInt64(value);
                return true;
            }

            // A bare numeric component (e.g. "4") is also a valid flags token.
            if (ulong.TryParse(partString, out bits))
            {
                return true;
            }

            bits = 0;
            return false;
        }

        // Pre-encode the name to UTF8 if (and only if) it is a plain scalar — i.e. a single
        // line needing no quotes. Such bytes can be emitted verbatim by WriteScalar in any
        // context. Names that need quoting/escaping return null and fall back to WriteString,
        // which also keeps the runtime StringQuoteStyle option authoritative.
        static byte[]? TryEncodePlainUtf8(string value)
        {
            var info = EmitStringAnalyzer.Analyze(value.AsSpan());
            return info.Lines <= 1 && !info.NeedsQuotes
                ? StringEncoding.Utf8.GetBytes(value)
                : null;
        }

        // Reinterpret an enum value as an unsigned integer of the matching width.
        // This is a bitwise cast (no boxing), unlike Convert.ToUInt64.
        static unsafe ulong ToUInt64(T value)
        {
            switch (sizeof(T))
            {
                case 1:
                    return *(byte*)&value;
                case 2:
                    return *(ushort*)&value;
                case 4:
                    return *(uint*)&value;
                default:
                    return *(ulong*)&value;
            }
        }

        // Reinterpret an unsigned integer back into the enum type (no boxing, unlike Enum.ToObject).
        static unsafe T FromUInt64(ulong bits)
        {
            switch (sizeof(T))
            {
                case 1:
                {
                    var narrowed = (byte)bits;
                    return *(T*)&narrowed;
                }
                case 2:
                {
                    var narrowed = (ushort)bits;
                    return *(T*)&narrowed;
                }
                case 4:
                {
                    var narrowed = (uint)bits;
                    return *(T*)&narrowed;
                }
                default:
                    return *(T*)&bits;
            }
        }
    }
}
