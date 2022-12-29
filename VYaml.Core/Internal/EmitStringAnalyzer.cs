using System;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using VYaml.Emitter;

namespace VYaml.Internal
{
    readonly struct EmitStringInfo
    {
        public readonly int Lines;
        public readonly bool NeedsQuotes;
        public readonly bool IsReservedWord;
        public readonly char ChompHint;

        public EmitStringInfo(
            int lines,
            bool needsQuotes,
            bool isReservedWord,
            char chompHint)
        {
            Lines = lines;
            NeedsQuotes = needsQuotes;
            IsReservedWord = isReservedWord;
            ChompHint = chompHint;
        }

        public ScalarStyle SuggestScalarStyle()
        {
            if (Lines <= 1)
            {
                return NeedsQuotes ? ScalarStyle.DoubleQuoted : ScalarStyle.Plain;
            }
            return ScalarStyle.Literal;
        }
    }

    static class EmitStringAnalyzer
    {
        [ThreadStatic]
        static StringBuilder? stringBuilderThreadStatic;

        static char[] WhiteSpaces =
        {
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
        };

        public static EmitStringInfo Analyze(string value)
        {
            var chars = value.AsSpan();
            if (chars.Length <= 0)
            {
                return new EmitStringInfo(0, true, false, '\0');
            }

            var isReservedWord = IsReservedWord(value);

            var first = chars[0];
            var last = chars[^1];

            var needsQuotes = isReservedWord ||
                              first == YamlCodes.Space ||
                              last == YamlCodes.Space ||
                              first is '&' or '*' or '?' or '|' or '-' or '<' or '>' or '=' or '!' or '%' or '@' or '.';

            var chompHint = '\0';
            if (last == '\n')
            {
                if (chars[^2] == '\n' ||
                    (chars[^2] == '\r' && chars[^3] == '\n'))
                {
                    chompHint = '+';
                }
            }
            else
            {
                chompHint = '-';
            }

            var lines = 1;
            foreach (var ch in chars)
            {
                switch (ch)
                {
                    case ':':
                    case '{':
                    case '[':
                    case ']':
                    case ',':
                    case '#':
                    case '`':
                    case '"':
                    case '\'':
                        needsQuotes = true;
                        break;
                    case '\n':
                        lines++;
                        break;
                }
            }

            if (last == '\n')
            {
                lines--;
            }
            return new EmitStringInfo(lines, needsQuotes, isReservedWord, chompHint);
        }

        internal static StringBuilder BuildLiteralScalar(
            ReadOnlySpan<char> originalValue,
            char chompHint,
            int indentCharCount)
        {
            var stringBuilder = (stringBuilderThreadStatic ??= new StringBuilder(1024)).Clear();
            stringBuilder.Append('|');
            if (chompHint > 0)
            {
                stringBuilder.Append(chompHint);
            }
            stringBuilder.Append('\n');
            AppendWhiteSpace(stringBuilder, indentCharCount);

            for (var i = 0; i < originalValue.Length; i++)
            {
                var ch = originalValue[i];
                stringBuilder.Append(ch);
                if (ch == '\n' && i < originalValue.Length - 1)
                {
                    AppendWhiteSpace(stringBuilder, indentCharCount);
                }
            }
            return stringBuilder;
        }

        internal static StringBuilder BuildDoubleQuotedScalar(ReadOnlySpan<char> originalValue)
        {
            var stringBuilder = GetStringBuilder();
            stringBuilder.Append('"');

            for (var i = 0; i < originalValue.Length; i++)
            {
                var ch = originalValue[i];
                switch (ch)
                {
                    case '\0':
                        stringBuilder.Append("\\0");
                        break;
                    case '\x7':
                        stringBuilder.Append("\\a");
                        break;
                    case '\x8':
                        stringBuilder.Append("\\b");
                        break;
                    case '\x9':
                        stringBuilder.Append("\\t");
                        break;
                    case '\xA':
                        stringBuilder.Append("\\n");
                        break;
                    case '\xB':
                        stringBuilder.Append("\\v");
                        break;
                    case '\xC':
                        stringBuilder.Append("\\f");
                        break;
                    case '\xD':
                        stringBuilder.Append("\\r");
                        break;
                    case '\x1B':
                        stringBuilder.Append("\\e");
                        break;
                    case '\x22':
                        stringBuilder.Append("\\\"");
                        break;
                    case '\x5C':
                        stringBuilder.Append("\\\\");
                        break;
                    case '\x85':
                        stringBuilder.Append("\\N");
                        break;
                    case '\xA0':
                        stringBuilder.Append("\\_");
                        break;
                    case '\x2028':
                        stringBuilder.Append("\\L");
                        break;
                    case '\x2029':
                        stringBuilder.Append("\\P");
                        break;
                    default:
                        var code = (ushort)ch;
                        if (code <= 0xFF)
                        {
                            stringBuilder.Append('x');
                            stringBuilder.AppendFormat("{0:X02}", code);
                        }
                        else if (IsHighSurrogate(ch))
                        {
                            if (i < originalValue.Length - 1 && IsLowSurrogate(originalValue[i + 1]))
                            {
                                stringBuilder.Append('U');
                                stringBuilder.AppendFormat("{0:X08}", char.ConvertToUtf32(ch, originalValue[++i]));
                            }
                            else
                            {
                                throw new SyntaxErrorException("While writing a quoted scalar, found an orphaned high surrogate.");
                            }
                        }
                        else
                        {
                            stringBuilder.Append('u');
                            stringBuilder.AppendFormat("{0:X04}", code);
                        }
                        break;
                }
            }
            stringBuilder.Append('"');
            return stringBuilder;
        }

        static bool IsReservedWord(string value)
        {
            var b = new StringBuilder();
            b.Append('\n');
            switch (value.Length)
            {
                case 1:
                    if (value == "~")
                    {
                        return true;
                    }
                    break;
                case 4:
                    if (value is "null" or "Null" or "NULL" or "true" or "True" or "TRUE")
                    {
                        return true;
                    }
                    break;
                case 5:
                    if (value is "false" or "False" or "FALSE")
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsHighSurrogate(char c)
        {
            return 0xD800 <= c && c <= 0xDBFF;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsLowSurrogate(char c)
        {
            return 0xDC00 <= c && c <= 0xDFFF;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static StringBuilder GetStringBuilder() =>
            (stringBuilderThreadStatic ??= new StringBuilder(1024)).Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AppendWhiteSpace(StringBuilder stringBuilder, int length)
        {
            if (length > WhiteSpaces.Length)
            {
                WhiteSpaces = Enumerable.Repeat(' ', length * 2).ToArray();
            }
            stringBuilder.Append(WhiteSpaces.AsSpan(0, length));
        }
    }
}
