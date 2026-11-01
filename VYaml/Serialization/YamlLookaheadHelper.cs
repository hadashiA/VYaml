using System;
using VYaml.Parser;

namespace VYaml.Serialization
{
    /// <summary>
    /// Inspects the rest of the current mapping without consuming it. Each method scans on a clone of
    /// the parser, so the given parser does not move. Used by source-generated union formatters to
    /// identify the subtype before deserializing it.
    /// </summary>
    public static class YamlLookaheadHelper
    {
        /// <summary>
        /// Finds the scalar value of <paramref name="keyUtf8"/> in the current mapping.
        /// </summary>
        /// <param name="parser">Positioned inside a mapping (after the mapping start).</param>
        /// <returns>false if the key is not found or its value is not a scalar.</returns>
        public static bool TryFindScalarValueInMapping(ref YamlParser parser, ReadOnlySpan<byte> keyUtf8, out string? value)
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
    }
}
