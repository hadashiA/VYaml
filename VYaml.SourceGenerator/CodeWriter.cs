using System;
using System.Text;

namespace VYaml.SourceGenerator;

class CodeWriter
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

    public void Append(string value, bool indent = true)
    {
        if (indent)
        {
            buffer.Append($"{new string(' ', indentLevel * 4)} {value}");
        }
        else
        {
            buffer.Append(value);
        }
    }

    public void AppendLine(string? value = null, bool indent = true)
    {
        if (string.IsNullOrEmpty(value))
        {
            buffer.AppendLine();
        }
        else if (indent)
        {
            buffer.AppendLine($"{new string(' ', indentLevel * 4)} {value}");
        }
        else
        {
            buffer.AppendLine(value);
        }
    }

    public void AppendByteArrayString(byte[] bytes)
    {
        buffer.Append("{ ");
        var first = true;
        foreach (var x in bytes)
        {
            if (!first)
            {
                buffer.Append(", ");
            }
            buffer.Append(x);
            first = false;
        }
        buffer.Append(" }");
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
