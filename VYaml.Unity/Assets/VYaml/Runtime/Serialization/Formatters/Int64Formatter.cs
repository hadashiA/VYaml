#nullable enable
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class Int64Formatter : IYamlFormatter<long>
    {
        public static readonly Int64Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, long value, YamlSerializationContext context)
        {
            emitter.WriteInt64(value);
        }

        public long Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsInt64();
            parser.Read();
            return result;
        }
    }

    public class NullableInt64Formatter : IYamlFormatter<long?>
    {
        public static readonly NullableInt64Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, long? value, YamlSerializationContext context)
        {
            if (value.HasValue)
            {
                emitter.WriteInt64(value.Value);
            }
            else
            {
                emitter.WriteNull();
            }
        }

        public long? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsInt64();
            parser.Read();
            return result;
        }
    }
}
