using System;
using System.Collections.Generic;

namespace VYaml.Formatters
{
    public class PrimitiveObjectFormatter : IYamlFormatter<object>
    {
        public object Deserialize(ref Utf8YamlTokenizer yamlTokenizer)
        {
        //     switch (reader.CurrentTokenType)
        //     {
        //         case TokenType.PlainScalar:
        //             if (reader.IsNull())
        //             {
        //                 reader.Read();
        //                 return null;
        //             }
        //             if (reader.TryGetBool(out var boolValue))
        //             {
        //                 reader.Read();
        //                 return boolValue;
        //             }
        //
        //             if (reader.TryGetInt64(out var longValue))
        //             {
        //                 reader.Read();
        //                 return longValue;
        //             }
        //
        //             if (reader.TryGetDouble(out var doubleValue))
        //             {
        //                 reader.Read();
        //                 return doubleValue;
        //             }
        //
        //             var stringValue = reader.GetString();
        //             reader.Read();
        //             return stringValue;
        //
        //         case TokenType.FoldedScalar:
        //         case TokenType.LiteralScalar:
        //         case TokenType.DoubleQuotedScaler:
        //         case TokenType.SingleQuotedScaler:
        //         {
        //             var value = reader.GetString();
        //             reader.Read();
        //             return value;
        //         }
        //
        //         case TokenType.FlowMappingStart:
        //         {
        //             var dict = new Dictionary<object, object>();
        //             reader.Read();
        //             while (reader.CurrentTokenType != TokenType.FlowMappingEnd)
        //             {
        //                 reader.ReadWithVerify(TokenType.KeyStart);
        //                 var key = Deserialize(ref reader);
        //
        //                 reader.ReadWithVerify(TokenType.ValueStart);
        //                 var value = Deserialize(ref reader);
        //                 dict.Add(key, value);
        //
        //                 if (reader.CurrentTokenType == TokenType.FlowEntryStart)
        //                 {
        //                     reader.Read();
        //                 }
        //                 else
        //                 {
        //                     break;
        //                 }
        //             }
        //             reader.ReadWithVerify(TokenType.FlowMappingEnd);
        //             return dict;
        //         }
        //
        //         case TokenType.BlockMappingStart:
        //         {
        //             var dict = new Dictionary<object, object>();
        //             reader.Read();
        //             while (reader.CurrentTokenType != TokenType.BlockEnd)
        //             {
        //                 reader.ReadWithVerify(TokenType.KeyStart);
        //                 var key = Deserialize(ref reader);
        //
        //                 reader.ReadWithVerify(TokenType.ValueStart);
        //                 var value = Deserialize(ref reader);
        //                 dict.Add(key, value);
        //             }
        //             reader.ReadWithVerify(TokenType.BlockEnd);
        //             return dict;
        //         }
        //
        //         case TokenType.FlowSequenceStart:
        //         {
        //             var list = new List<object>(4);
        //             reader.Read();
        //             while (reader.CurrentTokenType != TokenType.FlowSequenceEnd)
        //             {
        //                 var element = Deserialize(ref reader);
        //                 list.Add(element);
        //
        //                 if (reader.CurrentTokenType == TokenType.FlowEntryStart)
        //                 {
        //                     reader.Read();
        //                 }
        //                 else
        //                 {
        //                     break;
        //                 }
        //             }
        //             reader.ReadWithVerify(TokenType.FlowSequenceEnd);
        //             return list;
        //         }
        //
        //         case TokenType.BlockSequenceStart:
        //         {
        //             var list = new List<object>(4);
        //             reader.Read();
        //             while (reader.CurrentTokenType != TokenType.BlockEnd)
        //             {
        //                 reader.ReadWithVerify(TokenType.BlockEntryStart);
        //                 var element = Deserialize(ref reader);
        //                 list.Add(element);
        //             }
        //             reader.ReadWithVerify(TokenType.BlockEnd);
        //             return list;
        //         }
        //         default:
        //             return null;
        //    }
            throw new NotImplementedException();
        }
    }
}