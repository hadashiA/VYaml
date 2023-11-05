namespace VYaml.SourceGenerator;

enum NamingConvention
{
    LowerCamelCase,
    UpperCamelCase,
    SnakeCase,
    KebabCase,
}

static class KeyNameMutator
{
    public static string Mutate(string s, NamingConvention namingConvention)
    {
        return namingConvention switch
        {
            NamingConvention.LowerCamelCase => ToLowerCamelCase(s),
            NamingConvention.UpperCamelCase => s,
            NamingConvention.SnakeCase => ToSnakeCase(s),
            NamingConvention.KebabCase => ToSnakeCase(s, '-'),
            _ => throw new ArgumentOutOfRangeException(nameof(namingConvention), namingConvention, null)
        };
    }

    public static string ToLowerCamelCase(string s)
    {
        var span = s.AsSpan();
        if (span.Length <= 0 ||
            (span.Length <= 1 && char.IsLower(span[0])))
        {
            return s;
        }

        Span<char> buf = stackalloc char[span.Length];
        span.CopyTo(buf);
        buf[0] = char.ToLowerInvariant(span[0]);
        return buf.ToString();
    }

    public static string ToSnakeCase(string s, char separator = '_')
    {
        var span = s.AsSpan();
        if (span.Length <= 0) return s;

        Span<char> buf = stackalloc char[span.Length * 2];
        var written = 0;
        foreach (var ch in span)
        {
            if (char.IsUpper(ch))
            {
                if (written == 0 || // first
                    char.IsUpper(span[written - 1])) // WriteIO => write_io
                {
                    buf[written++] = char.ToLowerInvariant(ch);
                }
                else
                {
                    buf[written++] = separator;
                    if (buf.Length <= written)
                    {
                        buf = new char[buf.Length * 2];
                    }
                    buf[written++] = char.ToLowerInvariant(ch);
                }
            }
            else
            {
                buf[written++] = ch;
            }
        }
        return buf.Slice(0, written).ToString();
    }
}