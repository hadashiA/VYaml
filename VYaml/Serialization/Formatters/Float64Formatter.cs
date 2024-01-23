#nullable enable
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class Float64Formatter : IYamlFormatter<double>
    {
        public static readonly Float64Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double value, YamlSerializationContext context)
        {
            emitter.WriteDouble(value);
        }

        public double Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsDouble();
            parser.Read();
            return result;
        }
    }

    public class NullableFloat64Formatter : IYamlFormatter<double?>
    {
        public static readonly NullableFloat64Formatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, double? value, YamlSerializationContext context)
        {
            if (value.HasValue)
            {
                emitter.WriteDouble(value.Value);
            }
            else
            {
                emitter.WriteNull();
            }
        }

        public double? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsDouble();
            parser.Read();
            return result;
        }
    }
}
