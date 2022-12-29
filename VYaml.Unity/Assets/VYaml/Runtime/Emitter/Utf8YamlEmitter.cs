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
        static readonly byte[] MappingKeyFooter = { (byte)':', (byte)' ' };
        static readonly byte[] FlowSequenceEmpty = { (byte)'[', (byte)']' };
        static readonly byte[] FlowSequenceSeparator = { (byte)',', (byte)' ' };

        EmitState NextState => stateStack.Peek();

        readonly IBufferWriter<byte> writer;
        readonly YamlEmitOptions options;

        ExpandBuffer<char> stringBuffer;
        ExpandBuffer<EmitState> stateStack;

        int currentIndentLevel;
        int currentElementCount;

        public Utf8YamlEmitter(IBufferWriter<byte> writer, YamlEmitOptions? options = null)
        {
            this.writer = writer;
            this.options = options ?? YamlEmitOptions.Default;

            currentIndentLevel = 0;
            stringBuffer = new ExpandBuffer<char>(1024);
            stateStack = new ExpandBuffer<EmitState>(16);
            stateStack.Add(EmitState.None);
            currentElementCount = 0;
        }

        internal readonly IBufferWriter<byte> GetWriter() => writer;

        public void Dispose()
        {
            stringBuffer.Dispose();
            stateStack.Dispose();
        }

        public void BeginSequence(SequenceStyle style = SequenceStyle.Block)
        {
            switch (style)
            {
                case SequenceStyle.Block:
                {
                    switch (NextState)
                    {
                        case EmitState.BlockSequenceEntry:
                        {
                            WriteRaw(BlockSequenceEntryHeaderEmpty, true);
                            IncreaseIndent();
                            break;
                        }
                        case EmitState.BlockMappingKey:
                            throw new YamlEmitterException("To start block-sequence in the mapping key is not supported.");

                        case EmitState.BlockMappingValue:
                        {
                            var output = writer.GetSpan(1);
                            output[0] = YamlCodes.Lf;
                            writer.Advance(1);
                            // IncreaseIndent();
                            ReplaceNextState(EmitState.BlockMappingKey);
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
                        case EmitState.BlockMappingValue:
                            throw new YamlEmitterException("To start flow-mapping in the mapping key is not supported.");
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
                    currentElementCount = 0;
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
                    PopState();
                    DecreaseIndent();
                    if (currentElementCount == 0)
                    {
                        var output = writer.GetSpan(FlowSequenceEmpty.Length);
                        FlowSequenceEmpty.CopyTo(output);
                        writer.Advance(FlowSequenceEmpty.Length);
                    }
                    break;
                }

                case EmitState.FlowSequenceEntry:
                {
                    PopState();
                    var output = writer.GetSpan(1);
                    output[0] = YamlCodes.FlowSequenceEnd;
                    writer.Advance(1);
                    break;
                }

                default:
                    throw new YamlEmitterException($"Current state is not sequence: {NextState}");
            }
        }

        public void BeginMapping(MappingStyle style = MappingStyle.Block)
        {
            switch (style)
            {
                case MappingStyle.Block:
                {
                    switch (NextState)
                    {
                        case EmitState.BlockMappingKey:
                            throw new YamlEmitterException("To start block-mapping in the mapping key is not supported.");
                        case EmitState.BlockMappingValue:
                        {
                            IncreaseIndent();
                            ReplaceNextState(EmitState.BlockMappingKey);
                            var output = writer.GetSpan(1);
                            output[0] = YamlCodes.Lf;
                            writer.Advance(1);
                            break;
                        }
                        case EmitState.BlockSequenceEntry:
                        {
                            WriteRaw(BlockSequenceEntryHeaderEmpty, true);
                            IncreaseIndent();
                            break;
                        }
                    }
                    PushState(EmitState.BlockMappingKey);
                    currentElementCount = 0;
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
            var output = writer.GetSpan(GetScalarBufferLength(64));

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
            var output = writer.GetSpan(GetScalarBufferLength(128));

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
            if (style == ScalarStyle.Plain)
            {
                WritePlainScalar(value);
                return;
            }

            var analyzeInfo = EmitStringAnalyzer.Analyze(value);
            if (style == ScalarStyle.Any)
            {
                style = analyzeInfo.SuggestScalarStyle();
            }

            switch (style)
            {
                case ScalarStyle.Plain:
                    WritePlainScalar(value);
                    break;
                // case ScalarStyle.SingleQuoted:
                //     break;
                case ScalarStyle.DoubleQuoted:
                    WriteDoubleQuotedScalar(value, analyzeInfo);
                    break;
                case ScalarStyle.Literal:
                    WriteLiteralScalar(value, analyzeInfo);
                    break;
                // case ScalarStyle.Folded:
                //     break;
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

        void WriteDoubleQuotedScalar(string value, in EmitStringInfo analyzeInfo)
        {

        }

        void WriteLiteralScalar(string value, in EmitStringInfo analyzeInfo)
        {
            var indentCharCount = (currentIndentLevel + 1) * options.IndentWidth;
            var scalarValueBuilder = EmitStringAnalyzer.ToLiteralScalar(value, analyzeInfo.ChompHint, indentCharCount);

            stringBuffer.SetCapacity(scalarValueBuilder.Length);
            var scalarChars = stringBuffer.AsSpan(scalarValueBuilder.Length);
            scalarValueBuilder.CopyTo(0, scalarChars, scalarValueBuilder.Length);

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

        void WriteFoldedScalar(string value, in EmitStringInfo analyzeInfo)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteRaw(ReadOnlySpan<byte> value, bool indent)
        {
            var length = value.Length + (indent ? currentIndentLevel * options.IndentWidth : 0);
            var offset = 0;
            var output = writer.GetSpan(length);
            if (indent)
            {
                WriteIndent(output, ref offset);
                output = output[offset..];
            }
            value.CopyTo(output);
            writer.Advance(length);
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
            switch (NextState)
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
                case EmitState.FlowSequenceEntry:
                    if (currentElementCount > 0)
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
                    currentElementCount++;
                    break;
                case EmitState.BlockMappingKey:
                    MappingKeyFooter.CopyTo(output[offset..]);
                    offset += MappingKeyFooter.Length;
                    ReplaceNextState(EmitState.BlockMappingValue);
                    break;
                case EmitState.BlockMappingValue:
                    output[offset++] = YamlCodes.Lf;
                    ReplaceNextState(EmitState.BlockMappingKey);
                    currentElementCount++;
                    break;
                case EmitState.FlowSequenceEntry:
                    currentElementCount++;
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PopState()
        {
            stateStack.Pop();
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
