namespace VYaml.Formatters
{
    public interface IYamlFormatter
    {
    }

    public interface IYamlFormatter<out T> : IYamlFormatter
    {
        T Deserialize(ref YamlParser parser, YamlDeserializationContext context);
    }
}
