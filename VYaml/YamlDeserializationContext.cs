using System.Collections.Generic;
using VYaml.Formatters;
using VYaml.Resolvers;

namespace VYaml
{
    public class YamlDeserializationContext
    {
        public IYamlFormatterResolver Resolver { get; set; } = StandardResolver.Instance;

        readonly Dictionary<Anchor, object?> aliases = new();

        public void Reset()
        {
            aliases.Clear();
        }

        public T? DeserializeWithAlias<T>(IYamlFormatter<T> innerFormatter, ref YamlParser parser)
        {
            if (TryResolveCurrentAlias<T>(ref parser, out var aliasValue))
            {
                return aliasValue;
            }

            var withAnchor = parser.TryGetCurrentAnchor(out var anchor);

            var result = innerFormatter.Deserialize(ref parser, this);

            if (withAnchor)
            {
                RegisterAnchor(anchor, result);
            }
            return result;
        }

        void RegisterAnchor(Anchor anchor, object? value)
        {
            aliases[anchor] = value;
        }

        bool TryResolveCurrentAlias<T>(ref YamlParser parser, out T? aliasValue)
        {
            if (parser.CurrentEventType != ParseEventType.Alias)
            {
                aliasValue = default;
                return false;
            }

            if (parser.TryGetCurrentAnchor(out var anchor))
            {
                if (aliases.TryGetValue(anchor, out var obj))
                {
                    switch (obj)
                    {
                        case null:
                            aliasValue = default;
                            return true;
                        case T value:
                            aliasValue = value;
                            return true;
                        default:
                            throw new YamlSerializerException($"Alias value is not a type of {typeof(T).Name}");
                    }
                }
                throw new YamlSerializerException($"Could not found alias value of anchor: {anchor}");
            }

            aliasValue = default;
            return false;
        }
    }
}
