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

        static readonly bool[] EmptyTable = new bool[256];
        static readonly bool[] BlankTable = new bool[256];
        static readonly bool[] FlowSymbolTable = new bool[256];

        static YamlCodes()
        {
            EmptyTable[Space] = true;
            EmptyTable[Tab] = true;
            EmptyTable[Lf] = true;
            EmptyTable[Cr] = true;

            BlankTable[Space] = true;
            BlankTable[Tab] = true;

            FlowSymbolTable[','] = true;
            FlowSymbolTable['['] = true;
            FlowSymbolTable[']'] = true;
            FlowSymbolTable['{'] = true;
            FlowSymbolTable['}'] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphaNumericDashOrUnderscore(byte code) =>
            IsNumber(code) || IsAlphabet(code) || code is (byte)'_' or (byte)'-';

        // Spec: https://yaml.org/spec/1.2.2/#rule-ns-word-char
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWordChar(byte code) =>
            IsNumber(code) || IsAlphabet(code) || code is (byte)'-';

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
        public static bool IsEmpty(byte code) => EmptyTable[code];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlank(byte code) => BlankTable[code];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLineBreak(byte code) => code is Lf or Cr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphabet(byte c) => (byte)((c | 0x20) - (byte)'a') < 26;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHexAlphabet(byte c) => (byte)((c | 0x20) - (byte)'a') < 6;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHex(byte code) => IsNumber(code) || IsHexAlphabet(code);

        // Spec: https://yaml.org/spec/1.2.2/#rule-c-flow-indicator
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyFlowSymbol(byte code) => FlowSymbolTable[code];

        /// <summary>
        /// Gets the length of a UTF-8 sequence from its first byte.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetUtf8SequenceLength(byte firstByte)
        {
            // UTF-8 sequence length from first byte:
            // 0xxxxxxx = 1 byte (ASCII)
            // 110xxxxx = 2 bytes
            // 1110xxxx = 3 bytes  
            // 11110xxx = 4 bytes
            if ((firstByte & 0x80) == 0) return 1;
            if ((firstByte & 0xE0) == 0xC0) return 2;
            if ((firstByte & 0xF0) == 0xE0) return 3;
            if ((firstByte & 0xF8) == 0xF0) return 4;
            return 1; // Invalid UTF-8, treat as single byte
        }

        /// <summary>
        /// Checks if a byte is a UTF-8 continuation byte.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUtf8ContinuationByte(byte code)
        {
            return (code & 0xC0) == 0x80;
        }

        /// <summary>
        /// Checks if a byte sequence represents a UTF-8 whitespace character.
        /// </summary>
        public static bool IsUtf8Whitespace(byte code, ReadOnlySpan<byte> next)
        {
            // Fast path for ASCII
            if (IsBlank(code)) return true;
            
            // Common UTF-8 whitespaces:
            // U+00A0 (NBSP) = 0xC2 0xA0
            if (code == 0xC2 && next.Length >= 1 && next[0] == 0xA0)
                return true;
            
            // U+2000-U+200B (various spaces) = 0xE2 0x80 0x80-0x8B
            if (code == 0xE2 && next.Length >= 2 && next[0] == 0x80 && 
                next[1] >= 0x80 && next[1] <= 0x8B)
                return true;
            
            // U+3000 (Ideographic space) = 0xE3 0x80 0x80
            if (code == 0xE3 && next.Length >= 2 && 
                next[0] == 0x80 && next[1] == 0x80)
                return true;
            
            // U+FEFF (Zero-width non-breaking space) = 0xEF 0xBB 0xBF
            if (code == 0xEF && next.Length >= 2 && 
                next[0] == 0xBB && next[1] == 0xBF)
                return true;
            
            return false;
        }

        /// <summary>
        /// Checks if a byte sequence represents a UTF-8 flow symbol character.
        /// </summary>
        public static bool IsUtf8FlowSymbol(byte code, ReadOnlySpan<byte> next)
        {
            // Fast path for ASCII
            if (IsAnyFlowSymbol(code)) return true;
            
            // Full-width brackets that might be used as flow symbols:
            // U+FF3B ［ = 0xEF 0xBC 0xBB
            // U+FF3D ］ = 0xEF 0xBC 0xBD
            // U+FF5B ｛ = 0xEF 0xBD 0x9B
            // U+FF5D ｝ = 0xEF 0xBD 0x9D
            // U+FF0C ， = 0xEF 0xBC 0x8C
            
            if (code == 0xEF && next.Length >= 2)
            {
                if (next[0] == 0xBC)
                {
                    return next[1] == 0xBB || next[1] == 0xBD || next[1] == 0x8C;
                }
                if (next[0] == 0xBD)
                {
                    return next[1] == 0x9B || next[1] == 0x9D;
                }
            }
            
            return false;
        }

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
