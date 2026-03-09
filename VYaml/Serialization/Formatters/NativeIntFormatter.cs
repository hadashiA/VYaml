#nullable enable
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class NativeIntFormatter : IYamlFormatter<nint>
    {
        public static readonly NativeIntFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, nint value, YamlSerializationContext context)
        {
            emitter.WriteInt64(value);
        }

        public nint Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsInt64();
            parser.Read();
            return (nint)result;
        }
    }

    public class NullableNativeIntFormatter : IYamlFormatter<nint?>
    {
        public static readonly NullableNativeIntFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, nint? value, YamlSerializationContext context)
        {
            if (value.HasValue)
            {
                emitter.WriteInt64(value.GetValueOrDefault());
            }
            else
            {
                emitter.WriteNull();
            }
        }

        public nint? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsInt64();
            parser.Read();
            return (nint)result;
        }
    }

    public class NativeUIntFormatter : IYamlFormatter<nuint>
    {
        public static readonly NativeUIntFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, nuint value, YamlSerializationContext context)
        {
            emitter.WriteUInt64(value);
        }

        public nuint Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            var result = parser.GetScalarAsUInt64();
            parser.Read();
            return (nuint)result;
        }
    }

    public class NullableNativeUIntFormatter : IYamlFormatter<nuint?>
    {
        public static readonly NullableNativeUIntFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, nuint? value, YamlSerializationContext context)
        {
            if (value.HasValue)
            {
                emitter.WriteUInt64(value.GetValueOrDefault());
            }
            else
            {
                emitter.WriteNull();
            }
        }

        public nuint? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return default;
            }

            var result = parser.GetScalarAsUInt64();
            parser.Read();
            return (nuint)result;
        }
    }
}