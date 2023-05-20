#nullable enable
using System.Text;

namespace VYaml.Internal
{
    static class KeyNameHelper
    {
        public static string Original(string s)
        {
            return s;
        }

        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || char.IsLower(s, 0))
            {
                return s;
            }

            var array = s.ToCharArray();
            array[0] = char.ToLowerInvariant(array[0]);
            return new string(array);
        }

        public static string ToSnakeCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var sb = new StringBuilder();
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];

                if (char.IsUpper(c))
                {
                    // first
                    if (i == 0)
                    {
                        sb.Append(char.ToLowerInvariant(c));
                    }
                    else if (char.IsUpper(s[i - 1])) // WriteIO => write_io
                    {
                        sb.Append(char.ToLowerInvariant(c));
                    }
                    else
                    {
                        sb.Append("_");
                        sb.Append(char.ToLowerInvariant(c));
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
