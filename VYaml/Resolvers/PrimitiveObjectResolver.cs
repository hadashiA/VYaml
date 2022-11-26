using VYaml.Formatters;

namespace VYaml.Resolvers
{
    public class PrimitiveObjectResolver : IYamlFormatterResolver
    {
        static class FormatterCache<T>
        {
            public static readonly IYamlFormatter<T> Formatter;

            static FormatterCache()
            {
                Formatter = (typeof(T) == typeof(object)
                    ? (IYamlFormatter<T>)(object)PrimitiveObjectFormatter.Instance
                    : null;
            }
        }

        public IYamlFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }
    }
}
