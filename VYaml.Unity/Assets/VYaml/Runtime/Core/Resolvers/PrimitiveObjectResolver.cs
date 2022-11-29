using VYaml.Formatters;

namespace VYaml.Resolvers
{
    public class PrimitiveObjectResolver : IYamlFormatterResolver
    {
        public static readonly PrimitiveObjectResolver Instance = new();

        static class FormatterCache<T>
        {
            public static readonly IYamlFormatter<T> Formatter;

            static FormatterCache()
            {
                Formatter = (IYamlFormatter<T>)PrimitiveObjectFormatter.Instance;
            }
        }

        public IYamlFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }
    }
}
