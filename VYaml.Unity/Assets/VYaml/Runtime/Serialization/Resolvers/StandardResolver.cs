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

        CompositeResolver? extra;

        public IYamlFormatter<T>? GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        public void AddFormatter(params IYamlFormatter[] formatters)
        {
            if (extra is null)
            {
                extra = CompositeResolver.Create(formatters);
            }
            else
            {
                foreach (var f in formatters)
                {
                    extra.AddFormatter(f);
                }
            }
        }

        public void AddResolver(params IYamlFormatterResolver[] resolvers)
        {
            if (extra is null)
            {
                extra = CompositeResolver.Create(resolvers);
            }
            else
            {
                foreach (var r in resolvers)
                {
                    extra.AddResolver(r);
                }
            }
        }
    }
}
