using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;
using static System.Runtime.CompilerServices.Unsafe;

namespace VYaml.Serialization
{
    static class EnumAsStringNonGenericHelper
    {
        static readonly ConcurrentDictionary<object, string?> AliasStringValues = new();
        static readonly ConcurrentDictionary<Type, NamingConvention?> NamingConventionsByType = new();

        static readonly Func<object, Type, string?> AliasStringValueFactory = AnalyzeAliasStringValue;
        static readonly Func<Type, NamingConvention?> NamingConventionFactory = AnalyzeNamingConventionByType;

        public static string? GetAliasStringValue(Type type, object value) =>
            AliasStringValues.GetOrAdd(value, AliasStringValueFactory, type);

        public static NamingConvention? GetNamingConventionByType(Type type) =>
            NamingConventionsByType.GetOrAdd(type, NamingConventionFactory);

        public static void Serialize(ref Utf8YamlEmitter emitter, Type type, object value,
            YamlSerializationContext context)
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
}

namespace VYaml.Serialization
{
    public class EnumAsStringOrFlagFormatter<T> : IYamlFormatter<T> where T : struct, Enum
    {
        // All fields below are implicitly private. Consider making them explicit.
        static readonly bool IsFlagsEnum = typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false);
        static readonly Dictionary<T, (string Value, bool IsAlias)> StringValues = new();
        static readonly Dictionary<string, T> Values = new(StringComparer.OrdinalIgnoreCase);

        static EnumAsStringOrFlagFormatter()
        {
            var type = typeof(T);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != type) continue;

                var value = (T)field.GetValue(null)!;
                var aliasValue = EnumAsStringNonGenericHelper.GetAliasStringValue(type, value);

                if (aliasValue != null)
                {
                    StringValues[value] = (aliasValue, true);
                    Values[aliasValue] = value;
                }
                else
                {
                    var namingConvention = EnumAsStringNonGenericHelper.GetNamingConventionByType(type)
                                           ?? YamlSerializerOptions.DefaultNamingConvention;

                    var name = Enum.GetName(type, value)!;
                    var mutator = NamingConventionMutator.Of(namingConvention);

                    Span<char> destination = stackalloc char[name.Length * 2];
                    int written;
                    while (!mutator.TryMutate(name.AsSpan(), destination, out written))
                    {
                        // ReSharper disable once StackAllocInsideLoop
                        destination = stackalloc char[destination.Length * 2];
                    }

                    var stringValue = destination[..written].ToString();
                    StringValues[value] = (stringValue, false);
                    Values[stringValue] = value;
                }
            }
        }

        public void Serialize(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
        {
            if (!IsFlagsEnum)
            {
                // For normal enums
                if (StringValues.TryGetValue(value, out var t))
                {
                    var (stringValue, alias) = t;
                    if (alias || context.Options.NamingConvention ==
                        (EnumAsStringNonGenericHelper.GetNamingConventionByType(typeof(T))
                         ?? YamlSerializerOptions.DefaultNamingConvention))
                    {
                        emitter.WriteString(stringValue);
                        return;
                    }
                }

                // Fallback
                EnumAsStringNonGenericHelper.Serialize(ref emitter, typeof(T), value, context);
                return;
            }

            // Output as comma-separated names (e.g. "Read, Write, Execute")
            string flagsString = value.ToString();
            emitter.WriteString(flagsString);
        }

        public T Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var scalar = parser.ReadScalarAsString();
            if (scalar == null)
            {
                YamlSerializerException.ThrowInvalidType<T>("null");
                return default;
            }

            // If a flagging enum, then parse as one.
            if (IsFlagsEnum)
                return ParseFlags(scalar); // Short-circuit.
            
            // Original normal enum deserialization.
            if (Values.TryGetValue(scalar, out var value))
                return value; // If it gets a value immediately, then it is a 1:1 enum value, and it'll be fine.

            // Try with naming convention mutation
            var mutator = NamingConventionMutator.Of(
                EnumAsStringNonGenericHelper.GetNamingConventionByType(typeof(T))
                ?? YamlSerializerOptions.DefaultNamingConvention);

            Span<char> buffer = stackalloc char[scalar.Length * 2];
            int written;
            while (!mutator.TryMutate(scalar.AsSpan(), buffer, out written))
            {
                // There is a memory alloc. in a loop. Hopefully this is fine. -Z
                buffer = stackalloc char[buffer.Length * 2];
            }

            var mutated = buffer[..written].ToString();
            if (Values.TryGetValue(mutated, out value))
                return value;

            YamlSerializerException.ThrowInvalidType<T>(scalar);
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T ParseFlags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return default;

            // Support: "Read, Write", "Read | Write", "Read Write"
            var parts = input.Split(new[] { ',', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            T result = default;

            foreach (var part in parts)
            {
                // Attempt to extract a flag directly from a string
                if (Values.TryGetValue(part, out var flag))
                {
                    result = Or(result, flag);
                }
                // Attempt to extract a number instead, which may just be a flag-value.
                else if (ulong.TryParse(part, out var num))
                {
                    result = Or(result, FromUInt64(num));
                }
                // If all else fails: throw an exception.
                else
                {
                    throw new YamlSerializerException($"Unknown flag value '{part}' for Flags enum {typeof(T)}");
                }
            }

            return result;

            // Note: The following methods do not live outside of this one, and so have been in-lined.

            // Fast bitwise OR helper
            static T Or(T left, T right)
            {
                ulong l = ToUInt64(left);
                ulong r = ToUInt64(right);
                ulong v = l | r;

                return FromUInt64(v);
            }

            // Provided numeric value, get size and determine type w. As().
            // Doesn't box, but is more unsafe.
            static ulong ToUInt64(T value)
            {
                switch (SizeOf<T>())
                {
                    case 1:
                        return As<T, byte>(ref value);
                    case 2:
                        return As<T, ushort>(ref value);
                    case 4:
                        return As<T, uint>(ref value);
                    case 8:
                        return As<T, ulong>(ref value);
                    default:
                        YamlSerializerException.ThrowInvalidType<T>(value.ToString());
                        return default;
                }
            }

            // Provided ulong value, cast to type T enum. ulong is max-size supported for flags enums.
            // Requires enums to be explicitly defined. (E.g. "enum MyEnum : byte")
            static T FromUInt64(ulong number)
            {
                switch (SizeOf<T>())
                {
                    case 1:
                        return As<byte, T>(ref As<ulong, byte>(ref number));
                    case 2:
                        return As<ushort, T>(ref As<ulong, ushort>(ref number));
                    case 4:
                        return As<uint, T>(ref As<ulong, uint>(ref number));
                    case 8:
                        return As<ulong, T>(ref number);
                    default:
                        YamlSerializerException.ThrowInvalidType<T>(number.ToString());
                        return default;
                }
            }
        }
    }
}