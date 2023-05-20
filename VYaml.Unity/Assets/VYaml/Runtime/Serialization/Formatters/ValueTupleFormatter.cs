#nullable enable
using System;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class ValueTupleFormatter<T1> : IYamlFormatter<ValueTuple<T1>>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, ValueTuple<T1> value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            context.Serialize(ref emitter, value.Item1);
            emitter.EndSequence();
        }

        public ValueTuple<T1> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var item1 = context.DeserializeWithAlias<T1>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new ValueTuple<T1>(item1);
        }
    }

    public class ValueTupleFormatter<T1, T2> : IYamlFormatter<ValueTuple<T1, T2>>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, (T1, T2) value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            context.Serialize(ref emitter, value.Item1);
            context.Serialize(ref emitter, value.Item2);
            emitter.EndSequence();
        }

        public ValueTuple<T1, T2> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            parser.ReadWithVerify(ParseEventType.SequenceStart);
            var item1 = context.DeserializeWithAlias<T1>(ref parser);
            var item2 = context.DeserializeWithAlias<T2>(ref parser);
            parser.ReadWithVerify(ParseEventType.SequenceEnd);
            return new ValueTuple<T1, T2>(item1, item2);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3> : IYamlFormatter<ValueTuple<T1, T2, T3>>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, (T1, T2, T3) value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            context.Serialize(ref emitter, value.Item1);
            context.Serialize(ref emitter, value.Item2);
            context.Serialize(ref emitter, value.Item3);
            emitter.EndSequence();
        }

        public ValueTuple<T1, T2, T3> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
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
            return new ValueTuple<T1, T2, T3>(item1, item2, item3);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3, T4> : IYamlFormatter<ValueTuple<T1, T2, T3, T4>>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, (T1, T2, T3, T4) value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            context.Serialize(ref emitter, value.Item1);
            context.Serialize(ref emitter, value.Item2);
            context.Serialize(ref emitter, value.Item3);
            context.Serialize(ref emitter, value.Item4);
            emitter.EndSequence();
        }

        public ValueTuple<T1, T2, T3, T4> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
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
            return new ValueTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3, T4, T5> : IYamlFormatter<ValueTuple<T1, T2, T3, T4, T5>>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, (T1, T2, T3, T4, T5) value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            context.Serialize(ref emitter, value.Item1);
            context.Serialize(ref emitter, value.Item2);
            context.Serialize(ref emitter, value.Item3);
            context.Serialize(ref emitter, value.Item4);
            context.Serialize(ref emitter, value.Item5);
            emitter.EndSequence();
        }

        public ValueTuple<T1, T2, T3, T4, T5> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
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
            return new ValueTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3, T4, T5, T6> : IYamlFormatter<ValueTuple<T1, T2, T3, T4, T5, T6>>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, (T1, T2, T3, T4, T5, T6) value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            context.Serialize(ref emitter, value.Item1);
            context.Serialize(ref emitter, value.Item2);
            context.Serialize(ref emitter, value.Item3);
            context.Serialize(ref emitter, value.Item4);
            context.Serialize(ref emitter, value.Item5);
            context.Serialize(ref emitter, value.Item6);
            emitter.EndSequence();
        }

        public ValueTuple<T1, T2, T3, T4, T5, T6> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
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
            return new ValueTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7> : IYamlFormatter<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        public void Serialize(ref Utf8YamlEmitter emitter, (T1, T2, T3, T4, T5, T6, T7) value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            context.Serialize(ref emitter, value.Item1);
            context.Serialize(ref emitter, value.Item2);
            context.Serialize(ref emitter, value.Item3);
            context.Serialize(ref emitter, value.Item4);
            context.Serialize(ref emitter, value.Item5);
            context.Serialize(ref emitter, value.Item6);
            context.Serialize(ref emitter, value.Item7);
            emitter.EndSequence();
        }

        public ValueTuple<T1, T2, T3, T4, T5, T6, T7> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
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
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }
    }

    public class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest> : IYamlFormatter<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>
        where TRest : struct
    {
        public void Serialize(ref Utf8YamlEmitter emitter, ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> value, YamlSerializationContext context)
        {
            emitter.BeginSequence(SequenceStyle.Flow);
            context.Serialize(ref emitter, value.Item1);
            context.Serialize(ref emitter, value.Item2);
            context.Serialize(ref emitter, value.Item3);
            context.Serialize(ref emitter, value.Item4);
            context.Serialize(ref emitter, value.Item5);
            context.Serialize(ref emitter, value.Item6);
            context.Serialize(ref emitter, value.Item7);
            context.Serialize(ref emitter, value.Rest);
            emitter.EndSequence();
        }

        public ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
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
            return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(item1, item2, item3, item4, item5, item6, item7, item8);
        }
    }
}
