using System;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class ValueTupleFormatter<T1> : IYamlFormatter<ValueTuple<T1?>>
    {
        public ValueTuple<T1?> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var item1 = context.DeserializeWithAlias<T1>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new ValueTuple<T1?>(item1);
        }
    }

    public class ValueTupleFormatter<T1, T2> : IYamlFormatter<ValueTuple<T1?, T2?>>
    {
        public ValueTuple<T1?, T2?> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var item1 = context.DeserializeWithAlias<T1>(ref parser);
            var item2 = context.DeserializeWithAlias<T2>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new ValueTuple<T1?, T2?>(item1, item2);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3> : IYamlFormatter<ValueTuple<T1?, T2?, T3?>>
    {
        public ValueTuple<T1?, T2?, T3?> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var item1 = context.DeserializeWithAlias<T1>(ref parser);
            var item2 = context.DeserializeWithAlias<T2>(ref parser);
            var item3 = context.DeserializeWithAlias<T3>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new ValueTuple<T1?, T2?, T3?>(item1, item2, item3);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3, T4> : IYamlFormatter<ValueTuple<T1?, T2?, T3?, T4?>>
    {
        public ValueTuple<T1?, T2?, T3?, T4?> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var item1 = context.DeserializeWithAlias<T1>(ref parser);
            var item2 = context.DeserializeWithAlias<T2>(ref parser);
            var item3 = context.DeserializeWithAlias<T3>(ref parser);
            var item4 = context.DeserializeWithAlias<T4>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new ValueTuple<T1?, T2?, T3?, T4?>(item1, item2, item3, item4);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3, T4, T5> : IYamlFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
    {
        public ValueTuple<T1?, T2?, T3?, T4?, T5?> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var item1 = context.DeserializeWithAlias<T1>(ref parser);
            var item2 = context.DeserializeWithAlias<T2>(ref parser);
            var item3 = context.DeserializeWithAlias<T3>(ref parser);
            var item4 = context.DeserializeWithAlias<T4>(ref parser);
            var item5 = context.DeserializeWithAlias<T5>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new ValueTuple<T1?, T2?, T3?, T4?, T5?>(item1, item2, item3, item4, item5);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3, T4, T5, T6> : IYamlFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
    {
        public ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var item1 = context.DeserializeWithAlias<T1>(ref parser);
            var item2 = context.DeserializeWithAlias<T2>(ref parser);
            var item3 = context.DeserializeWithAlias<T3>(ref parser);
            var item4 = context.DeserializeWithAlias<T4>(ref parser);
            var item5 = context.DeserializeWithAlias<T5>(ref parser);
            var item6 = context.DeserializeWithAlias<T6>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(item1, item2, item3, item4, item5, item6);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7> : IYamlFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
    {
        public ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var item1 = context.DeserializeWithAlias<T1>(ref parser);
            var item2 = context.DeserializeWithAlias<T2>(ref parser);
            var item3 = context.DeserializeWithAlias<T3>(ref parser);
            var item4 = context.DeserializeWithAlias<T4>(ref parser);
            var item5 = context.DeserializeWithAlias<T5>(ref parser);
            var item6 = context.DeserializeWithAlias<T6>(ref parser);
            var item7 = context.DeserializeWithAlias<T7>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(item1, item2, item3, item4, item5, item6, item7);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest> : IYamlFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>
        where TRest : struct
    {
        public ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var item1 = context.DeserializeWithAlias<T1>(ref parser);
            var item2 = context.DeserializeWithAlias<T2>(ref parser);
            var item3 = context.DeserializeWithAlias<T3>(ref parser);
            var item4 = context.DeserializeWithAlias<T4>(ref parser);
            var item5 = context.DeserializeWithAlias<T5>(ref parser);
            var item6 = context.DeserializeWithAlias<T6>(ref parser);
            var item7 = context.DeserializeWithAlias<T7>(ref parser);
            var item8 = context.DeserializeWithAlias<TRest>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(item1, item2, item3, item4, item5, item6, item7, item8);
        }
    }
}