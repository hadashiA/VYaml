#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace VYaml.Internal
{
    public static class YamlCodes
    {
        public static readonly byte[] YamlDirectiveName = { (byte)'Y', (byte)'A', (byte)'M', (byte)'L' };
        public static readonly byte[] TagDirectiveName = { (byte)'T', (byte)'A', (byte)'G' };

        public static readonly byte[] Utf8Bom = { 0xEF, 0xBB, 0xBF };
        public static readonly byte[] StreamStart = { (byte)'-', (byte)'-', (byte)'-' };
        public static readonly byte[] DocStart = { (byte)'.', (byte)'.', (byte)'.' };
        public static readonly byte[] CrLf = { Cr, Lf };

        public static readonly byte[] Null0 = { (byte)'n', (byte)'u', (byte)'l', (byte)'l' };
        public static readonly byte[] Null1 = { (byte)'N', (byte)'u', (byte)'l', (byte)'l' };
        public static readonly byte[] Null2 = { (byte)'N', (byte)'U', (byte)'L', (byte)'L' };
        public const byte NullAlias = (byte)'~';

        public static readonly byte[] True0 = { (byte)'t', (byte)'r', (byte)'u', (byte)'e' };
        public static readonly byte[] True1 = { (byte)'T', (byte)'r', (byte)'u', (byte)'e' };
        public static readonly byte[] True2 = { (byte)'T', (byte)'R', (byte)'U', (byte)'E' };

        public static readonly byte[] False0 = { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };
        public static readonly byte[] False1 = { (byte)'F', (byte)'a', (byte)'l', (byte)'s', (byte)'e' };
        public static readonly byte[] False2 = { (byte)'F', (byte)'A', (byte)'L', (byte)'S', (byte)'E' };

        public static readonly byte[] Inf0 = { (byte)'.', (byte)'i', (byte)'n', (byte)'f' };
        public static readonly byte[] Inf1 = { (byte)'.', (byte)'I', (byte)'n', (byte)'f' };
        public static readonly byte[] Inf2 = { (byte)'.', (byte)'I', (byte)'N', (byte)'F' };
        public static readonly byte[] Inf3 = { (byte)'+', (byte)'.', (byte)'i', (byte)'n', (byte)'f' };
        public static readonly byte[] Inf4 = { (byte)'+', (byte)'.', (byte)'I', (byte)'n', (byte)'f' };
        public static readonly byte[] Inf5 = { (byte)'+', (byte)'.', (byte)'I', (byte)'N', (byte)'F' };

        public static readonly byte[] Yes0 = { (byte)'y', (byte)'e', (byte)'s' };
        public static readonly byte[] Yes1 = { (byte)'Y', (byte)'e', (byte)'s' };
        public static readonly byte[] Yes2 = { (byte)'Y', (byte)'E', (byte)'S' };

        public static readonly byte[] No0 = { (byte)'n', (byte)'o' };
        public static readonly byte[] No1 = { (byte)'N', (byte)'o' };
        public static readonly byte[] No2 = { (byte)'N', (byte)'O' };

        public static readonly byte[] On0 = { (byte)'o', (byte)'n' };
        public static readonly byte[] On1 = { (byte)'O', (byte)'n' };
        public static readonly byte[] On2 = { (byte)'O', (byte)'N' };

        public static readonly byte[] Off0 = { (byte)'o', (byte)'f', (byte)'f' };
        public static readonly byte[] Off1 = { (byte)'O', (byte)'f', (byte)'f' };
        public static readonly byte[] Off2 = { (byte)'O', (byte)'F', (byte)'F' };

        public static readonly byte[] NegInf0 = { (byte)'-', (byte)'.', (byte)'i', (byte)'n', (byte)'f' };
        public static readonly byte[] NegInf1 = { (byte)'-', (byte)'.', (byte)'I', (byte)'n', (byte)'f' };
        public static readonly byte[] NegInf2 = { (byte)'-', (byte)'.', (byte)'I', (byte)'N', (byte)'F' };

        public static readonly byte[] Nan0 = { (byte)'.', (byte)'n', (byte)'a', (byte)'n' };
        public static readonly byte[] Nan1 = { (byte)'.', (byte)'N', (byte)'a', (byte)'N' };
        public static readonly byte[] Nan2 = { (byte)'.', (byte)'N', (byte)'A', (byte)'N' };

        public static readonly byte[] HexPrefix = { (byte)'0', (byte)'x' };
        public static readonly byte[] HexPrefixNegative = { (byte)'-', (byte)'0', (byte)'x' };

        public static readonly byte[] OctalPrefix = { (byte)'0', (byte)'o' };

        public static readonly byte[] UnityStrippedSymbol =
        {
            (byte)'s', (byte)'t', (byte)'r', (byte)'i', (byte)'p', (byte)'p', (byte)'e', (byte)'d'
        };

        public const byte Space = (byte)' ';
        public const byte Tab = (byte)'\t';
        public const byte Lf = (byte)'\n';
        public const byte Cr = (byte)'\r';
        public const byte Comment = (byte)'#';
        public const byte DirectiveLine = (byte)'%';
        public const byte Alias = (byte)'*';
        public const byte Anchor = (byte)'&';
        public const byte Tag = (byte)'!';
        public const byte SingleQuote = (byte)'\'';
        public const byte DoubleQuote = (byte)'"';
        public const byte LiteralScalerHeader = (byte)'|';
        public const byte FoldedScalerHeader = (byte)'>';
        public const byte Comma = (byte)',';
        public const byte BlockEntryIndent = (byte)'-';
        public const byte ExplicitKeyIndent = (byte)'?';
        public const byte MapValueIndent = (byte)':';
        public const byte FlowMapStart = (byte)'{';
        public const byte FlowMapEnd = (byte)'}';
        public const byte FlowSequenceStart = (byte)'[';
        public const byte FlowSequenceEnd = (byte)']';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphaNumericDashOrUnderscore(byte code) => code is
            >= (byte)'0' and <= (byte)'9' or
            >= (byte)'A' and <= (byte)'Z' or
            >= (byte)'a' and <= (byte)'z' or
            (byte)'_' or
            (byte)'-';

        // Spec: https://yaml.org/spec/1.2.2/#rule-ns-word-char
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWordChar(byte code) => code is
            >= (byte)'0' and <= (byte)'9' or
            >= (byte)'A' and <= (byte)'Z' or
            >= (byte)'a' and <= (byte)'z' or
            (byte)'-';

        // Spec: https://yaml.org/spec/1.2.2/#rule-ns-uri-char
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUriChar(byte code) => code is
            >= (byte)'0' and <= (byte)'9' or
            >= (byte)'A' and <= (byte)'Z' or
            >= (byte)'a' and <= (byte)'z' or
            (byte)'-' or
            (byte)'#' or
            (byte)';' or
            (byte)'/' or
            (byte)'?' or
            (byte)':' or
            (byte)'@' or
            (byte)'&' or
            (byte)'=' or
            (byte)'+' or
            (byte)'$' or
            (byte)',' or
            (byte)'_' or
            (byte)'.' or
            (byte)'!' or
            (byte)'~' or
            (byte)'*' or
            (byte)'\'' or
            (byte)'(' or
            (byte)')' or
            (byte)'[' or
            (byte)']';

        // Spec: https://yaml.org/spec/1.2.2/#rule-ns-tag-char
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTagChar(byte code) => code is
            >= (byte)'0' and <= (byte)'9' or
            >= (byte)'A' and <= (byte)'Z' or
            >= (byte)'a' and <= (byte)'z' or
            (byte)'-' or
            (byte)'#' or
            (byte)';' or
            (byte)'/' or
            (byte)'?' or
            (byte)':' or
            (byte)'@' or
            (byte)'&' or
            (byte)'=' or
            (byte)'+' or
            (byte)'$' or
            // (byte)',' or
            (byte)'_' or
            (byte)'.' or
            // (byte)'!' or
            (byte)'~' or
            (byte)'*' or
            (byte)'\'' // or
            // (byte)'(' or
            // (byte)')' or
            // (byte)'[' or
            // (byte)']'
            ;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAscii(byte code) => code <= '\x7F';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumber(byte code) => code is >= (byte)'0' and <= (byte)'9';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(byte code) => code is Space or Tab or Lf or Cr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLineBreak(byte code) => code is Lf or Cr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlank(byte code) => code is Space or Tab;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumberRepresentation(byte code) => code is
            >= (byte)'0' and <= (byte)'9' or
            (byte)'+' or (byte)'-' or (byte)'.';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHex(byte code) => code is
            >= (byte)'0' and <= (byte)'9' or
            >= (byte)'A' and <= (byte)'F' or
            >= (byte)'a' and <= (byte)'f';

        // Spec: https://yaml.org/spec/1.2.2/#rule-c-flow-indicator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyFlowSymbol(byte code) => code is
            (byte)',' or (byte)'[' or (byte)']' or (byte)'{' or (byte)'}';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte AsHex(byte code) => code switch
        {
            >= (byte)'0' and <= (byte)'9' => (byte)(code - (byte)'0'),
            >= (byte)'a' and <= (byte)'f' => (byte)(code - (byte)'a' + 10),
            >= (byte)'A' and <= (byte)'F' => (byte)(code - (byte)'A' + 10),
            _ => throw new InvalidOperationException()
        };
    }
}
