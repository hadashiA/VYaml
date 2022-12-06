using System.Text;

namespace VYaml.SourceGenerator;

public class CodeWriter
{
    readonly struct IndentScope : IDisposable
    {
        readonly CodeWriter source;

        public IndentScope(CodeWriter source, string? startLine = null)
        {
            this.source = source;
            source.AppendLine(startLine);
            source.IncreaseIndent();
        }

        public void Dispose()
        {
            source.DecreaseIndent();
        }
    }

    readonly struct BlockScope : IDisposable
    {
        readonly CodeWriter source;

        public BlockScope(CodeWriter source, string? startLine = null)
        {
            this.source = source;
            source.AppendLine(startLine);
            source.BeginBlock();
        }

        public void Dispose()
        {
            source.EndBlock();
        }
    }

    readonly StringBuilder buffer = new();
    int indentLevel;

    public void Append(string value)
    {
        buffer.AppendLine($"{new string(' ', indentLevel * 4)} {value}");
    }

    public void AppendLine(string? value = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            buffer.AppendLine();
        }
        else
        {
            buffer.AppendLine($"{new string(' ', indentLevel * 4)} {value}");
        }
    }

    public string CreateEmbededByteArrayString(byte[] bytes)
    {
        var result = new StringBuilder();
        result.Append("{ ");
        var first = true;
        foreach (var x in bytes)
        {
            if (!first)
            {
                result.Append(", ");
            }
            result.Append(x);
            first = false;
        }
        result.Append(" }");
        return result.ToString();
    }

    public override string ToString() => buffer.ToString();

    public IDisposable BeginIndentScope(string? startLine = null) => new IndentScope(this, startLine);
    public IDisposable BeginBlockScope(string? startLine = null) => new BlockScope(this, startLine);

    public void IncreaseIndent()
    {
        indentLevel++;
    }

    public void DecreaseIndent()
    {
        if (indentLevel > 0)
            indentLevel--;
    }

    public void BeginBlock()
    {
        AppendLine("{");
        IncreaseIndent();
    }

    public void EndBlock()
    {
        DecreaseIndent();
        AppendLine("}");
    }

    public void Clear()
    {
        buffer.Clear();
    }
}
