#nullable enable
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class SByteFormatter : IYamlFormatter<sbyte>
    {
        public static readonly SByteFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, sbyte value, YamlSerializationContext context)
        {
            emitter.WriteInt32(value);
        }

        public sbyte Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsInt32();
            parser.Read();
            return checked((sbyte)result);
        }
    }

    public class NullableSByteFormatter : IYamlFormatter<sbyte?>
    {
        public static readonly NullableSByteFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, sbyte? value, YamlSerializationContext context)
        {
            if (value.HasValue)
            {
                emitter.WriteInt32(value.Value);
            }
            else
            {
                emitter.WriteNull();
            }
        }

        public sbyte? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsInt32();
            parser.Read();
            return checked((sbyte)result);
        }
    }
}
