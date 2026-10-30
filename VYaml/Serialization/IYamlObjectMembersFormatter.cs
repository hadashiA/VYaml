using System;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    /// <summary>
    /// Implemented by source-generated formatters of [YamlObject] types: reads/writes the members of the
    /// mapping without its start/end. Used by unions with DiscriminatorKey ([YamlObjectUnion]), whose key is in the
    /// same mapping as the subtype's members, and by <see cref="YamlUnionStyle.Wrapped"/> unions for an empty value.
    /// </summary>
    public interface IYamlObjectMembersFormatter<T>
    {
        /// <summary>
        /// Writes the key/value pairs of the members. The caller writes the mapping start/end.
        /// </summary>
        void SerializeMembers(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context);

        /// <summary>
        /// Reads the remaining key/value pairs after the mapping start, and the mapping end.
        /// </summary>
        /// <param name="discriminatorKeyUtf8">
        /// The discriminator key of the union, which is skipped even with <see cref="YamlUnmappedMemberHandling.Disallow"/>.
        /// Empty if none.
        /// </param>
        T DeserializeMembers(ref YamlParser parser, YamlDeserializationContext context, ReadOnlySpan<byte> discriminatorKeyUtf8);

        /// <summary>
        /// Reads the current null scalar (<c>key:</c> with no value, see <see cref="YamlUnionStyle.Wrapped"/>) as an
        /// empty mapping: every member takes its default value, and required members are reported as missing.
        /// </summary>
        T DeserializeEmpty(ref YamlParser parser, YamlDeserializationContext context);
    }

    public static class YamlObjectMembersFormatter
    {
        public static IYamlObjectMembersFormatter<T> GetWithVerify<T>(IYamlFormatterResolver resolver)
        {
            if (resolver.GetFormatterWithVerify<T>() is IYamlObjectMembersFormatter<T> formatter)
            {
                return formatter;
            }
            throw new YamlSerializerException(
                $"{typeof(T)} is used as a subtype of a union with DiscriminatorKey or Style = Wrapped, but its formatter does not support it. The subtype must be a [YamlObject] type that is not a union.");
        }

        /// <summary>
        /// Reads the value of a <see cref="YamlUnionStyle.Wrapped"/> union: the mapping of the subtype, or an empty
        /// value (<c>key:</c>) as an empty mapping.
        /// </summary>
        /// <param name="parser">Positioned at the value.</param>
        public static T DeserializeWrapped<T>(ref YamlParser parser, YamlDeserializationContext context)
        {
            var formatter = GetWithVerify<T>(context.Resolver);
            if (parser.IsNullScalar())
            {
                return formatter.DeserializeEmpty(ref parser, context);
            }
            parser.ReadWithVerify(ParseEventType.MappingStart);
            return formatter.DeserializeMembers(ref parser, context, default);
        }

        /// <summary>
        /// Skips a key/value pair whose key does not match any member, following <see cref="YamlSerializerOptions.UnmappedMemberHandling"/>.
        /// </summary>
        /// <param name="parser">Positioned at the key.</param>
        public static void SkipUnmappedMember(
            ref YamlParser parser,
            YamlDeserializationContext context,
            ReadOnlySpan<byte> discriminatorKeyUtf8,
            string typeName)
        {
            if (context.Options.UnmappedMemberHandling == YamlUnmappedMemberHandling.Disallow &&
                !(parser.TryGetScalarAsSpan(out var key) && !discriminatorKeyUtf8.IsEmpty && key.SequenceEqual(discriminatorKeyUtf8)))
            {
                throw new YamlSerializerException(parser.CurrentMark, $"Unknown key '{parser.GetScalarAsString()}' for {typeName}");
            }
            parser.Read(); // skip key
            parser.SkipCurrentNode(); // skip value
        }
    }
}
