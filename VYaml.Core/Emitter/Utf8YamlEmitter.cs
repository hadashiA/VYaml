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
        FlowSequenceEntry,
    }

    public ref struct Utf8YamlEmitter
    {
        static byte[] WhiteSpaces =
        {
            (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ',
            (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ',
            (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ',
            (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ', (byte)' ',
        };
        static readonly byte[] BlockSequenceEntryHeaderEmpty = { (byte)'-', (byte)'\n' };
        static readonly byte[] BlockSequenceEntryHeader = { (byte)'-', (byte)' ' };
        static readonly byte[] FlowSequenceEmpty = { (byte)'[', (byte)']' };
        static readonly byte[] FlowSequenceSeparator = { (byte)',', (byte)' ' };
        static readonly byte[] MappingKeyFooter = { (byte)':', (byte)' ' };
        static readonly byte[] FlowMappingEmpty = { (byte)'{', (byte)'}' };

        EmitState NextState => stateStack.Peek();

        readonly IBufferWriter<byte> writer;
        readonly YamlEmitOptions options;

        ExpandBuffer<char> stringBuffer;
        ExpandBuffer<EmitState> stateStack;
        ExpandBuffer<int> elementCountStack;
        ExpandBuffer<byte> nodePrefixBuffer;

        int currentIndentLevel;
        int currentTokenCount;

        Span<byte> nodePrefix;

        public Utf8YamlEmitter(IBufferWriter<byte> writer, YamlEmitOptions? options = null)
        {
            this.writer = writer;
            this.options = options ?? YamlEmitOptions.Default;

            currentIndentLevel = 0;
            stringBuffer = new ExpandBuffer<char>(1024);
            stateStack = new ExpandBuffer<EmitState>(16);
            elementCountStack = new ExpandBuffer<int>(16);
            stateStack.Add(EmitState.None);
            currentTokenCount = 0;

            nodePrefixBuffer = new ExpandBuffer<byte>(256);
            nodePrefix = Span<byte>.Empty;
        }

        internal readonly IBufferWriter<byte> GetWriter() => writer;

        public void Dispose()
        {
            stringBuffer.Dispose();
            stateStack.Dispose();
            elementCountStack.Dispose();
            nodePrefixBuffer.Dispose();
        }

        public void BeginSequence(SequenceStyle style = SequenceStyle.Block)
        {
            currentTokenCount = 0;

            if (!nodePrefix.IsEmpty)
            {
                nodePrefix = Span<byte>.Empty;
                throw new NotImplementedException();
            }

            switch (style)
            {
                case SequenceStyle.Block:
                {
                    switch (NextState)
                    {
                        case EmitState.BlockSequenceEntry:
                        {
                            WriteRaw(BlockSequenceEntryHeaderEmpty, true, false);
                            IncreaseIndent();
                            break;
                        }
                        case EmitState.BlockMappingKey:
                        {
                            throw new YamlEmitterException("To start block-sequence in the mapping key is not supported.");
                        }
                        case EmitState.BlockMappingValue:
                        {
                            var output = writer.GetSpan(1);
                            output[0] = YamlCodes.Lf;
                            writer.Advance(1);
                            // IncreaseIndent();
                            ReplaceNextState(EmitState.BlockMappingKey); // Close mapping
                            break;
                        }
                    }
                    PushState(EmitState.BlockSequenceEntry);
                    break;
                }
                case SequenceStyle.Flow:
                {
                    switch (NextState)
                    {
                        case EmitState.BlockMappingKey:
                        {
                            throw new YamlEmitterException("To start flow-mapping in the mapping key is not supported.");
                        }
                        case EmitState.BlockMappingValue:
                        {
                            ReplaceNextState(EmitState.BlockMappingKey); // Close mapping
                            var output = writer.GetSpan(1);
                            output[0] = YamlCodes.FlowSequenceStart;
                            writer.Advance(1);
                            break;
                        }
                        case EmitState.BlockSequenceEntry:
                        {
                            var length = BlockSequenceEntryHeader.Length + 1;
                            var output = writer.GetSpan(length);
                            BlockSequenceEntryHeader.CopyTo(output);
                            output[BlockSequenceEntryHeader.Length] = YamlCodes.FlowSequenceStart;
                            writer.Advance(length);
                            break;
                        }
                        case EmitState.FlowSequenceEntry:
                        {
                            var length = FlowSequenceSeparator.Length + 1;
                            var output = writer.GetSpan(length);
                            FlowSequenceSeparator.CopyTo(output);
                            output[FlowSequenceSeparator.Length] = YamlCodes.FlowSequenceStart;
                            writer.Advance(length);
                            break;
                        }
                        default:
                        {
                            var output = writer.GetSpan(1);
                            output[0] = YamlCodes.FlowSequenceStart;
                            writer.Advance(1);
                            break;
                        }
                    }
                    PushState(EmitState.FlowSequenceEntry);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

        public void EndSequence()
        {
            switch (NextState)
            {
                case EmitState.BlockSequenceEntry:
                {
                    if (currentTokenCount == 0)
                    {
                        var output = writer.GetSpan(FlowSequenceEmpty.Length);
                        FlowSequenceEmpty.CopyTo(output);
                        writer.Advance(FlowSequenceEmpty.Length);
                    }
                    PopState();
                    DecreaseIndent();
                    if (NextState != EmitState.None)
                    {
                        currentTokenCount++;
                    }
                    break;
                }

                case EmitState.FlowSequenceEntry:
                {
                    PopState(); // end sequence

                    var needsLineBreak = false;
                    switch (NextState)
                    {
                        case EmitState.BlockSequenceEntry:
                        case EmitState.BlockMappingKey:
                            needsLineBreak = true;
                            currentTokenCount++;
                            break;
                        case EmitState.BlockMappingValue:
                        case EmitState.FlowSequenceEntry:
                            currentTokenCount++;
                            break;
                    }

                    var suffixLength = 1;
                    if (needsLineBreak) suffixLength++;

                    var offset = 0;
                    var output = writer.GetSpan(suffixLength);
                    output[offset++] = YamlCodes.FlowSequenceEnd;
                    if (needsLineBreak)
                    {
                        output[offset++] = YamlCodes.Lf;
                    }
                    writer.Advance(offset);
                    break;
                }

                default:
                    throw new YamlEmitterException($"Current state is not sequence: {NextState}");
            }
        }

        public void BeginMapping(MappingStyle style = MappingStyle.Block)
        {
            currentTokenCount = 0;

            switch (style)
            {
                case MappingStyle.Block:
                {
                    switch (NextState)
                    {
                        case EmitState.BlockMappingKey:
                        {
                            throw new YamlEmitterException("To start block-mapping in the mapping key is not supported.");
                        }
                        case EmitState.FlowSequenceEntry:
                        {
                            throw new YamlEmitterException( "Cannot start block-mapping in the flow-sequence");
                        }
                        case EmitState.BlockMappingValue:
                        {
                            IncreaseIndent();
                            ReplaceNextState(EmitState.BlockMappingKey);

                            if (nodePrefix.IsEmpty)
                            {
                                var output = writer.GetSpan(1);
                                output[0] = YamlCodes.Lf;
                                writer.Advance(1);
                            }
                            else
                            {
                                WriteRaw(nodePrefix, false, true);
                                nodePrefix = Span<byte>.Empty;
                                currentTokenCount++;
                            }
                            break;
                        }
                        case EmitState.BlockSequenceEntry:
                        {
                            if (nodePrefix.IsEmpty)
                            {
                                WriteRaw(BlockSequenceEntryHeader, true, false);
                            }
                            else
                            {
                                WriteRaw(BlockSequenceEntryHeader, nodePrefix, true, true);
                                currentTokenCount++;
                                nodePrefix = Span<byte>.Empty;
                            }
                            IncreaseIndent();
                            break;
                        }
                        default:
                        {
                            if (!nodePrefix.IsEmpty)
                            {
                                WriteRaw(nodePrefix, false, true);
                                currentTokenCount++;
                                nodePrefix = Span<byte>.Empty;
                            }
                            break;
                        }
                    }
                    PushState(EmitState.BlockMappingKey);
                    break;
                }
                case MappingStyle.Flow:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

        public void EndMapping()
        {
            if (NextState != EmitState.BlockMappingKey)
            {
                throw new YamlEmitterException($"Invalid block mapping end: {NextState}");
            }

            if (currentTokenCount == 0)
            {
                var output = writer.GetSpan(FlowMappingEmpty.Length);
                FlowMappingEmpty.CopyTo(output);
                writer.Advance(FlowMappingEmpty.Length);
            }

            DecreaseIndent();
            PopState();

            if (NextState != EmitState.None)
            {
                currentTokenCount++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRaw(ReadOnlySpan<byte> value, bool indent, bool lineBreak)
        {
            var length = value.Length +
                         (indent ? currentIndentLevel * options.IndentWidth : 0) +
                         (lineBreak ? 1 : 0);

            var offset = 0;
            var output = writer.GetSpan(length);
            if (indent)
            {
                WriteIndent(output, ref offset);
            }
            value.CopyTo(output[offset..]);
            if (lineBreak)
            {
                output[length - 1] = YamlCodes.Lf;
            }
            writer.Advance(length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRaw(ReadOnlySpan<byte> value1, ReadOnlySpan<byte> value2, bool indent, bool lineBreak)
        {
            var length = value1.Length + value2.Length +
                         (indent ? currentIndentLevel * options.IndentWidth : 0) +
                         (lineBreak ? 1 : 0);
            var offset = 0;
            var output = writer.GetSpan(length);
            if (indent)
            {
                WriteIndent(output, ref offset);
            }

            value1.CopyTo(output[offset..]);
            offset += value1.Length;

            value2.CopyTo(output[offset..]);
            if (lineBreak)
            {
                output[length - 1] = YamlCodes.Lf;
            }
            writer.Advance(length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tag(ReadOnlySpan<byte> value)
        {
            nodePrefix = nodePrefixBuffer.AsSpan(value.Length);
            value.CopyTo(nodePrefix);
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
            var offset = 0;
            var output = writer.GetSpan(GetScalarBufferLength(20)); // -9223372036854775808

            BeginScalar(output, ref offset);
            if (!Utf8Formatter.TryFormat(value, output[offset..], out var bytesWritten))
            {
                throw new YamlEmitterException($"Failed to emit : {value}");
            }
            offset += bytesWritten;
            EndScalar(output, ref offset);

            writer.Advance(offset);
        }

        public void WriteUInt64(ulong value)
        {
            var offset = 0;
            var output = writer.GetSpan(GetScalarBufferLength(20)); // 18446744073709551615

            BeginScalar(output, ref offset);
            if (!Utf8Formatter.TryFormat(value, output[offset..], out var bytesWritten))
            {
                throw new YamlEmitterException($"Failed to emit : {value}");
            }
            offset += bytesWritten;
            EndScalar(output, ref offset);

            writer.Advance(offset);
        }

        public void WriteFloat(float value)
        {
            var offset = 0;
            var output = writer.GetSpan(GetScalarBufferLength(12));

            BeginScalar(output, ref offset);
            if (!Utf8Formatter.TryFormat(value, output[offset..], out var bytesWritten))
            {
                throw new YamlEmitterException($"Failed to emit : {value}");
            }
            offset += bytesWritten;
            EndScalar(output, ref offset);

            writer.Advance(offset);
        }

        public void WriteDouble(double value)
        {
            var offset = 0;
            var output = writer.GetSpan(GetScalarBufferLength(17));

            BeginScalar(output, ref offset);
            if (!Utf8Formatter.TryFormat(value, output[offset..], out var bytesWritten))
            {
                throw new YamlEmitterException($"Failed to emit : {value}");
            }
            offset += bytesWritten;
            EndScalar(output, ref offset);

            writer.Advance(offset);
        }

        public void WriteString(string value, ScalarStyle style = ScalarStyle.Any)
        {
            if (style == ScalarStyle.Any)
            {
                var analyzeInfo = EmitStringAnalyzer.Analyze(value);
                style = analyzeInfo.SuggestScalarStyle();
            }

            switch (style)
            {
                case ScalarStyle.Plain:
                    WritePlainScalar(value);
                    break;

                case ScalarStyle.SingleQuoted:
                    WriteQuotedScalar(value, doubleQuote: false);
                    break;

                case ScalarStyle.DoubleQuoted:
                    WriteQuotedScalar(value, doubleQuote: true);
                    break;

                case ScalarStyle.Literal:
                    WriteLiteralScalar(value);
                    break;

                case ScalarStyle.Folded:
                    throw new NotSupportedException();

                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

        void WritePlainScalar(string value)
        {
            var stringMaxByteCount = StringEncoding.Utf8.GetMaxByteCount(value.Length);
            var output = writer.GetSpan(GetScalarBufferLength(stringMaxByteCount));
            var offset = 0;
            BeginScalar(output, ref offset);
            offset += StringEncoding.Utf8.GetBytes(value, output[offset..]);
            EndScalar(output, ref offset);
            writer.Advance(offset);
        }

        void WriteLiteralScalar(string value)
        {
            var indentCharCount = (currentIndentLevel + 1) * options.IndentWidth;
            var scalarStringBuilt = EmitStringAnalyzer.BuildLiteralScalar(value, indentCharCount);
            var scalarChars = stringBuffer.AsSpan(scalarStringBuilt.Length);
            scalarStringBuilt.CopyTo(0, scalarChars, scalarStringBuilt.Length);

            if (NextState is EmitState.BlockMappingValue or EmitState.BlockSequenceEntry)
            {
                scalarChars = scalarChars[..^1]; // Remove duplicate last line-break;
            }

            var maxByteCount = StringEncoding.Utf8.GetMaxByteCount(scalarChars.Length);
            var offset = 0;
            var output = writer.GetSpan(GetScalarBufferLength(maxByteCount));
            BeginScalar(output, ref offset);
            offset += StringEncoding.Utf8.GetBytes(scalarChars, output[offset..]);
            EndScalar(output, ref offset);
            writer.Advance(offset);
        }

        void WriteQuotedScalar(string value, bool doubleQuote = true)
        {
            var scalarStringBuilt = EmitStringAnalyzer.BuildQuotedScalar(value, doubleQuote);
            var scalarChars = stringBuffer.AsSpan(scalarStringBuilt.Length);
            scalarStringBuilt.CopyTo(0, scalarChars, scalarStringBuilt.Length);

            var maxByteCount = StringEncoding.Utf8.GetMaxByteCount(scalarChars.Length);
            var offset = 0;
            var output = writer.GetSpan(GetScalarBufferLength(maxByteCount));
            BeginScalar(output, ref offset);
            offset += StringEncoding.Utf8.GetBytes(scalarChars, output[offset..]);
            EndScalar(output, ref offset);
            writer.Advance(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteIndent(Span<byte> output, ref int offset, int forceWidth = -1)
        {
            int length;
            if (forceWidth > -1)
            {
                if (forceWidth <= 0) return;
                length = forceWidth;
            }
            else if (currentIndentLevel > 0)
            {
                length = currentIndentLevel * options.IndentWidth;
            }
            else
            {
                return;
            }

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
            switch (NextState)
            {
                case EmitState.BlockSequenceEntry:
                    length += 3; // "- " + "\n"
                    break;
                case EmitState.BlockMappingKey:
                    length += 2; // ": "
                    break;
                case EmitState.BlockMappingValue:
                    break;
                case EmitState.FlowSequenceEntry:
                    length += 2; // ", "
                    break;
                case EmitState.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void BeginScalar(Span<byte> output, ref int offset)
        {
            if (!nodePrefix.IsEmpty)
            {
                nodePrefix = Span<byte>.Empty;
                throw new NotImplementedException();
            }

            switch (NextState)
            {
                case EmitState.BlockSequenceEntry:
                {
                    WriteIndent(output, ref offset);
                    BlockSequenceEntryHeader.CopyTo(output[offset..]);
                    offset += BlockSequenceEntryHeader.Length;
                    break;
                }
                case EmitState.BlockMappingKey:
                {
                    // First key in block-sequence is like so that: "- key: .."
                    if (currentTokenCount <= 0 && stateStack[^2] == EmitState.BlockSequenceEntry)
                    {
                        WriteIndent(output, ref offset, options.IndentWidth - 2);
                    }
                    else
                    {
                        WriteIndent(output, ref offset);
                    }
                    break;
                }
                case EmitState.BlockMappingValue:
                {
                    break;
                }
                case EmitState.FlowSequenceEntry:
                    if (currentTokenCount > 0)
                    {
                        output[offset++] = YamlCodes.Comma;
                        output[offset++] = YamlCodes.Space;
                    }
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
            switch (NextState)
            {
                case EmitState.BlockSequenceEntry:
                    output[offset++] = YamlCodes.Lf;
                    currentTokenCount++;
                    break;
                case EmitState.BlockMappingKey:
                    MappingKeyFooter.CopyTo(output[offset..]);
                    offset += MappingKeyFooter.Length;
                    ReplaceNextState(EmitState.BlockMappingValue);
                    break;
                case EmitState.BlockMappingValue:
                    output[offset++] = YamlCodes.Lf;
                    ReplaceNextState(EmitState.BlockMappingKey);
                    currentTokenCount++;
                    break;
                case EmitState.FlowSequenceEntry:
                    currentTokenCount++;
                    break;
                case EmitState.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReplaceNextState(EmitState newState)
        {
            stateStack[^1] = newState;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushState(EmitState state)
        {
            stateStack.Add(state);
            elementCountStack.Add(currentTokenCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PopState()
        {
            stateStack.Pop();
            currentTokenCount = elementCountStack.Length > 0 ? elementCountStack.Pop() : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IncreaseIndent()
        {
            currentIndentLevel++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DecreaseIndent()
        {
            if (currentIndentLevel > 0)
                currentIndentLevel--;
        }
    }
}
