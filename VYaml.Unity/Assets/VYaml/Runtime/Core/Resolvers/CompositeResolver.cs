using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VYaml.Formatters;

namespace VYaml.Resolvers
{
    public static class CompositeResolver
    {
        static readonly IReadOnlyDictionary<Type, IYamlFormatter> EmptyFormattersByType =
            new Dictionary<Type, IYamlFormatter>();

        public static IYamlFormatterResolver Create(IReadOnlyList<IYamlFormatter> formatters, IReadOnlyList<IYamlFormatterResolver> resolvers)
        {
            if (formatters is null)
            {
                throw new ArgumentNullException(nameof(formatters));
            }

            if (resolvers is null)
            {
                throw new ArgumentNullException(nameof(resolvers));
            }

            // Make a copy of the resolvers list provided by the caller to guard against them changing it later.
            var immutableFormatters = formatters.ToArray();
            var immutableResolvers = resolvers.ToArray();

            return new CachingResolver(immutableFormatters, immutableResolvers);
        }

        public static IYamlFormatterResolver Create(params IYamlFormatterResolver[] resolvers) =>
            Create(Array.Empty<IYamlFormatter>(), resolvers);

        public static IYamlFormatterResolver Create(params IYamlFormatter[] formatters) =>
            Create(formatters, Array.Empty<IYamlFormatterResolver>());

        class CachingResolver : IYamlFormatterResolver
        {
            readonly ConcurrentDictionary<Type, IYamlFormatter> formattersCache = new();
            readonly IYamlFormatter[] subFormatters;
            readonly IYamlFormatterResolver[] subResolvers;

            /// <summary>
            /// Initializes a new instance of the <see cref="CachingResolver"/> class.
            /// </summary>
            internal CachingResolver(IYamlFormatter[] subFormatters, IYamlFormatterResolver[] subResolvers)
            {
                this.subFormatters = subFormatters;
                this.subResolvers = subResolvers;
            }

            public IYamlFormatter<T> GetFormatter<T>()
            {
                if (!this.formattersCache.TryGetValue(typeof(T), out IYamlFormatter formatter))
                {
                    foreach (var subFormatter in subFormatters)
                    {
                        if (subFormatter is IYamlFormatter<T>)
                        {
                            formatter = subFormatter;
                            goto CACHE;
                        }
                    }

                    foreach (IYamlFormatterResolver resolver in subResolvers)
                    {
                        formatter = resolver.GetFormatter<T>();
                        if (formatter != null)
                        {
                            goto CACHE;
                        }
                    }

// when not found, cache null.
CACHE:
                    formattersCache.TryAdd(typeof(T), formatter);
                }

                return (IYamlFormatter<T>)formatter;
            }
        }
    }
}