using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using VYaml.Internal;

namespace VYaml
{
    public class YamlParserException : Exception
    {
        public YamlParserException(in Marker marker, string message)
            : base($"{message} at {marker}")
        {
        }
    }

    public enum ParseEventType
    {
        /// Reserved for internal use
        Nothing,
        StreamStart,
        StreamEnd,
        DocumentStart,
        DocumentEnd,
        /// Refer to an anchor ID
        Alias,
        /// Value, style, anchor_id, tag
        Scalar,
        /// Anchor ID
        SequenceStart,
        SequenceEnd,
        /// Anchor ID
        MappingStart,
        MappingEnd,
    }

    public enum ScalarStyle
    {
        Any,
        Plain,
        SingleQuoted,
        DoubleQuoted,
        Literal,
        Foled,
    }

    // readonly struct ParseEvent
    // {
    //     public readonly ParseEventType Type;
    //     public readonly Marker Mark;
    //     public readonly int AnchorId;
    //     public readonly ScalarStyle ScalarStyle;
    //
    //     public ParseEvent(
    //         ParseEventType type,
    //         Marker mark,
    //         int anchorId = 0,
    //         ScalarStyle scalarStyle = default)
    //     {
    //         Type = type;
    //         Mark = mark;
    //         AnchorId = anchorId;
    //         ScalarStyle = scalarStyle;
    //     }
    // }

    enum ParseState
    {
        StreamStart,
        ImplicitDocumentStart,
        DocumentStart,
        DocumentContent,
        DocumentEnd,
        BlockNode,
        // BlockNodeOrIndentlessSequence,
        // FlowNode,
        BlockSequenceFirstEntry,
        BlockSequenceEntry,
        IndentlessSequenceEntry,
        BlockMappingFirstKey,
        BlockMappingKey,
        BlockMappingValue,
        FlowSequenceFirstEntry,
        FlowSequenceEntry,
        FlowSequenceEntryMappingKey,
        FlowSequenceEntryMappingValue,
        FlowSequenceEntryMappingEnd,
        FlowMappingFirstKey,
        FlowMappingKey,
        FlowMappingValue,
        FlowMappingEmptyValue,
        End,
    }

    public ref struct Parser
    {
        public static Parser FromFilePath(string path)
        {
            throw new NotImplementedException();
        }

        public static Parser FromStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public static Parser FromBytes(byte[] bytes)
        {
            var sequence = new ReadOnlySequence<byte>(bytes);
            var tokenizer = new Utf8Tokenizer(sequence);
            return new Parser(tokenizer);
        }

        public static Parser FromString(string content)
        {
            var bytes = StringEncoding.Utf8.GetBytes(content);
            return FromBytes(bytes);
        }

        public static Parser FromSequence(ReadOnlySequence<byte> sequence)
        {
            var tokenizer = new Utf8Tokenizer(sequence);
            return new Parser(tokenizer);
        }

        public ParseEventType CurrentEventType { get; private set; }

        public Marker CurrentMark
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => tokenizer.CurrentMark;
        }

        public int CurrentAnchorId { get; private set; }

