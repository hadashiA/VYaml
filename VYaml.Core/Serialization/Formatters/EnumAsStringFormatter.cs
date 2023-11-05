#nullable enable
using System;
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
    class EnumAsStringNonGenericCache
    {
        public static readonly EnumAsStringNonGenericCache Instance = new();

        readonly ConcurrentDictionary<object, string> stringValues = new();
        readonly Func<object, Type, string> valueFactory = CreateValue;

        public string GetStringValue(Type type, object value)
        {
            if (stringValues.TryGetValue(value, out var stringValue))
            {
                return stringValue;
            }
            return stringValues.GetOrAdd(value, valueFactory, type);
        }

        static string CreateValue(object value, Type type)
        {
            var attr = type.GetCustomAttribute<YamlObjectAttribute>();
            var namingConvention = attr?.NamingConvention ?? NamingConvention.LowerCamelCase;
            var stringValue = Enum.GetName(type, value)!;
            return KeyNameMutator.Mutate(stringValue, namingConvention);
        }
    }

    public class EnumAsStringFormatter<T> : IYamlFormatter<T> where T : Enum
    {
        static readonly Dictionary<string, T> NameValueMapping;
        static readonly Dictionary<T, string> ValueNameMapping;

        static EnumAsStringFormatter()
        {
            var names = new List<string>();
            var values = new List<object>();

            var type = typeof(T);
            var namingConvention = type.GetCustomAttribute<YamlObjectAttribute>()?.NamingConvention ?? NamingConvention.LowerCamelCase;
            foreach (var item in type.GetFields().Where(x => x.FieldType == type))
            {
                var value = item.GetValue(null);
                values.Add(value);

                var attributes = item.GetCustomAttributes(true);
                if (attributes.OfType<EnumMemberAttribute>().FirstOrDefault() is { Value: { } enumMemberValue })
                {
                    names.Add(enumMemberValue);
                }
                else if (attributes.OfType<DataMemberAttribute>().FirstOrDefault() is { Name: { } dataMemberName })
                {
                    names.Add(dataMemberName);
                }
                else
                {
                    var name = Enum.GetName(type, value)!;
                    names.Add(KeyNameMutator.Mutate(name, namingConvention));
                }
            }

            NameValueMapping = new Dictionary<string, T>(names.Count);
            ValueNameMapping = new Dictionary<T, string>(names.Count);

            foreach (var (value, name) in values.Zip(names, (v, n) => (v, n)))
            {
                NameValueMapping[name] = (T)value;
                ValueNameMapping[(T)value] = name;
            }
        }

        public void Serialize(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
        {
            if (ValueNameMapping.TryGetValue(value, out var name))
            {
                emitter.WriteString(name, ScalarStyle.Plain);
            }
            else
            {
                YamlSerializerException.ThrowInvalidType(value);
            }
        }

        public T Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var scalar = parser.ReadScalarAsString();
            if (scalar is null)
            {
                YamlSerializerException.ThrowInvalidType<T>();
            }
            else if (NameValueMapping.TryGetValue(scalar, out var value))
            {
                return value;
            }
            YamlSerializerException.ThrowInvalidType<T>();
            return default!;
        }
    }
}

