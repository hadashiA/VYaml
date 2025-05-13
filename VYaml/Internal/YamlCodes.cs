#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace VYaml.Internal
{
    public static class YamlCodes
    {
        public static readonly byte[] YamlDirectiveName = "YAML"u8.ToArray();
        public static readonly byte[] TagDirectiveName = "TAG"u8.ToArray();

        public static readonly byte[] Utf8Bom = [0xEF, 0xBB, 0xBF];
        public static readonly byte[] StreamStart = "---"u8.ToArray();
        public static readonly byte[] DocStart = "..."u8.ToArray();
        public static readonly byte[] CrLf = "\r\n"u8.ToArray();

        public static readonly byte[] Null0 = "null"u8.ToArray();
        public static readonly byte[] Null1 = "Null"u8.ToArray();
        public static readonly byte[] Null2 = "NULL"u8.ToArray();
        public const byte NullAlias = (byte)'~';

        public static readonly byte[] True0 = "true"u8.ToArray();
        public static readonly byte[] True1 = "True"u8.ToArray();
        public static readonly byte[] True2 = "TRUE"u8.ToArray();

        public static readonly byte[] False0 = "false"u8.ToArray();
        public static readonly byte[] False1 = "False"u8.ToArray();
        public static readonly byte[] False2 = "FALSE"u8.ToArray();

        public static readonly byte[] Inf0 = ".inf"u8.ToArray();
        public static readonly byte[] Inf1 = ".Inf"u8.ToArray();
        public static readonly byte[] Inf2 = ".INF"u8.ToArray();
        public static readonly byte[] Inf3 = "+.inf"u8.ToArray();
        public static readonly byte[] Inf4 = "+.Inf"u8.ToArray();
        public static readonly byte[] Inf5 = "+.INF"u8.ToArray();

        public static readonly byte[] Yes0 = "yes"u8.ToArray();
        public static readonly byte[] Yes1 = "Yes"u8.ToArray();
        public static readonly byte[] Yes2 = "YES"u8.ToArray();

        public static readonly byte[] No0 = "no"u8.ToArray();
        public static readonly byte[] No1 = "No"u8.ToArray();
        public static readonly byte[] No2 = "NO"u8.ToArray();

        public static readonly byte[] On0 = "on"u8.ToArray();
        public static readonly byte[] On1 = "On"u8.ToArray();
        public static readonly byte[] On2 = "ON"u8.ToArray();

        public static readonly byte[] Off0 = "off"u8.ToArray();
        public static readonly byte[] Off1 = "Off"u8.ToArray();
        public static readonly byte[] Off2 = "OFF"u8.ToArray();

        public static readonly byte[] NegInf0 = "-.inf"u8.ToArray();
        public static readonly byte[] NegInf1 = "-.Inf"u8.ToArray();
        public static readonly byte[] NegInf2 = "-.INF"u8.ToArray();

        public static readonly byte[] Nan0 = ".nan"u8.ToArray();
        public static readonly byte[] Nan1 = ".NaN"u8.ToArray();
        public static readonly byte[] Nan2 = ".NAN"u8.ToArray();

        public static readonly byte[] HexPrefix = "0x"u8.ToArray();
        public static readonly byte[] HexPrefixNegative = "-0x"u8.ToArray();

        public static readonly byte[] OctalPrefix = "0o"u8.ToArray();
        public static readonly byte[] UnityStrippedSymbol = "stripped"u8.ToArray();

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
        public static bool IsNumber(byte c) => (byte)((c | 0x20) - (byte)'0') < 10;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(byte code) => code is Space or Tab or Lf or Cr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLineBreak(byte code) => code is Lf or Cr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlank(byte code) => code is Space or Tab;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHexAlphabet(byte c) => (byte)((c | 0x20) - (byte)'a') < 6;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHex(byte code) => IsNumber(code) || IsHexAlphabet(code);

        // Spec: https://yaml.org/spec/1.2.2/#rule-c-flow-indicator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyFlowSymbol(byte code) => code is
            (byte)',' or (byte)'[' or (byte)']' or (byte)'{' or (byte)'}';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte AsHex(byte code)
        {
            var x = code - (byte)'0';
            if ((uint)x <= 9)
            {
                return (byte)x;
            }
            x = (code | 0x20) - (byte)'a';
            if ((uint)x <= 5)
            {
                return (byte)(x + 10);
            }
            throw new InvalidOperationException();
        }
    }
}
