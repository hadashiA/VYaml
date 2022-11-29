using System;
using System.Buffers;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class YamlSerializerException : Exception
    {
        public YamlSerializerException(string message) : base(message)
        {
        }
    }

    public static class YamlSerializer
    {
        [ThreadStatic]
        static YamlDeserializationContext? deserializationContext;

        public static YamlSerializerOptions DefaultOptions
        {
            get => defaultOptions ??= YamlSerializerOptions.Standard;
            set => defaultOptions = value;
        }

        static YamlSerializerOptions? defaultOptions;

        public static T Deserialize<T>(ReadOnlyMemory<byte> memory, YamlSerializerOptions options)
        {
            var parser = YamlParser.FromSequence(new ReadOnlySequence<byte>(memory));
            return Deserialize<T>(ref parser, options);
        }

        public static T Deserialize<T>(in ReadOnlySequence<byte> sequence, YamlSerializerOptions options)
        {
            var parser = YamlParser.FromSequence(sequence);
            try
            {
                return Deserialize<T>(ref parser, options);
            }
            finally
            {
                parser.Dispose();
            }
        }

        public static T Deserialize<T>(ref YamlParser parser, YamlSerializerOptions options)
        {
            var contextLocal = deserializationContext ??= new YamlDeserializationContext();
            contextLocal.Reset();
            contextLocal.Resolver = options.Resolver;

            parser.SkipAfter(ParseEventType.DocumentStart);

            return options.Resolver.GetFormatterWithVerify<T>()
                .Deserialize(ref parser, contextLocal);
        }
    }
}
