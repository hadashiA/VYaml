namespace VYaml.Formatters
{
    public interface IYamlFormatter<T>
    {
        // void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options);
        T Deserialize(ref Utf8Tokenizer tokenizer);        
    }
}
