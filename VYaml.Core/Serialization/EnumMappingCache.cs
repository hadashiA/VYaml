using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VYaml.Internal;

namespace VYaml.Serialization
{
    class EnumMappingCache<T> where T : Enum
    {
        public IReadOnlyDictionary<string, T> NameValueMapping { get; }
        public IReadOnlyDictionary<T, string> ValueNameMapping { get; }

        public static EnumMappingCache<T> Create(NamingConvention namingConvention)
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
                    var name = Enum.GetName(type, value)!;
                    names.Add(namingConvention switch
                    {
                        NamingConvention.LowerCamelCase => KeyNameHelper.ToCamelCase(name),
                        NamingConvention.UpperCamelCase => name,
                        _ => throw new ArgumentOutOfRangeException(nameof(namingConvention), namingConvention, null)
                    });
                }
            }

            var nameValueMapping = new Dictionary<string, T>(names.Count);
            var valueNameMapping = new Dictionary<T, string>(names.Count);

            foreach (var (value, name) in values.Zip(names, (v, n) => (v, n)))
            {
                nameValueMapping[name] = (T)value;
                valueNameMapping[(T)value] = name;
            }
            return new EnumMappingCache<T>(nameValueMapping, valueNameMapping);
        }

        EnumMappingCache(
            IReadOnlyDictionary<string, T> nameValueMapping,
            IReadOnlyDictionary<T, string> valueNameMapping)
        {
            NameValueMapping = nameValueMapping;
            ValueNameMapping = valueNameMapping;
        }
    }
}