using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace VYaml.Serialization
{
    public class GeneratedResolver : IYamlFormatterResolver
    {
        public static GeneratedResolver Instance = new();

        static class Cache<T>
        {
            internal static IYamlFormatter<T>? Formatter;
            internal static bool Registered;

            static Cache()
            {
                if (Registered) return;

                var type = typeof(T);
                TryInvokeRegisterYamlFormatter(type);
            }

            static readonly ConcurrentDictionary<Type, IYamlFormatter> GeneratedFormatters = new();

            public static void Register<T>(IYamlFormatter<T> formatter)
            {
                Cache<T>.Registered = true; // avoid to call Cache() constructor called.
                GeneratedFormatters[typeof(T)] = formatter;
                Cache<T>.Formatter = formatter;
            }

            static bool TryInvokeRegisterYamlFormatter(Type type)
            {
                var m = type.GetMethod("__RegisterVYamlFormatter",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static);

                if (m == null)
                {
                    return false;
                }

                m.Invoke(null, null); // Cache<T>.formatter will set from method
                return true;
            }

        }

        public IYamlFormatter<T>? GetFormatter<T>() => Cache<T>.Formatter;
    }
}