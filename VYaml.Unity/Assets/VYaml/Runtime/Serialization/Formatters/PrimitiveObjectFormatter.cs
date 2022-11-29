using System;
using System.Collections.Generic;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class PrimitiveObjectFormatter : IYamlFormatter<object?>
    {
        public static readonly PrimitiveObjectFormatter Instance = new();

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
