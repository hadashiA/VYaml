#nullable enable
namespace VYaml.Serialization
{
    public class StandardResolver : IYamlFormatterResolver
    {
        public static readonly StandardResolver Instance = new();

        public static readonly IYamlFormatterResolver[] DefaultResolvers =
        {
            BuiltinResolver.Instance,
            GeneratedResolver.Instance,
        };

        static class FormatterCache<T>
        {
            public static readonly IYamlFormatter<T>? Formatter;

            static FormatterCache()
            {
                if (typeof(T) == typeof(object))
                {
                    // final fallback
                    Formatter = PrimitiveObjectResolver.Instance.GetFormatter<T>();
                }
                else
                {
                    foreach (var item in DefaultResolvers)
                    {
                        var f = item.GetFormatter<T>();
                        if (f != null)
                        {
                            Formatter = f;
                            return;
                        }
                    }
                }
            }
        }

        public IYamlFormatter<T>? GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }
    }
}
