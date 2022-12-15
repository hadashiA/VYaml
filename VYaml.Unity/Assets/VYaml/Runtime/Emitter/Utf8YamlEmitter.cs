using System;
using System.Buffers;
using System.Buffers.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using VYaml.Internal;

namespace VYaml.Emitter
{
    public class YamlEmitterException : Exception
    {
        public YamlEmitterException(string message) : base(message)
        {
        }
    }

    enum EmitState
    {
        None,
        BlockSequenceEntry,
        BlockMappingKey,
        BlockMappingValue,
    }

    public ref struct Utf8YamlEmitter
    {
        static byte[] WhiteSpaces;
        static readonly byte[] BlockSequenceEntryHeaderEmpty = { (byte)'-', (byte)'\n' };
        static readonly byte[] BlockSequenceEntryHeader = { (byte)'-', (byte)' ' };
        static readonly byte[] MappingKeyFooter = { (byte)':', (byte)' ' };

        EmitState CurrentState => stateStack.Peek();

        readonly IBufferWriter<byte> writer;
        readonly YamlEmitOptions options;

        ExpandBuffer<byte> stringBuffer;
        ExpandBuffer<EmitState> stateStack;

        int currentIndentLevel;

        static Utf8YamlEmitter()
        {
            WhiteSpaces = Enumerable.Repeat(YamlCodes.Space, 64).ToArray();
        }

        public Utf8YamlEmitter(IBufferWriter<byte> writer, YamlEmitOptions? options = null)
        {
            this.writer = writer;
            this.options = options ?? YamlEmitOptions.Default;

            currentIndentLevel = 0;
            stringBuffer = new ExpandBuffer<byte>(1024);
            stateStack = new ExpandBuffer<EmitState>(16);
            stateStack.Add(EmitState.None);
        }

        internal readonly IBufferWriter<byte> GetWriter() => writer;

        public void Dispose()
        {
            stringBuffer.Dispose();
            stateStack.Dispose();
        }

        public void BeginBlockSequence()
        {
            switch (CurrentState)
            {
                case EmitState.BlockMappingKey:
                    throw new YamlEmitterException("To start block-sequence in the mapping key is not supported.");
                case EmitState.BlockSequenceEntry or EmitState.BlockMappingValue:
                {
                    var length = BlockSequenceEntryHeaderEmpty.Length + currentIndentLevel * options.IndentWidth;
                    var offset = 0;
                    var output = writer.GetSpan(length);
                    WriteIndent(output, ref offset);
                    BlockSequenceEntryHeaderEmpty.CopyTo(output[offset..]);
                    writer.Advance(length);
                    IncreaseIndent();
                    break;
                }
            }
            PushState(EmitState.BlockSequenceEntry);
        }

        public void EndBlockSequence()
        {
            PopState();
            DecreaseIndent();
        }

        public void BeginBlockMapping()
        {
            switch (CurrentState)
            {
                case EmitState.BlockMappingKey:
                    throw new YamlEmitterException("To start block-mapping in the mapping key is not supported.");
                case EmitState.BlockSequenceEntry or EmitState.BlockMappingValue:
                    IncreaseIndent();
                    break;
            }
            PushState(EmitState.BlockMappingKey);
        }

        public void EndBlockMapping()
        {
            if (CurrentState != EmitState.BlockMappingKey)
            {
                throw new YamlEmitterException($"Invalid block mapping end: {CurrentState}");
            }
            DecreaseIndent();
            PopState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteScalar(ReadOnlySpan<byte> value)
        {
            var offset = 0;
            var output = writer.GetSpan(GetScalarBufferLength(value.Length));

            BeginScalar(output, ref offset);
            value.CopyTo(output[offset..]);
            offset += value.Length;
            EndScalar(output, ref offset);

            writer.Advance(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull()
        {
            WriteScalar(YamlCodes.Null0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBool(bool value)
        {
            WriteScalar(value ? YamlCodes.True0 : YamlCodes.False0);
        }

        public void WriteInt32(int value)
        {
            var offset = 0;
            var output = writer.GetSpan(GetScalarBufferLength(11)); // -2147483648

            BeginScalar(output, ref offset);
            if (!Utf8Formatter.TryFormat(value, output[offset..], out var bytesWritten))
            {
                throw new YamlEmitterException($"Failed to emit : {value}");
            }
            offset += bytesWritten;
            EndScalar(output, ref offset);

            writer.Advance(offset);
        }

        public void WriteUInt32(uint value)
        {
            var offset = 0;
            var output = writer.GetSpan(GetScalarBufferLength(10)); // 4294967295

            BeginScalar(output, ref offset);
            if (!Utf8Formatter.TryFormat(value, output[offset..], out var bytesWritten))
            {
                throw new YamlEmitterException($"Failed to emit : {value}");
            }
            offset += bytesWritten;
            EndScalar(output, ref offset);

            writer.Advance(offset);
        }

        public void WriteInt64(long value)
        {
            throw new NotImplementedException();
            var span = writer.GetSpan(GetScalarBufferLength(20)); // -9223372036854775808
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                throw new YamlEmitterException($"Failed to emit : {value}");
            }
            writer.Advance(bytesWritten);
        }

        public void WriteUInt64(ulong value)
        {
            throw new NotImplementedException();
            var span = writer.GetSpan(20); // 18446744073709551615
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                throw new YamlEmitterException($"Failed to emit : {value}");
            }
            writer.Advance(bytesWritten);
        }

        public void WriteFloat(float value)
        {
            throw new NotImplementedException();
            var span = writer.GetSpan(16);
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                span = writer.GetSpan(128);
                if (!Utf8Formatter.TryFormat(value, span, out bytesWritten))
                {
                    throw new YamlEmitterException($"Failed to emit : {value}");
                }
            }
            writer.Advance(bytesWritten);
        }

        public void WriteDouble(double value)
        {
            throw new NotImplementedException();
            var span = writer.GetSpan(16);
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                span = writer.GetSpan(128);
                if (!Utf8Formatter.TryFormat(value, span, out bytesWritten))
                {
                    throw new YamlEmitterException($"Failed to emit : {value}");
                }
            }
            writer.Advance(bytesWritten);
        }

        public void WriteString(string value, ScalarStyle style = ScalarStyle.Any)
        {
            throw new NotImplementedException();

            if (style == ScalarStyle.Any)
            {
                var analyzeInfo = EmitStringAnalyzer.Analyze(value);
            }

            switch (style)
            {
                case ScalarStyle.Plain:
                    //WritePlainScalar(value);
                    break;
                case ScalarStyle.SingleQuoted:
                    break;
                case ScalarStyle.DoubleQuoted:
                    break;
                case ScalarStyle.Literal:
                    break;
                case ScalarStyle.Folded:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

        public void WriteBlockEntryHeader()
        {
            var whiteSpaceLength = currentIndentLevel * options.IndentWidth;
            if (whiteSpaceLength > WhiteSpaces.Length)
            {
                WhiteSpaces = Enumerable.Repeat(YamlCodes.Space, whiteSpaceLength * 2).ToArray();
            }

            var length = whiteSpaceLength + 2;
            var target = writer.GetSpan(length);

            WhiteSpaces.AsSpan(0, whiteSpaceLength).CopyTo(target);
            target[whiteSpaceLength + 1] = YamlCodes.BlockEntryIndent;
            target[whiteSpaceLength + 2] = YamlCodes.Space;
            writer.Advance(length);
        }

        void IncreaseIndent()
        {
            currentIndentLevel++;
        }

        void DecreaseIndent()
        {
            if (currentIndentLevel > 0)
                currentIndentLevel--;
        }

        void WritePlainScalar(string value, in EmitStringInfo analyzeInfo)
        {
            if (analyzeInfo.NeedsQuotes)
            {
                throw new NotImplementedException();
            }

            var bufferLength = StringEncoding.Utf8.GetMaxByteCount(value.Length);
            var span = writer.GetSpan(bufferLength);
            StringEncoding.Utf8.GetBytes(value, span);
        }

        void WriteLiteralScalar(string value, in EmitStringInfo analyzeInfo)
        {
            var defaultMaxByteCount = StringEncoding.Utf8.GetMaxByteCount(value.Length);
            stringBuffer.Clear();
            stringBuffer.SetCapacity(defaultMaxByteCount);

            var buf = stringBuffer.AsSpan();
            var inputBytes = StringEncoding.Utf8.GetBytes(value, buf);

            var totalSize = inputBytes +
                            (analyzeInfo.Lines + 1) * currentIndentLevel * options.IndentWidth +
                            4; // " |\n"

            var offset = 0;
            var output = writer.GetSpan(totalSize);

            output[offset++] = YamlCodes.Space;
            output[offset++] = YamlCodes.LiteralScalerHeader;
            if (analyzeInfo.ChompHint > 0)
            {
                output[offset++] = analyzeInfo.ChompHint;
            }
            output[offset++] = YamlCodes.Lf;

            IncreaseIndent();
            try
            {
                WriteIndent(output, ref offset);

                foreach (var x in buf)
                {
                    if (x == YamlCodes.Lf)
                    {
                        WriteIndent(output, ref offset);
                    }
                    output[offset++] = x;
                }
                writer.Advance(offset);
            }
            finally
            {
                DecreaseIndent();
            }
        }

        void WriteFoldedScalar(string value, in EmitStringInfo analyzeInfo)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteIndent(Span<byte> output, ref int offset)
        {
            if (currentIndentLevel <= 0)
            {
                return;
            }

            var length = currentIndentLevel * options.IndentWidth;
            if (length > WhiteSpaces.Length)
            {
                WhiteSpaces = Enumerable.Repeat(YamlCodes.Space, length * 2).ToArray();
            }
            WhiteSpaces.AsSpan(0, length).CopyTo(output[offset..]);
            offset += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetScalarBufferLength(int length)
        {
            length += currentIndentLevel * options.IndentWidth; // indent
            for (var i = 0; i < stateStack.Length; i++)
            {
                switch (stateStack[i])
                {
                    case EmitState.BlockSequenceEntry:
                        length += 3; // "- " + "\n"
                        break;
                    case EmitState.BlockMappingKey:
                        length += 2; // ": "
                        break;
                    case EmitState.BlockMappingValue:
                        break;
                    case EmitState.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void BeginScalar(Span<byte> output, ref int offset)
        {
            switch (CurrentState)
            {
                case EmitState.BlockSequenceEntry:
                    WriteIndent(output, ref offset);
                    BlockSequenceEntryHeader.CopyTo(output[offset..]);
                    offset += BlockSequenceEntryHeader.Length;
                    break;
                case EmitState.BlockMappingKey:
                    WriteIndent(output, ref offset);
                    break;
                case EmitState.BlockMappingValue:
                    break;
                case EmitState.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EndScalar(Span<byte> output, ref int offset)
        {
            switch (CurrentState)
            {
                case EmitState.BlockSequenceEntry:
                    output[offset++] = YamlCodes.Lf;
                    break;
                case EmitState.BlockMappingKey:
                    MappingKeyFooter.CopyTo(output[offset..]);
                    offset += MappingKeyFooter.Length;
                    ReplaceCurrentState(EmitState.BlockMappingValue);
                    break;
                case EmitState.BlockMappingValue:
                    output[offset++] = YamlCodes.Lf;
                    ReplaceCurrentState(EmitState.BlockMappingKey);
                    break;
                case EmitState.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReplaceCurrentState(EmitState newState)
        {
            stateStack[^1] = newState;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushState(EmitState state)
        {
            stateStack.Add(state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PopState()
        {
            stateStack.Pop();
        }
    }
}
