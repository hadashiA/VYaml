using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public sealed class TwoDimensionalArrayFormatter<T> : IYamlFormatter<T[,]?>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, T[,]? value, YamlSerializationContext context)
        {
            if (value == null)
            {
                emitter.WriteNull();
            }
            else
            {
                var formatter = context.Resolver.GetFormatterWithVerify<T>();
                emitter.BeginSequence();
                for (var i = 0; i < value.GetLength(0); i++)
                {
                    emitter.BeginSequence();
                    for (var j = 0; j < value.GetLength(1); j++)
                    {
                        formatter.Serialize(ref emitter, value[i, j], context);
                    }
                    emitter.EndSequence();
                }
                emitter.EndSequence();
            }
        }

        public T[,]? Deserialize(ref YamlParser parser, YamlDeserializationContext options)
        {
            if (parser.IsNullScalar())
            {
                return null;
            }

            var formatter = options.Resolver.GetFormatterWithVerify<T>();

            var list = new List<List<T>>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            while (parser.CurrentEventType != ParseEventType.SequenceEnd)
            {
                var innerList = new List<T>();
                parser.ReadWithVerify(ParseEventType.SequenceStart);
                while (parser.CurrentEventType != ParseEventType.SequenceEnd)
                {
                    innerList.Add(options.DeserializeWithAlias(formatter, ref parser));
                }
                parser.ReadWithVerify(ParseEventType.SequenceEnd);
                list.Add(innerList);
            }
            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            var result = new T[list.Count, list.Count > 0 ? list[0].Count : 0];
            for (var i = 0; i < list.Count; i++)
            {
                for (var j = 0; j < list[i].Count; j++)
                {
                    result[i, j] = list[i][j];
                }
            }
            return result;
        }
    }

    public sealed class ThreeDimensionalArrayFormatter<T> : IYamlFormatter<T[,,]?>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, T[,,]? value, YamlSerializationContext context)
        {
            if (value == null)
            {
                emitter.WriteNull();
            }
            else
            {
                var formatter = context.Resolver.GetFormatterWithVerify<T>();
                emitter.BeginSequence();
                for (var i = 0; i < value.GetLength(0); i++)
                {
                    emitter.BeginSequence();
                    for (var j = 0; j < value.GetLength(1); j++)
                    {
                        emitter.BeginSequence();
                        for (var k = 0; k < value.GetLength(2); k++)
                        {
                            formatter.Serialize(ref emitter, value[i, j, k], context);
                        }
                        emitter.EndSequence();
                    }
                    emitter.EndSequence();
                }
                emitter.EndSequence();
            }
        }

        public T[,,]? Deserialize(ref YamlParser parser, YamlDeserializationContext options)
        {
            if (parser.IsNullScalar())
            {
                return null;
            }

            var formatter = options.Resolver.GetFormatterWithVerify<T>();

            var list = new List<List<List<T>>>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            while (parser.CurrentEventType != ParseEventType.SequenceEnd)
            {
                var innerList1 = new List<List<T>>();
                parser.ReadWithVerify(ParseEventType.SequenceStart);
                while (parser.CurrentEventType != ParseEventType.SequenceEnd)
                {
                    var innerList2 = new List<T>();
                    parser.ReadWithVerify(ParseEventType.SequenceStart);
                    while (parser.CurrentEventType != ParseEventType.SequenceEnd)
                    {
                        innerList2.Add(options.DeserializeWithAlias(formatter, ref parser));
                    }

                    parser.ReadWithVerify(ParseEventType.SequenceEnd);
                    innerList1.Add(innerList2);
                }

                parser.ReadWithVerify(ParseEventType.SequenceEnd);
                list.Add(innerList1);
            }

            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            var length0 = list.Count;
            var length1 = length0 > 0 ? list[0].Count : 0;
            var length2 = length1 > 0 ? list[0][0].Count : 0;
            var result = new T[length0, length1, length2];
            for (var i = 0; i < list.Count; i++)
            {
                for (var j = 0; j < list[i].Count; j++)
                {
                    for (var k = 0; k < list[i][j].Count; k++)
                    {
                        result[i, j, k] = list[i][j][k];
                    }
                }
            }

            return result;
        }
    }

    public sealed class FourDimensionalArrayFormatter<T> : IYamlFormatter<T[,,,]?>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, T[,,,]? value, YamlSerializationContext context)
        {
            if (value == null)
            {
                emitter.WriteNull();
            }
            else
            {
                var formatter = context.Resolver.GetFormatterWithVerify<T>();
                emitter.BeginSequence();
                for (var i = 0; i < value.GetLength(0); i++)
                {
                    emitter.BeginSequence();
                    for (var j = 0; j < value.GetLength(1); j++)
                    {
                        emitter.BeginSequence();
                        for (var k = 0; k < value.GetLength(2); k++)
                        {
                            emitter.BeginSequence();
                            for (var l = 0; l < value.GetLength(3); l++)
                            {
                                formatter.Serialize(ref emitter, value[i, j, k, l], context);
                            }
                            emitter.EndSequence();
                        }
                        emitter.EndSequence();
                    }
                    emitter.EndSequence();
                }
                emitter.EndSequence();
            }
        }

        public T[,,,]? Deserialize(ref YamlParser parser, YamlDeserializationContext options)
        {
            if (parser.IsNullScalar())
            {
                return null;
            }

            var formatter = options.Resolver.GetFormatterWithVerify<T>();

            var list = new List<List<List<List<T>>>>();

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            while (parser.CurrentEventType != ParseEventType.SequenceEnd)
            {
                var innerList1 = new List<List<List<T>>>();
                parser.ReadWithVerify(ParseEventType.SequenceStart);
                while (parser.CurrentEventType != ParseEventType.SequenceEnd)
                {
                    var innerList2 = new List<List<T>>();
                    parser.ReadWithVerify(ParseEventType.SequenceStart);
                    while (parser.CurrentEventType != ParseEventType.SequenceEnd)
                    {
                        var innerList3 = new List<T>();
                        parser.ReadWithVerify(ParseEventType.SequenceStart);
                        while (parser.CurrentEventType != ParseEventType.SequenceEnd)
                        {
                            innerList3.Add(options.DeserializeWithAlias(formatter, ref parser));
                        }
                        parser.ReadWithVerify(ParseEventType.SequenceEnd);
                        innerList2.Add(innerList3);
                    }
                    parser.ReadWithVerify(ParseEventType.SequenceEnd);
                    innerList1.Add(innerList2);
                }
                parser.ReadWithVerify(ParseEventType.SequenceEnd);
                list.Add(innerList1);
            }

            parser.ReadWithVerify(ParseEventType.SequenceEnd);

            var length0 = list.Count;
            var length1 = length0 > 0 ? list[0].Count : 0;
            var length2 = length1 > 0 ? list[0][0].Count : 0;
            var length3 = length2 > 0 ? list[0][0][0].Count : 0;
            var result = new T[length0, length1, length2, length3];
            for (var i = 0; i < list.Count; i++)
            {
                for (var j = 0; j < list[i].Count; j++)
                {
                    for (var k = 0; k < list[i][j].Count; k++)
                    {
                        for (var l = 0; l < list[i][j][k].Count; l++)
                        {
                            result[i, j, k, l] = list[i][j][k][l];
                        }
                    }
                }
            }
            return result;
        }
    }
}