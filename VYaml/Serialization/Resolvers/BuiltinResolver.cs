using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VYaml.Serialization
{
    public class BuiltinResolver : IYamlFormatterResolver
    {
        static class FormatterCache<T>
        {
            public static readonly IYamlFormatter<T>? Formatter;

            static FormatterCache()
            {
                if (FormatterMap.TryGetValue(typeof(T), out var formatter))
                {
                    Formatter = (IYamlFormatter<T>)formatter;
                    return;
                }

                if (TryCreateGenericFormatter(typeof(T)) is IYamlFormatter<T> f)
                {
                    Formatter = f;
                    return;
                }

                Formatter = null;
            }
        }

        public static readonly BuiltinResolver Instance = new();

        static readonly Dictionary<Type, object> FormatterMap = new()
        {
            // Primitive
            { typeof(Int16), Int16Formatter.Instance },
            { typeof(Int32), Int32Formatter.Instance },
            { typeof(Int64), Int64Formatter.Instance },
            { typeof(UInt16), UInt16Formatter.Instance },
            { typeof(UInt32), UInt32Formatter.Instance },
            { typeof(UInt64), UInt64Formatter.Instance },
            { typeof(Single), Float32Formatter.Instance },
            { typeof(Double), Float64Formatter.Instance },
            { typeof(bool), BooleanFormatter.Instance },
            { typeof(byte), ByteFormatter.Instance },
            { typeof(sbyte), SByteFormatter.Instance },
            { typeof(DateTime), DateTimeFormatter.Instance },
            { typeof(char), CharFormatter.Instance },
            { typeof(byte[]), ByteArrayFormatter.Instance },

            // Nullable Primitive
            { typeof(Int16?), NullableInt16Formatter.Instance },
            { typeof(Int32?), NullableInt32Formatter.Instance },
            { typeof(Int64?), NullableInt64Formatter.Instance },
            { typeof(UInt16?), NullableUInt16Formatter.Instance },
            { typeof(UInt32?), NullableUInt32Formatter.Instance },
            { typeof(UInt64?), NullableUInt64Formatter.Instance },
            { typeof(Single?), NullableFloat32Formatter.Instance },
            { typeof(Double?), NullableFloat64Formatter.Instance },
            { typeof(bool?), NullableBooleanFormatter.Instance },
            { typeof(byte?), NullableByteFormatter.Instance },
            { typeof(sbyte?), NullableSByteFormatter.Instance },
            { typeof(DateTime?), NullableDateTimeFormatter.Instance },
            { typeof(char?), NullableCharFormatter.Instance },

            // StandardClassLibraryFormatter
            { typeof(string), NullableStringFormatter.Instance },
            { typeof(decimal), DecimalFormatter.Instance },
            { typeof(decimal?), new StaticNullableFormatter<decimal>(DecimalFormatter.Instance) },
            { typeof(TimeSpan), TimeSpanFormatter.Instance },
            { typeof(TimeSpan?), new StaticNullableFormatter<TimeSpan>(TimeSpanFormatter.Instance) },
            { typeof(DateTimeOffset), DateTimeOffsetFormatter.Instance },
            { typeof(DateTimeOffset?), new StaticNullableFormatter<DateTimeOffset>(DateTimeOffsetFormatter.Instance) },
            { typeof(Guid), GuidFormatter.Instance },
            { typeof(Guid?), new StaticNullableFormatter<Guid>(GuidFormatter.Instance) },
            { typeof(Uri), UriFormatter.Instance },
            { typeof(Version), VersionFormatter.Instance },
            { typeof(BitArray), BitArrayFormatter.Instance },
            { typeof(Type), TypeFormatter.Instance },

            // well known collections
            { typeof(List<Int16>), new ListFormatter<Int16>() },
            { typeof(List<Int32>), new ListFormatter<Int32>() },
            { typeof(List<Int64>), new ListFormatter<Int64>() },
            { typeof(List<UInt16>), new ListFormatter<UInt16>() },
            { typeof(List<UInt32>), new ListFormatter<UInt32>() },
            { typeof(List<UInt64>), new ListFormatter<UInt64>() },
            { typeof(List<Single>), new ListFormatter<Single>() },
            { typeof(List<Double>), new ListFormatter<Double>() },
            { typeof(List<Boolean>), new ListFormatter<Boolean>() },
            { typeof(List<byte>), new ListFormatter<byte>() },
            { typeof(List<SByte>), new ListFormatter<SByte>() },
            { typeof(List<DateTime>), new ListFormatter<DateTime>() },
            { typeof(List<Char>), new ListFormatter<Char>() },
            { typeof(List<string>), new ListFormatter<string>() },

            { typeof(object[]), new ArrayFormatter<object>() },
            { typeof(List<object>), new ListFormatter<object>() },

            { typeof(Memory<byte>), ByteMemoryFormatter.Instance },
            { typeof(Memory<byte>?), new StaticNullableFormatter<Memory<byte>>(ByteMemoryFormatter.Instance) },
            { typeof(ReadOnlyMemory<byte>), ByteReadOnlyMemoryFormatter.Instance },
            { typeof(ReadOnlyMemory<byte>?), new StaticNullableFormatter<ReadOnlyMemory<byte>>(ByteReadOnlyMemoryFormatter.Instance) },
            { typeof(ReadOnlySequence<byte>), ByteReadOnlySequenceFormatter.Instance },
            { typeof(ReadOnlySequence<byte>?), new StaticNullableFormatter<ReadOnlySequence<byte>>(ByteReadOnlySequenceFormatter.Instance) },
            { typeof(ArraySegment<byte>), ByteArraySegmentFormatter.Instance },
            { typeof(ArraySegment<byte>?), new StaticNullableFormatter<ArraySegment<byte>>(ByteArraySegmentFormatter.Instance) },

            { typeof(System.Numerics.BigInteger), BigIntegerFormatter.Instance },
            { typeof(System.Numerics.BigInteger?), new StaticNullableFormatter<System.Numerics.BigInteger>(BigIntegerFormatter.Instance) },
            { typeof(System.Numerics.Complex), ComplexFormatter.Instance },
            { typeof(System.Numerics.Complex?), new StaticNullableFormatter<System.Numerics.Complex>(ComplexFormatter.Instance) },
        };

        public static readonly Dictionary<Type, Type> KnownGenericTypes = new()
        {
            { typeof(Tuple<>), typeof(TupleFormatter<>) },
            { typeof(ValueTuple<>), typeof(ValueTupleFormatter<>) },
            { typeof(Tuple<,>), typeof(TupleFormatter<,>) },
            { typeof(ValueTuple<,>), typeof(ValueTupleFormatter<,>) },
            { typeof(Tuple<,,>), typeof(TupleFormatter<,,>) },
            { typeof(ValueTuple<,,>), typeof(ValueTupleFormatter<,,>) },
            { typeof(Tuple<,,,>), typeof(TupleFormatter<,,,>) },
            { typeof(ValueTuple<,,,>), typeof(ValueTupleFormatter<,,,>) },
            { typeof(Tuple<,,,,>), typeof(TupleFormatter<,,,,>) },
            { typeof(ValueTuple<,,,,>), typeof(ValueTupleFormatter<,,,,>) },
            { typeof(Tuple<,,,,,>), typeof(TupleFormatter<,,,,,>) },
            { typeof(ValueTuple<,,,,,>), typeof(ValueTupleFormatter<,,,,,>) },
            { typeof(Tuple<,,,,,,>), typeof(TupleFormatter<,,,,,,>) },
            { typeof(ValueTuple<,,,,,,>), typeof(ValueTupleFormatter<,,,,,,>) },
            { typeof(Tuple<,,,,,,,>), typeof(TupleFormatter<,,,,,,,>) },
            { typeof(ValueTuple<,,,,,,,>), typeof(ValueTupleFormatter<,,,,,,,>) },

            { typeof(KeyValuePair<,>), typeof(KeyValuePairFormatter<,>) },
            // { typeof(Lazy<>), typeof(LazyFormatter<>) },
            { typeof(Nullable<>), typeof(NullableFormatter<>) },

            // { typeof(ArraySegment<>), typeof(ArraySegmentFormatter<>) },
            // { typeof(Memory<>), typeof(MemoryFormatter<>) },
            // { typeof(ReadOnlyMemory<>), typeof(ReadOnlyMemoryFormatter<>) },
            // { typeof(ReadOnlySequence<>), typeof(ReadOnlySequenceFormatter<>) },

            { typeof(List<>), typeof(ListFormatter<>) },
            { typeof(Stack<>), typeof(StackFormatter<>) },
            { typeof(Queue<>), typeof(QueueFormatter<>) },
            { typeof(LinkedList<>), typeof(LinkedListFormatter<>) },
            { typeof(HashSet<>), typeof(HashSetFormatter<>) },
            { typeof(SortedSet<>), typeof(SortedSetFormatter<>) },

            // { typeof(ObservableCollection<>), typeof(ObservableCollectionFormatter<>) },
            // { typeof(Collection<>), typeof(CollectionFormatter<>) },
            { typeof(BlockingCollection<>), typeof(BlockingCollectionFormatter<>) },
            { typeof(ConcurrentQueue<>), typeof(ConcurrentQueueFormatter<>) },
            { typeof(ConcurrentStack<>), typeof(ConcurrentStackFormatter<>) },
            { typeof(ConcurrentBag<>), typeof(ConcurrentBagFormatter<>) },
            { typeof(Dictionary<,>), typeof(DictionaryFormatter<,>) },
            // { typeof(SortedDictionary<,>), typeof(SortedDictionaryFormatter<,>) },
            // { typeof(SortedList<,>), typeof(SortedListFormatter<,>) },
            // { typeof(ConcurrentDictionary<,>), typeof(ConcurrentDictionaryFormatter<,>) },
            // { typeof(ReadOnlyCollection<>), typeof(ReadOnlyCollectionFormatter<>) },
            // { typeof(ReadOnlyObservableCollection<>), typeof(ReadOnlyObservableCollectionFormatter<>) },

            { typeof(IEnumerable<>), typeof(InterfaceEnumerableFormatter<>) },
            { typeof(ICollection<>), typeof(InterfaceCollectionFormatter<>) },
            { typeof(IReadOnlyCollection<>), typeof(InterfaceReadOnlyCollectionFormatter<>) },
            { typeof(IList<>), typeof(InterfaceListFormatter<>) },
            { typeof(IReadOnlyList<>), typeof(InterfaceReadOnlyListFormatter<>) },
            { typeof(IDictionary<,>), typeof(InterfaceDictionaryFormatter<,>) },
            { typeof(IReadOnlyDictionary<,>), typeof(InterfaceReadOnlyDictionaryFormatter<,>) },
            { typeof(ISet<>), typeof(InterfaceSetFormatter<>) },
            // { typeof(ILookup<,>), typeof(InterfaceLookupFormatter<,>) },
            // { typeof(IGrouping<,>), typeof(InterfaceGroupingFormatter<,>) },
        };

        public IYamlFormatter<T>? GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        static object? TryCreateGenericFormatter(Type type)
        {
            Type? formatterType = null;

            if (type.IsArray)
            {
                if (type.IsSZArray)
                {
                    formatterType = typeof(ArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                }
                else
                {
                    var rank = type.GetArrayRank();
                    switch (rank)
                    {
                        case 2:

                            formatterType = typeof(TwoDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                            break;
                        case 3:
                            formatterType = typeof(ThreeDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                            break;
                        case 4:
                            formatterType = typeof(FourDimensionalArrayFormatter<>).MakeGenericType(type.GetElementType()!);
                            break;
                        default:
                            break; // not supported
                    }
                }
            }
            else if (type.IsEnum)
            {
                formatterType = typeof(EnumAsStringFormatter<>).MakeGenericType(type);
            }
            else
            {
                formatterType = TryCreateGenericFormatterType(type, KnownGenericTypes);
            }

            if (formatterType != null)
            {
                return Activator.CreateInstance(formatterType);
            }
            return null;
        }

        static Type? TryCreateGenericFormatterType(Type type, IDictionary<Type, Type> knownTypes)
        {
            if (type.IsGenericType)
            {
                var genericDefinition = type.GetGenericTypeDefinition();

                if (knownTypes.TryGetValue(genericDefinition, out var formatterType))
                {
                    return formatterType.MakeGenericType(type.GetGenericArguments());
                }
            }
            return null;
        }
    }
}
