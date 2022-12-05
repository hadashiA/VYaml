using VYaml.Parser;

namespace VYaml.Serialization
{
    public class NullableFormatter<T> : IYamlFormatter<T?> where T : struct
    {
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