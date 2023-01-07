using System;
using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class PrimitiveObjectFormatter : IYamlFormatter<object?>
    {
        public static readonly PrimitiveObjectFormatter Instance = new();

        static readonly Dictionary<Type, int> TypeToJumpCode = new()
        {
            { typeof(Boolean), 0 },
            { typeof(Char), 1 },
            { typeof(SByte), 2 },
            { typeof(Byte), 3 },
            { typeof(Int16), 4 },
            { typeof(UInt16), 5 },
            { typeof(Int32), 6 },
            { typeof(UInt32), 7 },
            { typeof(Int64), 8 },
            { typeof(UInt64), 9 },
            { typeof(Single), 10 },
            { typeof(Double), 11 },
            { typeof(DateTime), 12 },
            { typeof(string), 13 },
            { typeof(byte[]), 14 },
        };

        public void Serialize(ref Utf8YamlEmitter emitter, object? value, YamlSerializationContext context)
        {
            if (value is null)
            {
                emitter.WriteNull();
                return;
            }

            switch (value)
            {
                case int x:
                    emitter.WriteInt32(x);
                    break;
                case uint x:
                    emitter.WriteUInt32(x);
                    break;
            }
        }

        public object? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            object? result;

            switch (parser.CurrentEventType)
            {
                // TODO: checking tags
                case ParseEventType.Scalar:
                    if (parser.IsNullScalar())
                    {
                        parser.Read();
                        result = null;
                    }
                    else if (parser.TryGetScalarAsBool(out var boolValue))
                    {
                        parser.Read();
                        result = boolValue;
                    }
                    else if (parser.TryGetScalarAsInt32(out var intValue))
                    {
                        parser.Read();
                        result = intValue;
                    }
                    else if (parser.TryGetScalarAsDouble(out var doubleValue))
                    {
                        parser.Read();
                        result = doubleValue;
                    }
                    else
                    {
                        var stringValue = parser.GetScalarAsString();
                        parser.Read();
                        result = stringValue;
                    }
                    break;
                case ParseEventType.MappingStart:
                {
                    var dict = new Dictionary<object?, object?>();
                    parser.Read();
                    while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
                    {
                        var key = context.DeserializeWithAlias(this, ref parser);
                        var value = context.DeserializeWithAlias(this, ref parser);
                        dict.Add(key, value);
                    }
                    parser.ReadWithVerify(ParseEventType.MappingEnd);
                    result = dict;
                    break;
                 }
                 case ParseEventType.SequenceStart:
                 {
                     var list = new List<object?>();
                     parser.Read();
                     while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
                     {
                         var element = context.DeserializeWithAlias(this, ref parser);
                         list.Add(element);
                     }
                     parser.ReadWithVerify(ParseEventType.SequenceEnd);
                     result = list;
                     break;
                 }
                 default:
                     throw new InvalidOperationException();
            }
            return result;
        }
    }
}
