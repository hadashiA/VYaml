using System.Collections.Generic;

namespace VYaml
{
    public class YamlDeserializationContext
    {
        readonly Dictionary<Anchor, object?> aliases = new();

        public object? GetAlias(Anchor anchor)
        {
            return aliases[anchor];
        }

        public void RegisterAnchor(Anchor anchor, object? content)
        {
            aliases[anchor] = content;
        }
    }
}