        TokenType CurrentTokenType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => tokenizer.CurrentTokenType;
        }

        Utf8Tokenizer tokenizer;
        ParseState currentState;
        Scalar? currentScalar;
        int lastAnchorId;
        readonly Dictionary<string, int> anchors;
        readonly ExpandBuffer<ParseState> stateStack;

        public Parser(Utf8Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            currentState = ParseState.StreamStart;
            CurrentEventType = default;
            CurrentAnchorId = -1;
            anchors = new Dictionary<string, int>();
            stateStack = new ExpandBuffer<ParseState>(24);
            currentScalar = null;
            lastAnchorId = -1;
        }

        public readonly bool IsNullScalar()
        {
            if (currentScalar is { } scalar)
                return scalar.IsNull();
            return false;
        }

        public readonly string? GetScalarAsString()
        {
            return currentScalar?.ToString();
        }

        public readonly bool TryGetScalarAsBool(out bool value)
        {
            if (currentScalar is { } scalar)
                return scalar.TryGetBool(out value);
            value = default;
            return false;
        }

        public readonly bool TryGetScalarAsInt64(out long value)
        {
            if (currentScalar is { } scalar)
                return scalar.TryGetInt64(out value);
            value = default;
            return false;
        }

        public readonly bool TryGetScalarAsInt32(out int value)
        {
            if (currentScalar is { } scalar)
                return scalar.TryGetInt32(out value);
            value = default;
            return false;
        }

        public readonly bool TryGetScalarAsDouble(out double value)
        {
            if (currentScalar is { } scalar)
                return scalar.TryGetDouble(out value);
            value = default;
            return false;
        }

        public bool Read()
        {
            if (currentScalar is { } scalar)
            {
                tokenizer.ReturnScalarToPool(scalar);
                currentScalar = null;
            }

            if (currentState == ParseState.End)
            {
                CurrentEventType = ParseEventType.StreamEnd;
                return false;
            }

            CurrentEventType = StateMachine();
            return true;
        }

        ParseEventType StateMachine()
        {
            switch (currentState)
            {
                case ParseState.StreamStart:
                    return ParseStreamStart();

                case ParseState.ImplicitDocumentStart:
                    return ParseDocumentStart(true);

                case ParseState.DocumentStart:
                    return ParseDocumentStart(false);

                case ParseState.DocumentContent:
                    return ParseDocumentContent();

                case ParseState.DocumentEnd:
                    return ParseDocumentEnd();

                case ParseState.BlockNode:
                    return ParseNode(true, false);

                case ParseState.BlockMappingFirstKey:
                    return ParseBlockMappingKey(true);

                case ParseState.BlockMappingKey:
                    return ParseBlockMappingKey(false);

                case ParseState.BlockMappingValue:
                    return ParseBlockMappingValue();

                case ParseState.BlockSequenceFirstEntry:
                    return ParseBlockSequenceEntry(true);

                case ParseState.BlockSequenceEntry:
                    return ParseBlockSequenceEntry(false);

                case ParseState.FlowSequenceFirstEntry:
                    return ParseFlowSequenceEntry(true);

                case ParseState.FlowSequenceEntry:
                    return ParseFlowSequenceEntry(false);

                case ParseState.FlowMappingFirstKey:
                    return ParseFlowMappingKey(true);

                case ParseState.FlowMappingKey:
                    return ParseFlowMappingKey(false);

                case ParseState.FlowMappingValue:
                    return ParseFlowMappingValue(false);

                case ParseState.IndentlessSequenceEntry:
                    return ParseIndentlessSequenceEntry();

                case ParseState.FlowSequenceEntryMappingKey:
                    return ParseFlowSequenceEntryMappingKey();

                case ParseState.FlowSequenceEntryMappingValue:
                    return ParseFlowSequenceEntryMappingValue();

                case ParseState.FlowSequenceEntryMappingEnd:
                    return ParseFlowSequenceEntryMappingEnd();

                case ParseState.FlowMappingEmptyValue:
                    return ParseFlowMappingValue(true);

                case ParseState.End:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        ParseEventType ParseStreamStart()
        {
            if (CurrentTokenType == TokenType.None)
            {
                tokenizer.Read();
            }
            ThrowIfCurrentTokenUnless(TokenType.StreamStart);
            currentState = ParseState.ImplicitDocumentStart;
            tokenizer.Read();
            return ParseEventType.StreamStart;
        }

        ParseEventType ParseDocumentStart(bool implicitStarted)
        {
            if (!implicitStarted)
            {
                while (tokenizer.CurrentTokenType == TokenType.DocumentEnd)
                {
                    tokenizer.Read();
                }
            }

            switch (tokenizer.CurrentTokenType)
            {
                case TokenType.StreamEnd:
                    currentState = ParseState.End;
                    tokenizer.Read();
                    return ParseEventType.StreamEnd;

                case TokenType.VersionDirective:
                case TokenType.TagDirective:
                case TokenType.DocumentStart:
                    return ParseExplicitDocumentStart();

                default:
                    if (implicitStarted)
                    {
                        ProcessDirectives();
                        PushState(ParseState.DocumentEnd);
                        currentState = ParseState.BlockNode;
                        return ParseEventType.DocumentStart;
                    }
                    return ParseExplicitDocumentStart();
            }
        }

        ParseEventType ParseExplicitDocumentStart()
        {
            ProcessDirectives();
            ThrowIfCurrentTokenUnless(TokenType.DocumentStart);
            PushState(ParseState.DocumentEnd);
            currentState = ParseState.DocumentContent;
            tokenizer.Read();
            return ParseEventType.DocumentStart;
        }

        ParseEventType ParseDocumentContent()
        {
            switch (tokenizer.CurrentTokenType)
            {
                case TokenType.VersionDirective:
                case TokenType.TagDirective:
                case TokenType.DocumentStart:
                case TokenType.DocumentEnd:
                case TokenType.StreamEnd:
                    PopState();
                    return EmptyScalar();
                default:
                    return ParseNode(true, false);
            }
        }

        ParseEventType ParseDocumentEnd()
        {
            var _implicit = true;
            if (CurrentTokenType == TokenType.DocumentEnd)
            {
                _implicit = false;
                tokenizer.Read();
            }

            // TODO tag handling
            currentState = ParseState.DocumentStart;
            return ParseEventType.DocumentEnd;
        }

        ParseEventType ParseNode(bool block, bool indentlessSequence)
        {
            var anchorId = 0;
            var tag = default(TokenType);

            switch (tokenizer.CurrentTokenType)
            {
                case TokenType.Alias:
                    throw new NotImplementedException();

                case TokenType.Anchor:
                    throw new NotImplementedException();

                case TokenType.Tag:
                    tokenizer.Read();
                    ThrowIfCurrentTokenUnless(TokenType.Anchor);
                    throw new NotImplementedException();
                    // tokenizer.Read();
                    // break;
            }

            switch (CurrentTokenType)
            {
                case TokenType.BlockEntryStart when indentlessSequence:
                    currentState = ParseState.IndentlessSequenceEntry;
                    CurrentAnchorId = anchorId;
                    return ParseEventType.SequenceStart;

                case TokenType.PlainScalar:
                case TokenType.FoldedScalar:
                case TokenType.LiteralScalar:
                case TokenType.SingleQuotedScaler:
                case TokenType.DoubleQuotedScaler:
                    PopState();
                    currentScalar = tokenizer.TakeCurrentScalar();
                    CurrentAnchorId = anchorId;
                    tokenizer.Read();
                    return ParseEventType.Scalar;

                case TokenType.FlowSequenceStart:
                    currentState = ParseState.FlowSequenceFirstEntry;
                    CurrentAnchorId = anchorId;
                    return ParseEventType.SequenceStart;

                case TokenType.FlowMappingStart:
                    currentState = ParseState.FlowMappingFirstKey;
                    CurrentAnchorId = anchorId;
                    return ParseEventType.MappingStart;

                case TokenType.BlockSequenceStart when block:
                    currentState = ParseState.BlockSequenceFirstEntry;
                    CurrentAnchorId = anchorId;
                    return ParseEventType.SequenceStart;

                case TokenType.BlockMappingStart when block:
                    currentState = ParseState.BlockMappingFirstKey;
                    CurrentAnchorId = anchorId;
                    return ParseEventType.MappingStart;

                default:
                    // ex 7.2, an empty scalar can follow a secondary tag
                    if (anchorId > 0)
                    {
                        throw new NotImplementedException();
                    }
                    throw new YamlTokenizerException(tokenizer.CurrentMark, "while parsing a node, did not find expected node content");
            }
        }

        ParseEventType ParseBlockMappingKey(bool first)
        {
            // skip BlockMappingStart
            if (first)
            {
                tokenizer.Read();
            }

            switch (CurrentTokenType)
            {
                case TokenType.KeyStart:
                    tokenizer.Read();
                    if (CurrentTokenType is
                        TokenType.KeyStart or
                        TokenType.ValueStart or
                        TokenType.BlockEnd)
                    {
                        currentState = ParseState.BlockMappingValue;
                        return EmptyScalar();
                    }
                    PushState(ParseState.BlockMappingValue);
                    return ParseNode(true, true);

                case TokenType.ValueStart:
                    currentState = ParseState.BlockMappingValue;
                    return EmptyScalar();

                case TokenType.BlockEnd:
                    PopState();
                    tokenizer.Read();
                    return ParseEventType.MappingEnd;

                default:
                    throw new YamlParserException(CurrentMark,
                        "while parsing a block mapping, did not find expected key");
            }
        }

        ParseEventType ParseBlockMappingValue()
        {
            if (CurrentTokenType == TokenType.ValueStart)
            {
                tokenizer.Read();
                if (CurrentTokenType is
                    TokenType.KeyStart or
                    TokenType.ValueStart or
                    TokenType.BlockEnd)
                {
                    currentState = ParseState.BlockMappingKey;
                    return EmptyScalar();
                }

                PushState(ParseState.BlockMappingKey);
                return ParseNode(true, true);
            }

            currentState = ParseState.BlockMappingKey;
            return EmptyScalar();
        }

        ParseEventType ParseBlockSequenceEntry(bool first)
        {
            // BLOCK-SEQUENCE-START
            if (first)
            {
                tokenizer.Read();
            }

            switch (CurrentTokenType)
            {
                case TokenType.BlockEnd:
                    PopState();
                    tokenizer.Read();
                    return ParseEventType.SequenceEnd;

                case TokenType.BlockEntryStart:
                    tokenizer.Read();
                    if (CurrentTokenType is TokenType.BlockEntryStart or TokenType.BlockEnd)
                    {
                        currentState = ParseState.BlockSequenceEntry;
                        return EmptyScalar();
                    }

                    PushState(ParseState.BlockSequenceEntry);
                    return ParseNode(true, false);

                default:
                    throw new YamlParserException(CurrentMark,
                        "while parsing a block collection, did not find expected '-' indicator");
            }
        }

        ParseEventType ParseFlowSequenceEntry(bool first)
        {
            // skip FlowMappingStart
            if (first)
            {
                tokenizer.Read();
            }

            switch (CurrentTokenType)
            {
                case TokenType.FlowSequenceEnd:
                    PopState();
                    tokenizer.Read();
                    return ParseEventType.SequenceEnd;

                case TokenType.FlowEntryStart when !first:
                    tokenizer.Read();
                    break;

                default:
                    if (!first)
                    {
                        throw new YamlParserException(CurrentMark,
                            "while parsing a flow sequence, expected ',' or ']'");
                    }
                    break;
            }

            switch (CurrentTokenType)
            {
                case TokenType.FlowSequenceEnd:
                    PopState();
                    tokenizer.Read();
                    return ParseEventType.SequenceEnd;

                case TokenType.KeyStart:
                    currentState = ParseState.FlowSequenceEntryMappingKey;
                    tokenizer.Read();
                    return ParseEventType.MappingStart;

                default:
                    PushState(ParseState.FlowSequenceEntry);
                    return ParseNode(false, false);

            }
        }

        ParseEventType ParseFlowMappingKey(bool first)
        {
            if (first)
            {
                tokenizer.Read();
            }

            if (CurrentTokenType == TokenType.FlowMappingEnd)
            {
                PopState();
                tokenizer.Read();
                return ParseEventType.MappingEnd;
            }

            if (!first)
            {
                if (CurrentTokenType == TokenType.FlowEntryStart)
                {
                    tokenizer.Read();
                }
                else
                {
                    throw new YamlParserException(CurrentMark,
                        "While parsing a flow mapping, did not find expected ',' or '}'");
                }
            }

            switch (CurrentTokenType)
            {
                case TokenType.KeyStart:
                    tokenizer.Read();
                    if (CurrentTokenType is
                        TokenType.ValueStart or
                        TokenType.FlowEntryStart or
                        TokenType.FlowMappingEnd)
                    {
                        currentState = ParseState.FlowMappingValue;
                        return EmptyScalar();
                    }
                    PushState(ParseState.FlowMappingValue);
                    return ParseNode(false, false);

                case TokenType.ValueStart:
                    currentState = ParseState.FlowMappingValue;
                    return EmptyScalar();

                case TokenType.FlowMappingEnd:
                    PopState();
                    tokenizer.Read();
                    return ParseEventType.MappingEnd;

                default:
                    PushState(ParseState.FlowMappingEmptyValue);
                    return ParseNode(false, false);
            }
        }

        ParseEventType ParseFlowMappingValue(bool empty)
        {
            if (empty)
            {
                currentState = ParseState.FlowMappingKey;
                return EmptyScalar();
            }

            if (CurrentTokenType == TokenType.ValueStart)
            {
                tokenizer.Read();
                if (CurrentTokenType != TokenType.FlowEntryStart &&
                    CurrentTokenType != TokenType.FlowMappingEnd)
                {
                    PushState(ParseState.FlowMappingKey);
                    return ParseNode(false, false);
                }
            }

            currentState = ParseState.FlowMappingKey;
            return EmptyScalar();
        }

        ParseEventType ParseIndentlessSequenceEntry()
        {
            if (CurrentTokenType != TokenType.BlockEntryStart)
            {
                PopState();
                return ParseEventType.SequenceEnd;
            }

            tokenizer.Read();

            if (CurrentTokenType is
                TokenType.KeyStart or
                TokenType.ValueStart or
                TokenType.BlockEnd)
            {
                currentState = ParseState.IndentlessSequenceEntry;
                return EmptyScalar();
            }

            PushState(ParseState.IndentlessSequenceEntry);
            return ParseNode(true, false);
        }

        ParseEventType ParseFlowSequenceEntryMappingKey()
        {
            if (CurrentTokenType is
                TokenType.ValueStart or
                TokenType.FlowEntryStart or
                TokenType.FlowSequenceEnd)
            {
                tokenizer.Read();
                currentState = ParseState.FlowSequenceEntryMappingValue;
                return EmptyScalar();
            }
            PushState(ParseState.FlowSequenceEntryMappingValue);
            return ParseNode(false, false);
        }

        ParseEventType ParseFlowSequenceEntryMappingValue()
        {
            if (CurrentTokenType == TokenType.ValueStart)
            {
                tokenizer.Read();
                currentState = ParseState.FlowSequenceEntryMappingValue;
                if (CurrentTokenType is
                    TokenType.FlowEntryStart or
                    TokenType.FlowSequenceEnd)
                {
                    currentState = ParseState.FlowSequenceEntryMappingEnd;
                    return EmptyScalar();
                }
                PushState(ParseState.FlowSequenceEntryMappingEnd);
                return ParseNode(false, false);
            }

            currentState = ParseState.FlowSequenceEntryMappingEnd;
            return EmptyScalar();
        }

        ParseEventType ParseFlowSequenceEntryMappingEnd()
        {
            currentState = ParseState.FlowSequenceEntry;
            return ParseEventType.MappingEnd;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PopState()
        {
            currentState = stateStack.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PushState(ParseState state)
        {
            stateStack.Add(state);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ParseEventType EmptyScalar()
        {
            currentScalar = Scalar.Null;
            return ParseEventType.Scalar;
        }

        void ProcessDirectives()
        {
            while (true)
            {
                switch (tokenizer.CurrentTokenType)
                {
                    case TokenType.VersionDirective:
                        // TODO:
                        break;
                    case TokenType.TagDirective:
                        // TODO:
                        break;
                    default:
                        return;
                }
                tokenizer.Read();
            }
        }

        void RegisterAnchor()
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ThrowIfCurrentTokenUnless(TokenType expectedTokenType)
        {
            if (CurrentTokenType != expectedTokenType)
            {
                throw new YamlParserException(tokenizer.CurrentMark,
                    $"Did not find expected token of  `{expectedTokenType}`");
            }
        }
    }
}
