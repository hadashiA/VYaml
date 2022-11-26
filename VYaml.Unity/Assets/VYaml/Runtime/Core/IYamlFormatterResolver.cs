using VYaml.Formatters;

namespace VYaml
{
    public interface IYamlFormatterResolver
    {
        IYamlFormatter<T> GetFormatter<T>();
    }
}
