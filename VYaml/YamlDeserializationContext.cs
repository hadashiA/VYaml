using System.Collections.Generic;

namespace VYaml
{
    public class YamlDeserializationContext
    {
        readonly Dictionary<string, object?> anchors = new();
    }
}
