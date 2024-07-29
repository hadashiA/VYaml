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

        public EmitStringInfo(int lines, bool needsQuotes, bool isReservedWord)
        {
            Lines = lines;
            NeedsQuotes = needsQuotes;
            IsReservedWord = isReservedWord;
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

        static char[] whiteSpaces =
        {
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
        };

        public static EmitStringInfo Analyze(ReadOnlySpan<char> value)
        {
            if (value.Length <= 0)
            {
                return new EmitStringInfo(0, true, false);
            }

            var isReservedWord = IsReservedWord(value);

            var first = value[0];
            var last = value[^1];

            var needsQuotes = isReservedWord ||
                              first == YamlCodes.Space ||
                              last == YamlCodes.Space ||
                              first is '&' or '*' or '?' or '|' or '-' or '<' or '>' or '=' or '!' or '%' or '@' or '.';

            int numbers = 0;

            var lines = 1;
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        numbers++;
                        break;
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
            return new EmitStringInfo(lines, needsQuotes || numbers == chars.Length, isReservedWord);
        }

        internal static StringBuilder BuildLiteralScalar(ReadOnlySpan<char> originalValue, int indentCharCount)
        {
            var chompHint = '\0';
            if (originalValue.Length > 0 && originalValue[^1] == '\n')
            {
                if (originalValue[^2] == '\n' ||
                    (originalValue[^2] == '\r' && originalValue[^3] == '\n'))
                {
                    chompHint = '+';
                }
            }
            else
            {
                chompHint = '-';
            }

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

            if (chompHint == '-')
            {
                stringBuilder.Append('\n');
            }
            return stringBuilder;
        }

        internal static StringBuilder BuildQuotedScalar(ReadOnlySpan<char> originalValue, bool doubleQuote = true)
        {
            var stringBuilder = GetStringBuilder();

            var quoteChar = doubleQuote ? '"' : '\'';
            stringBuilder.Append(quoteChar);

            foreach (var ch in originalValue)
            {
                switch (ch)
                {
                    case '"' when doubleQuote:
                        stringBuilder.Append("\\\"");
                        break;
                    case '\'' when !doubleQuote:
                        stringBuilder.Append("\\'");
                        break;
                    case '\0':
                        stringBuilder.Append("\\0");
                        break;
                    case '\x1':
                        stringBuilder.Append("\\1");
                        break;
                    case '\x2':
                        stringBuilder.Append("\\2");
                        break;
                    case '\x3':
                        stringBuilder.Append("\\3");
                        break;
                    case '\x4':
                        stringBuilder.Append("\\4");
                        break;
                    case '\x5':
                        stringBuilder.Append("\\5");
                        break;
                    case '\x6':
                        stringBuilder.Append("\\6");
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
                    case '\xE':
                        stringBuilder.Append("\\r");
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
                    case '\xF':
                        stringBuilder.Append("\\u000f");
                        break;
                    case '\x10':
                        stringBuilder.Append("\\u0010");
                        break;
                    case '\x11':
                        stringBuilder.Append("\\u0011");
                        break;
                    case '\x12':
                        stringBuilder.Append("\\u0012");
                        break;
                    case '\x13':
                        stringBuilder.Append("\\u0013");
                        break;
                    case '\x14':
                        stringBuilder.Append("\\u0014");
                        break;
                    case '\x15':
                        stringBuilder.Append("\\u0015");
                        break;
                    case '\x16':
                        stringBuilder.Append("\\u0016");
                        break;
                    case '\x17':
                        stringBuilder.Append("\\u0017");
                        break;
                    case '\x18':
                        stringBuilder.Append("\\u0018");
                        break;
                    case '\x19':
                        stringBuilder.Append("\\u0019");
                        break;
                    case '\x1A':
                        stringBuilder.Append("\\u001a");
                        break;
                    case '\x1B':
                        stringBuilder.Append("\\u001b");
                        break;
                    case '\x1C':
                        stringBuilder.Append("\\u001c");
                        break;
                    case '\x1D':
                        stringBuilder.Append("\\u001d");
                        break;
                    case '\x1E':
                        stringBuilder.Append("\\u001e");
                        break;
                    case '\x1F':
                        stringBuilder.Append("\\u001f");
                        break;
                    case '\x7F':
                        stringBuilder.Append("\\u007F");
                        break;
                    default:
                        stringBuilder.Append(ch);
                        break;
                }
            }
            stringBuilder.Append(quoteChar);
            return stringBuilder;
        }

        static bool IsReservedWord(ReadOnlySpan<char> value)
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
                    if (value.SequenceEqual("null") ||
                        value.SequenceEqual("null") ||
                        value.SequenceEqual("Null") ||
                        value.SequenceEqual("NULL") ||
                        value.SequenceEqual("true") ||
                        value.SequenceEqual("True") ||
                        value.SequenceEqual("TRUE"))
                    {
                        return true;
                    }
                    break;
                case 5:
                    if (value.SequenceEqual("false") ||
                        value.SequenceEqual("False") ||
                        value.SequenceEqual("FALSE"))
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static StringBuilder GetStringBuilder() =>
            (stringBuilderThreadStatic ??= new StringBuilder(1024)).Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AppendWhiteSpace(StringBuilder stringBuilder, int length)
        {
            if (length > whiteSpaces.Length)
            {
                whiteSpaces = Enumerable.Repeat(' ', length * 2).ToArray();
            }
            stringBuilder.Append(whiteSpaces.AsSpan(0, length));
        }
    }
}

