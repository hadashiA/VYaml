using System;
using VYaml.Emitter;

namespace VYaml.Internal
{
    readonly struct EmitStringInfo
    {
        public readonly int Lines;
        public readonly bool NeedsQuotes;
        public readonly bool IsReservedWord;
        public readonly byte ChompHint;

        public EmitStringInfo(
            int lines,
            bool needsQuotes,
            bool isReservedWord,
            byte chompHint)
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
        public static EmitStringInfo Analyze(string value)
        {
            var chars = value.AsSpan();
            if (chars.Length <= 0)
            {
                return new EmitStringInfo(0, true, false, 0);
            }

            var isReservedWord = IsReservedWord(value);

            var first = chars[0];
            var last = chars[^1];

            var needsQuotes = isReservedWord ||
                              first == YamlCodes.Space ||
                              last == YamlCodes.Space ||
                              first is '&' or '*' or '?' or '|' or '-' or '<' or '>' or '=' or '!' or '%' or '@' or '.';

            byte chompHint = 0;
            if (last == '\n')
            {
                if (chars[^2] == '\n' ||
                    (chars[^2] == '\r' && chars[^3] == '\n'))
                {
                    chompHint = (byte)'+';
                }
            }
            else
            {
                chompHint = (byte)'-';
            }

            var lines = 0;
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
            return new EmitStringInfo(lines, needsQuotes, isReservedWord, chompHint);
        }

        static bool IsReservedWord(string value)
        {
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
    }
}
