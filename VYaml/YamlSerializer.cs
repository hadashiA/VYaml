using System;

namespace VYaml
{
    public static class YamlSerializer
    {
        [ThreadStatic]
        static YamlDeserializationContext? DeserializationContext;

        public static YamlSerializerOptions DefaultOptions
        {
            get => defaultOptions ??= YamlSerializerOptions.Standard;
            set => defaultOptions = value;
        }

        static YamlSerializerOptions? defaultOptions;


        public static T Deserialize<T>(ref YamlParser parser, YamlSerializerOptions options)
        {
            var contextLocal = (DeserializationContext ??= new YamlDeserializationContext());
            throw new NotImplementedException();
        }
    }
}
