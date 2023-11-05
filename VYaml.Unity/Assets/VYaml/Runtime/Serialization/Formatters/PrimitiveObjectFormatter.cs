#nullable enable
using System;
using System.Collections;
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

            var type = value.GetType();
            if (TypeToJumpCode.TryGetValue(type, out var code))
            {
                switch (code)
                {
                    case 0:
                        emitter.WriteBool((bool)value);
                        return;
                    case 1:
                        emitter.WriteInt32((char)value);
                        return;
                    case 2:
                        emitter.WriteInt32((sbyte)value);
                        return;
                    case 3:
                        emitter.WriteUInt32((byte)value);
                        return;
                    case 4:
                        emitter.WriteInt32((short)value);
                        return;
                    case 5:
                        emitter.WriteUInt32((ushort)value);
                        return;
                    case 6:
                        emitter.WriteInt32((int)value);
                        return;
                    case 7:
                        emitter.WriteUInt32((uint)value);
                        return;
                    case 8:
                        emitter.WriteInt64((long)value);
                        return;
                    case 9:
                        emitter.WriteUInt64((ulong)value);
                        return;
                    case 10:
                        emitter.WriteFloat((float)value);
                        return;
                    case 11:
                        emitter.WriteDouble((double)value);
                        return;
                    case 12:
                        DateTimeFormatter.Instance.Serialize(ref emitter, (DateTime)value, context);
                        return;
                    case 13:
                        emitter.WriteString((string)value);
                        return;
                    case 14:
                        ByteArrayFormatter.Instance.Serialize(ref emitter, (byte[])value, context);
                        return;
                }
            }

            if (type.IsEnum)
            {
                var enumValue = EnumAsStringNonGenericCache.Instance.GetStringValue(type, value);
                emitter.WriteString(enumValue, ScalarStyle.Plain);
                return;
            }

            // check dictionary first
            if (value is IDictionary dict)
            {
                emitter.BeginMapping();
                foreach (DictionaryEntry item in dict)
                {
                    Serialize(ref emitter, item.Key, context);
                    Serialize(ref emitter, item.Value, context);
                }
                emitter.EndMapping();
                return;
            }

            if (value is ICollection collection)
            {
                emitter.BeginSequence();
                foreach (var item in collection)
                {
                    Serialize(ref emitter, item, context);
                }
                emitter.EndSequence();
                return;
            }

            throw new YamlSerializerException($"Not supported primitive object resolver. type: {type}");
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
                    else if (parser.TryGetScalarAsInt64(out var int64Value))
                    {
                        parser.Read();
                        result = int64Value;
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

