using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using VYaml.Internal;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class YamlSerializerException : Exception
    {
        public YamlSerializerException(string message) : base(message)
        {
        }

        public YamlSerializerException(Marker mark, string message) : base($"{message} at {mark}")
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

        public static T Deserialize<T>(ReadOnlyMemory<byte> memory, YamlSerializerOptions? options = null)
        {
            var parser = YamlParser.FromSequence(new ReadOnlySequence<byte>(memory));
            return Deserialize<T>(ref parser, options);
        }

        public static T Deserialize<T>(in ReadOnlySequence<byte> sequence, YamlSerializerOptions? options = null)
        {
            var parser = YamlParser.FromSequence(sequence);
            return Deserialize<T>(ref parser, options);
        }

        public static async ValueTask<T> DeserializeAsync<T>(Stream stream, YamlSerializerOptions? options = null)
        {
            var byteSequenceBuilder = await StreamHelper.ReadAsSequenceAsync(stream);
            try
            {
                var sequence = byteSequenceBuilder.Build();
                return Deserialize<T>(in sequence, options);
            }
            finally
            {
                ReusableByteSequenceBuilderPool.Return(byteSequenceBuilder);
            }
        }

        public static T Deserialize<T>(ref YamlParser parser, YamlSerializerOptions? options = null)
        {
            options ??= DefaultOptions;
            try
            {
                var contextLocal = deserializationContext ??= new YamlDeserializationContext();
                contextLocal.Reset();
                contextLocal.Resolver = options.Resolver;

                parser.SkipAfter(ParseEventType.DocumentStart);

                var formatter = options.Resolver.GetFormatterWithVerify<T>();
                return contextLocal.DeserializeWithAlias(formatter, ref parser);
            }
            finally
            {
                parser.Dispose();
            }
        }
    }
}
