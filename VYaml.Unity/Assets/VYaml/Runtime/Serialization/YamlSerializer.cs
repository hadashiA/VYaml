#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VYaml.Emitter;
using VYaml.Internal;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class YamlSerializerException : Exception
    {
        public static void ThrowInvalidType<T>(T value)
        {
            throw new YamlSerializerException($"Cannot detect a value of enum: {typeof(T)}, {value}");
        }

        public static void ThrowInvalidType<T>()
        {
            throw new YamlSerializerException($"Cannot detect a scalar value of {typeof(T)}");
        }

        public YamlSerializerException(string message) : base(message)
        {
        }

        public YamlSerializerException(Marker mark, string message) : base($"{message} at {mark}")
        {
        }
    }

    public static partial class YamlSerializer
    {
        [ThreadStatic]
        static YamlDeserializationContext? deserializationContext;

        [ThreadStatic]
        static YamlSerializationContext? serializationContext;

        static YamlDeserializationContext GetThreadLocalDeserializationContext(YamlSerializerOptions? options = null)
        {
            options ??= DefaultOptions;
            var contextLocal = deserializationContext ??= new YamlDeserializationContext(options);
            contextLocal.Resolver = options.Resolver;
            return contextLocal;
        }

        static YamlSerializationContext GetThreadLocalSerializationContext(YamlSerializerOptions? options = null)
        {
            options ??= DefaultOptions;
            var contextLocal = serializationContext ??= new YamlSerializationContext(options);
            contextLocal.Resolver = options.Resolver;
            contextLocal.EmitOptions = options.EmitOptions;
            return contextLocal;
        }

        public static YamlSerializerOptions DefaultOptions
        {
            get => defaultOptions ??= YamlSerializerOptions.Standard;
            set => defaultOptions = value;
        }

        static YamlSerializerOptions? defaultOptions;

        public static ReadOnlyMemory<byte> Serialize<T>(T value, YamlSerializerOptions? options = null)
        {
            options ??= DefaultOptions;
            var contextLocal = GetThreadLocalSerializationContext(options);
            var writer = contextLocal.GetArrayBufferWriter();
            var emitter = new Utf8YamlEmitter(writer);
            contextLocal.Reset();
            var formatter = contextLocal.Resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref emitter, value, contextLocal);
            return writer.WrittenMemory;
        }

        public static void Serialize<T>(IBufferWriter<byte> writer, T value, YamlSerializerOptions? options = null)
        {
            var emitter = new Utf8YamlEmitter(writer);
            Serialize(ref emitter, value, options);
        }

        public static void Serialize<T>(ref Utf8YamlEmitter emitter, T value, YamlSerializerOptions? options = null)
        {
            options ??= DefaultOptions;
            var contextLocal = GetThreadLocalSerializationContext(options);
            contextLocal.Reset();

            var formatter = contextLocal.Resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref emitter, value, contextLocal);
        }

        public static string SerializeToString<T>(T value, YamlSerializerOptions? options = null)
        {
            var utf8Bytes = Serialize(value, options);
            return StringEncoding.Utf8.GetString(utf8Bytes.Span);
        }

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
            var contextLocal = GetThreadLocalDeserializationContext(options);
            contextLocal.Reset();

            parser.SkipHeader();

            var formatter = options.Resolver.GetFormatterWithVerify<T>();
            return contextLocal.DeserializeWithAlias(formatter, ref parser);
        }

        public static async ValueTask<IEnumerable<T>> DeserializeMultipleDocumentsAsync<T>(Stream stream, YamlSerializerOptions? options = null)
        {
            var byteSequenceBuilder = await StreamHelper.ReadAsSequenceAsync(stream);
            try
            {
                var sequence = byteSequenceBuilder.Build();
                return DeserializeMultipleDocuments<T>(in sequence, options);
            }
            finally
            {
                ReusableByteSequenceBuilderPool.Return(byteSequenceBuilder);
            }
        }

        public static IEnumerable<T> DeserializeMultipleDocuments<T>(ReadOnlyMemory<byte> memory, YamlSerializerOptions? options = null)
        {
            var parser = YamlParser.FromSequence(new ReadOnlySequence<byte>(memory));
            return DeserializeMultipleDocuments<T>(ref parser, options);
        }

        public static IEnumerable<T> DeserializeMultipleDocuments<T>(in ReadOnlySequence<byte> sequence, YamlSerializerOptions? options = null)
        {
            var parser = YamlParser.FromSequence(sequence);
            return DeserializeMultipleDocuments<T>(ref parser, options);
        }

        public static IEnumerable<T> DeserializeMultipleDocuments<T>(ref YamlParser parser, YamlSerializerOptions? options = null)
        {
            // try
            // {
                options ??= DefaultOptions;
                var contextLocal = GetThreadLocalDeserializationContext(options);
                var formatter = options.Resolver.GetFormatterWithVerify<T>();
                var documents = new List<T>();

                while (true)
                {
                    parser.SkipAfter(ParseEventType.DocumentStart);
                    if (parser.End)
                    {
                        break;
                    }

                    contextLocal.Reset();
                    var document = contextLocal.DeserializeWithAlias(formatter, ref parser);
                    documents.Add(document);
                }
                return documents;
            // }
            // finally
            // {
            //     parser.Dispose();
            // }
        }
    }
}
