using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    // TODO:
    static class EnumAsStringNonGenericHelper
    {
        static readonly ConcurrentDictionary<object, string> AliasStringValues = new();
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

    public class EnumAsStringFormatter<T> : IYamlFormatter<T> where T : Enum
    {
        // ReSharper disable once StaticMemberInGenericType
        internal static readonly NamingConvention? NamingConventionByType;

        static readonly Dictionary<T, (string Value, bool Alias)> StringValues = new();
        static readonly Dictionary<string, T> Values = new();

        static EnumAsStringFormatter()
        {
            var type = typeof(T);
            NamingConventionByType = EnumAsStringNonGenericHelper.GetNamingConventionByType(type);

            foreach (var item in type.GetFields().Where(x => x.FieldType == type))
            {
                var value = item.GetValue(null)!;
                var aliasValue = EnumAsStringNonGenericHelper.GetAliasStringValue(type, value);
                if (aliasValue != null)
                {
                    StringValues.Add((T)value, (aliasValue, true));
                    Values.Add(aliasValue, (T)value);
                }
                else
                {
                    var mutator = NamingConventionMutator.Of(NamingConventionByType ?? YamlSerializerOptions.DefaultNamingConvention);
                    var name = Enum.GetName(type, value)!;
                    Span<char> destination = stackalloc char[name.Length];
                    int written;
                    while (!mutator.TryMutate(name.AsSpan(), destination, out written))
                    {
                        // ReSharper disable once StackAllocInsideLoop
                        destination = stackalloc char[destination.Length * 2];
                    }

                    var stringValue = destination[..written].ToString();
                    StringValues.Add((T)value, (stringValue, false));
                    Values.Add(stringValue, (T)value);
                }
            }
        }

        public void Serialize(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
        {
            if (!StringValues.TryGetValue(value, out var t))
            {
                YamlSerializerException.ThrowInvalidType<T>(value.ToString());
                return;
            }

            var (stringValue, alias) = t;
            if (alias || context.Options.NamingConvention == (NamingConventionByType ?? YamlSerializerOptions.DefaultNamingConvention))
            {
                emitter.WriteString(stringValue);
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
            parser.Read();
            if (Values.TryGetValue(mutatedScalar, out value))
            {
                return value;
            }
            YamlSerializerException.ThrowInvalidType<T>(mutatedScalar);
            return default!;
        }
    }
}