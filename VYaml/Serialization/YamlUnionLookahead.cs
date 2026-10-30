using System;
using VYaml.Annotations;
using VYaml.Parser;

namespace VYaml.Serialization
{
    /// <summary>
    /// Used by source-generated union formatters to identify the subtype from the keys of a mapping
    /// before deserializing it. Each method scans the rest of the current mapping on a clone of the
    /// parser, so the given parser does not move.
    /// </summary>
    public static class YamlUnionLookahead
    {
        /// <summary>
        /// Finds the scalar value of <paramref name="keyUtf8"/> in the current mapping.
        /// </summary>
        /// <param name="parser">Positioned inside a mapping (after the mapping start).</param>
        /// <returns>false if the key is not found or its value is not a scalar.</returns>
        public static bool TryFindScalarValue(ref YamlParser parser, ReadOnlySpan<byte> keyUtf8, out string? value)
        {
            var lookahead = new YamlParser(ref parser);
            try
            {
                while (!lookahead.End && lookahead.CurrentEventType != ParseEventType.MappingEnd)
                {
                    if (lookahead.CurrentEventType == ParseEventType.Scalar &&
                        lookahead.TryGetScalarAsSpan(out var key) &&
                        key.SequenceEqual(keyUtf8))
                    {
                        lookahead.Read(); // skip key
                        if (lookahead.CurrentEventType == ParseEventType.Scalar)
                        {
                            value = lookahead.ReadScalarAsString();
                            return true;
                        }
                        break;
                    }
                    lookahead.SkipCurrentNode(); // key
                    lookahead.SkipCurrentNode(); // value
                }
                value = null;
                return false;
            }
            finally
            {
                lookahead.ReturnPool();
            }
        }

        /// <summary>
        /// Finds which of <paramref name="keysUtf8"/> are contained in the current mapping, and returns the
        /// smallest index of them. Keys are compared in the same way as the members of a generated formatter:
        /// when <see cref="YamlSerializerOptions.NamingConvention"/> differs from the naming convention of the
        /// key, the key in the YAML is mutated to it before comparison.
        /// </summary>
        /// <param name="parser">Positioned inside a mapping (after the mapping start).</param>
        /// <returns>-1 if none of the keys are found.</returns>
        public static int FindFirstKey(
            ref YamlParser parser,
            YamlDeserializationContext context,
            byte[][] keysUtf8,
            NamingConvention[] namingConventions)
        {
            if (keysUtf8.Length <= 0) return -1;

            // fast path: the first key of the mapping is the most preferred one
            if (parser.CurrentEventType == ParseEventType.Scalar &&
                parser.TryGetScalarAsSpan(out var firstKey) &&
                KeyEquals(firstKey, keysUtf8[0], namingConventions[0], context))
            {
                return 0;
            }

            var result = -1;
            var lookahead = new YamlParser(ref parser);
            try
            {
                while (!lookahead.End && lookahead.CurrentEventType != ParseEventType.MappingEnd)
                {
                    if (lookahead.CurrentEventType == ParseEventType.Scalar &&
                        lookahead.TryGetScalarAsSpan(out var key))
                    {
                        var end = result < 0 ? keysUtf8.Length : result;
                        for (var i = 0; i < end; i++)
                        {
                            if (KeyEquals(key, keysUtf8[i], namingConventions[i], context))
                            {
                                result = i;
                                break;
                            }
                        }
                        if (result == 0) break;
                    }
                    lookahead.SkipCurrentNode(); // key
                    lookahead.SkipCurrentNode(); // value
                }
                return result;
            }
            finally
            {
                lookahead.ReturnPool();
            }
        }

        static bool KeyEquals(ReadOnlySpan<byte> key, byte[] expected, NamingConvention namingConvention, YamlDeserializationContext context)
        {
            if (context.Options.NamingConvention == namingConvention)
            {
                return key.SequenceEqual(expected);
            }
            NamingConventionMutator.MutateToThreadStaticBufferUtf8(key, namingConvention, out var mutated, out var written);
            return mutated.AsSpan(0, written).SequenceEqual(expected);
        }
    }
}
