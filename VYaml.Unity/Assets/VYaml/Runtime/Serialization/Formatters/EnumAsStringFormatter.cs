#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VYaml.Emitter;
using VYaml.Internal;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class EnumAsStringFormatter<T> : IYamlFormatter<T> where T : Enum
    {
        static readonly Dictionary<string, T> NameValueMapping;
        static readonly Dictionary<T, string> ValueNameMapping;

        static EnumAsStringFormatter()
        {
            var names = new List<string>();
            var values = new List<object>();

            var type = typeof(T);
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
                    var name = Enum.GetName(type, value);
                    names.Add(KeyNameHelper.ToCamelCase(name));
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
                throw new YamlSerializerException($"Cannot detect a value of enum: {typeof(T)}, {value}");
            }
        }

        public T Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var scalar = parser.ReadScalarAsString();
            if (scalar is null)
            {
                throw new YamlSerializerException($"Cannot detect a scalar value of {typeof(T)}");
            }

            if (NameValueMapping.TryGetValue(scalar, out var value))
            {
                return value;
            }
            throw new YamlSerializerException($"Cannot detect a scalar value of {typeof(T)}");
        }
    }
}

