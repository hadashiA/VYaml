using System;
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

        internal static StringBuilder ToLiteralScalar(
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

        internal static void ToDoubleQuotedScalar(
            ReadOnlySpan<char> originalValue,
            Span<char> scalarValue)
        {

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

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // static bool IsPrintable(char ch) => ch is
        //     '\x9' or
        //     '\xA' or
        //     '\xD' or
        //     >= '\x20' and <= '\x7E' or
        //     '\x85' or
        //     >= '\xA0' and <= '\xD7FF' or
        //     >= '\xE000' and <= '\xFFFD';
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // static bool NeedsEspae(char ch)
        // {
        //     return IsPrintable(ch) || ch is '\n'
        // }

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
