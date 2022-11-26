namespace VYaml.Formatters
{
    public interface IYamlFormatter<out T>
    {
        T Deserialize(ref YamlParser parser);
    }
}
