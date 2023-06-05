#nullable enable
using System;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class EnumAsLowerCamelCaseStringFormatter<T> : IYamlFormatter<T> where T : Enum
    {
        static readonly EnumMappingCache<T> Mapping;

        static EnumAsLowerCamelCaseStringFormatter()
        {
            Mapping = EnumMappingCache<T>.Create(NamingConvention.LowerCamelCase);
        }

        public void Serialize(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
        {
            if (Mapping.ValueNameMapping.TryGetValue(value, out var name))
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

            if (Mapping.NameValueMapping.TryGetValue(scalar, out var value))
            {
                return value;
            }
            throw new YamlSerializerException($"Cannot detect a scalar value of {typeof(T)}");
        }
    }
}
