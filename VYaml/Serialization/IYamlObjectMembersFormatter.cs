using System;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    /// <summary>
    /// Implemented by source-generated formatters of [YamlObject] types: reads/writes the members of the
    /// mapping without its start/end. Used by unions with DiscriminatorKey / RequiredKey ([YamlObjectUnion]),
    /// whose keys are in the same mapping as the subtype's members.
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
                $"{typeof(T)} is used as a subtype of a union with DiscriminatorKey or RequiredKey, but its formatter does not support it. The subtype must be a [YamlObject] type that is not a union.");
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
