#nullable enable
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class UInt16Formatter : IYamlFormatter<ushort>
    {
        public static readonly UInt16Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, ushort value, YamlSerializationContext context)
        {
            emitter.WriteUInt32(value);
        }

        public ushort Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsUInt32();
            parser.Read();
            return checked((ushort)result);
        }
    }

    public class NullableUInt16Formatter : IYamlFormatter<ushort?>
    {
        public static readonly NullableUInt16Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, ushort? value, YamlSerializationContext context)
        {
            if (value.HasValue)
            {
                emitter.WriteUInt32(value.Value);
            }
            else
            {
                emitter.WriteNull();
            }
        }

        public ushort? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsUInt32();
            parser.Read();
            return checked((ushort)result);
        }
    }
}
