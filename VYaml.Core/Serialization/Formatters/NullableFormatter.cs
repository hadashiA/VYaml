#nullable enable
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class NullableFormatter<T> : IYamlFormatter<T?> where T : struct
    {
        public void Serialize(ref Utf8YamlEmitter emitter, T? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
            }
            else
            {
                context.Resolver.GetFormatterWithVerify<T>()
                    .Serialize(ref emitter, value.Value, context);
            }
        }

        public T? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return null;
            }

            return context.DeserializeWithAlias<T>(ref parser);
        }
    }

    public sealed class StaticNullableFormatter<T> : IYamlFormatter<T?> where T : struct
    {
        readonly IYamlFormatter<T> underlyingFormatter;

        public StaticNullableFormatter(IYamlFormatter<T> underlyingFormatter)
        {
            this.underlyingFormatter = underlyingFormatter;
        }

        public void Serialize(ref Utf8YamlEmitter emitter, T? value, YamlSerializationContext context)
        {
            if (value.HasValue)
            {
                underlyingFormatter.Serialize(ref emitter, value.Value, context);
            }
            else
            {
                emitter.WriteNull();
            }
        }

        public T? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                parser.Read();
                return null;
            }
            return underlyingFormatter.Deserialize(ref parser, context);
        }
    }
}
