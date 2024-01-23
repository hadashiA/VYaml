#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace VYaml.Serialization
{
    public class CompositeResolver : IYamlFormatterResolver
    {
        readonly ConcurrentDictionary<Type, IYamlFormatter> formattersCache = new();
        readonly List<IYamlFormatter> formatters;
        readonly List<IYamlFormatterResolver> resolvers;

        readonly object gate = new();

        public static CompositeResolver Create(IEnumerable<IYamlFormatter> formatters, IEnumerable<IYamlFormatterResolver> resolvers)
        {
            return new CompositeResolver(formatters.ToList(), resolvers.ToList());
        }

        public static CompositeResolver Create(IEnumerable<IYamlFormatter> formatters)
        {
            return new CompositeResolver(formatters.ToList());
        }

        public static CompositeResolver Create(IEnumerable<IYamlFormatterResolver> resolvers)
        {
            return new CompositeResolver(null, resolvers.ToList());
        }

        CompositeResolver(
            List<IYamlFormatter>? formatters = null,
            List<IYamlFormatterResolver>? resolvers = null)
        {
            this.formatters = formatters ?? new List<IYamlFormatter>();
            this.resolvers = resolvers ?? new List<IYamlFormatterResolver>();
        }

        public IYamlFormatter<T>? GetFormatter<T>()
        {
            if (!formattersCache.TryGetValue(typeof(T), out var formatter))
            {
                lock (gate)
                {
                    foreach (var f in formatters)
                    {
                        if (f is IYamlFormatter<T>)
                        {
                            formatter = f;
                            goto CACHE;
                        }
                    }

                    foreach (var resolver in resolvers)
                    {
                        if (resolver.GetFormatter<T>() is { } f)
                        {
                            formatter = f;
                            goto CACHE;
                        }
                    }
                }

// when not found, cache null.
CACHE:
                formattersCache.TryAdd(typeof(T), formatter!);
            }

            return formatter as IYamlFormatter<T>;
        }

        public void AddFormatter(IYamlFormatter formatter)
        {
            lock (gate)
            {
                formatters.Add(formatter);
            }
        }

        public void AddResolver(IYamlFormatterResolver resolver)
        {
            lock (gate)
            {
                resolvers.Add(resolver);
            }
        }
    }
}
