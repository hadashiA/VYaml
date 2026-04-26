namespace VYaml.Serialization
{
    public class StandardResolver : IYamlFormatterResolver
    {
        public static readonly StandardResolver Instance = new();

        public static readonly IYamlFormatterResolver[] DefaultResolvers =
        {
            BuiltinResolver.Instance,
#if UNITY_2018_3_OR_NEWER
            UnityResolver.Instance,
#endif
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
                    foreach (IYamlFormatterResolver item in DefaultResolvers)
                    {
                        var f = item.GetFormatter<T>();
                        if (f == null)
                        {
                            continue; // Short-circuit.
                        }
                        Formatter = f;
                        return;
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
