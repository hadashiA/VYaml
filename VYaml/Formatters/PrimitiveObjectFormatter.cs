using System;
using System.Collections.Generic;

namespace VYaml.Formatters
{
    public class PrimitiveObjectFormatter : IYamlFormatter<object?>
    {
        public object? Deserialize(ref YamlParser parser)
        {
             switch (parser.CurrentEventType)
             {
                 case ParseEventType.Scalar:
                     if (parser.IsNullScalar())
                     {
                         parser.Read();
                         return null;
                     }
                     if (parser.TryGetScalarAsBool(out var boolValue))
                     {
                         parser.Read();
                         return boolValue;
                     }
                     if (parser.TryGetScalarAsDouble(out var doubleValue))
                     {
                         parser.Read();
                         return doubleValue;
                     }

                     var stringValue = parser.GetScalarAsString();
                     parser.Read();
                     return stringValue;

                 case ParseEventType.MappingStart:
                 {
                     var dict = new Dictionary<object?, object?>();
                     parser.Read();
                     while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
                     {
                         var key = Deserialize(ref parser);
                         var value = Deserialize(ref parser);
                         dict.Add(key, value);
                     }
                     return dict;
                 }
                 case ParseEventType.SequenceStart:
                 {
                     var list = new List<object?>();
                     while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
                     {
                         var element = Deserialize(ref parser);
                         list.Add(element);
                     }
                     return list;
                 }
                 default:
                     throw new InvalidOperationException();
             }
        }
    }
}
