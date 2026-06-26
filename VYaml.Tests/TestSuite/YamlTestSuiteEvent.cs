using System;
using System.Collections.Generic;
using System.Text;

namespace VYaml.Tests.TestSuite
{
    /// <summary>
    /// One line of a yaml-test-suite <c>test.event</c> file.
    /// </summary>
    /// <see href="https://github.com/yaml/yaml-test-suite" />
    public sealed class YamlTestSuiteEvent
    {
        public string Type { get; }
        public string? Anchor { get; }
        public string? Tag { get; }
        public string? Value { get; }

        YamlTestSuiteEvent(string type, string? anchor, string? tag, string? value)
        {
            Type = type;
            Anchor = anchor;
            Tag = tag;
            Value = value;
        }

        public static IReadOnlyList<YamlTestSuiteEvent> ParseAll(string eventText)
        {
            var result = new List<YamlTestSuiteEvent>();
            foreach (var raw in eventText.Split('\n'))
            {
                var line = raw.TrimEnd('\r');
                if (line.Length <= 0)
                {
                    continue;
                }
                result.Add(Parse(line));
            }
            return result;
        }

        // Event line grammar (subset we care about):
        //   +STR
        //   -STR
        //   +DOC [---]
        //   -DOC [...]
        //   +MAP [&anchor] [<tag>]
        //   -MAP
        //   +SEQ [&anchor] [<tag>]
        //   -SEQ
        //   =VAL [&anchor] [<tag>] <style><value>
        //   =ALI *anchor
        static YamlTestSuiteEvent Parse(string line)
        {
            var type = line.Length >= 4 ? line.Substring(0, 4) : line;
            var rest = line.Length > 4 ? line.Substring(5) : string.Empty; // skip the space after the 4-char tag

            switch (type)
            {
                case "=ALI":
                    // rest = "*anchor"
                    return new YamlTestSuiteEvent(type, rest.TrimStart('*'), null, null);

                case "=VAL":
                {
                    string? anchor = null;
                    string? tag = null;
                    var i = 0;
                    while (i < rest.Length)
                    {
                        var c = rest[i];
                        if (c == '&')
                        {
                            var end = rest.IndexOf(' ', i);
                            anchor = rest.Substring(i + 1, end - i - 1);
                            i = end + 1;
                        }
                        else if (c == '<')
                        {
                            var end = rest.IndexOf('>', i);
                            tag = rest.Substring(i + 1, end - i - 1);
                            i = end + 2; // skip '>' and following space
                        }
                        else
                        {
                            break;
                        }
                    }
                    // rest[i] is the style char (:, ', ", |, >), value is the remainder.
                    var value = i < rest.Length ? Unescape(rest.Substring(i + 1)) : string.Empty;
                    return new YamlTestSuiteEvent(type, anchor, tag, value);
                }

                case "+MAP":
                case "+SEQ":
                {
                    string? anchor = null;
                    string? tag = null;
                    foreach (var token in rest.Split(' '))
                    {
                        if (token.Length <= 0) continue;
                        if (token[0] == '&') anchor = token.Substring(1);
                        else if (token[0] == '<') tag = token.Trim('<', '>');
                    }
                    return new YamlTestSuiteEvent(type, anchor, tag, null);
                }

                default:
                    // +STR, -STR, +DOC, -DOC, -MAP, -SEQ (trailing markers ignored)
                    return new YamlTestSuiteEvent(type, null, null, null);
            }
        }

        // Scalar contents in test.event are escaped with a small fixed set of sequences.
        static string Unescape(string s)
        {
            if (s.IndexOf('\\') < 0)
            {
                return s;
            }
            var sb = new StringBuilder(s.Length);
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c == '\\' && i + 1 < s.Length)
                {
                    var n = s[++i];
                    sb.Append(n switch
                    {
                        'n' => '\n',
                        't' => '\t',
                        'r' => '\r',
                        'b' => '\b',
                        '0' => '\0',
                        '\\' => '\\',
                        _ => n,
                    });
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Type);
            if (Anchor != null) sb.Append(" &").Append(Anchor);
            if (Tag != null) sb.Append(" <").Append(Tag).Append('>');
            if (Value != null) sb.Append(" \"").Append(Value.Replace("\n", "\\n")).Append('"');
            return sb.ToString();
        }
    }
}
