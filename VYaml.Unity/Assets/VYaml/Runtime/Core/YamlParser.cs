using System;
using System.Buffers;
using System.Collections.Generic;
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

    public enum ParseEventType : byte
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

    public ref struct YamlParser
    {
        public static YamlParser FromBytes(Memory<byte> bytes)
        {
            var sequence = new ReadOnlySequence<byte>(bytes);
            var tokenizer = new Utf8YamlTokenizer(sequence);
            return new YamlParser(tokenizer);
        }

        public static YamlParser FromSequence(ReadOnlySequence<byte> sequence)
        {
            var tokenizer = new Utf8YamlTokenizer(sequence);
            return new YamlParser(tokenizer);
        }

        public ParseEventType CurrentEventType { get; private set; }

        public Marker CurrentMark
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => yamlTokenizer.CurrentMark;
        }

        public int CurrentAnchorId { get; private set; }

        TokenType CurrentTokenType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => yamlTokenizer.CurrentTokenType;
        }

        Utf8YamlTokenizer yamlTokenizer;
        ParseState currentState;
        Scalar? currentScalar;
        Tag? currentTag;
        int lastAnchorId;

        readonly Dictionary<string, int> anchors;
        readonly ExpandBuffer<ParseState> stateStack;

        public YamlParser(Utf8YamlTokenizer yamlTokenizer)
        {
            this.yamlTokenizer = yamlTokenizer;
            currentState = ParseState.StreamStart;
            CurrentEventType = default;
            lastAnchorId = -1;
            anchors = new Dictionary<string, int>();
            stateStack = new ExpandBuffer<ParseState>(24);

            currentScalar = null;
            currentTag = null;
            CurrentAnchorId = -1;
        }

        public readonly bool IsNullScalar()
        {
            if (currentScalar is { } scalar)
                return scalar.IsNull();
            return true;
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
                yamlTokenizer.ReturnToPool(scalar);
                currentScalar = null;
            }
            if (currentTag is { } tag)
            {
                yamlTokenizer.ReturnToPool(tag.Handle);
                yamlTokenizer.ReturnToPool(tag.Suffix);
                currentTag = null;
            }

            if (currentState == ParseState.End)
            {
                CurrentEventType = ParseEventType.StreamEnd;
                return false;
            }

            switch (currentState)
            {
                case ParseState.StreamStart:
                    ParseStreamStart();
                    break;

                case ParseState.ImplicitDocumentStart:
                    ParseDocumentStart(true);
                    break;

                case ParseState.DocumentStart:
                    ParseDocumentStart(false);
                    break;

                case ParseState.DocumentContent:
                    ParseDocumentContent();
                    break;

                case ParseState.DocumentEnd:
                    ParseDocumentEnd();
                    break;

                case ParseState.BlockNode:
                    ParseNode(true, false);
                    break;

                case ParseState.BlockMappingFirstKey:
                    ParseBlockMappingKey(true);
                    break;

                case ParseState.BlockMappingKey:
                    ParseBlockMappingKey(false);
                    break;

                case ParseState.BlockMappingValue:
                    ParseBlockMappingValue();
                    break;

                case ParseState.BlockSequenceFirstEntry:
                    ParseBlockSequenceEntry(true);
                    break;

                case ParseState.BlockSequenceEntry:
                    ParseBlockSequenceEntry(false);
                    break;

                case ParseState.FlowSequenceFirstEntry:
                    ParseFlowSequenceEntry(true);
                    break;

                case ParseState.FlowSequenceEntry:
                    ParseFlowSequenceEntry(false);
                    break;

                case ParseState.FlowMappingFirstKey:
                    ParseFlowMappingKey(true);
                    break;

                case ParseState.FlowMappingKey:
                    ParseFlowMappingKey(false);
                    break;

                case ParseState.FlowMappingValue:
                    ParseFlowMappingValue(false);
                    break;

                case ParseState.IndentlessSequenceEntry:
                    ParseIndentlessSequenceEntry();
                    break;

                case ParseState.FlowSequenceEntryMappingKey:
                    ParseFlowSequenceEntryMappingKey();
                    break;

                case ParseState.FlowSequenceEntryMappingValue:
                    ParseFlowSequenceEntryMappingValue();
                    break;

                case ParseState.FlowSequenceEntryMappingEnd:
                    ParseFlowSequenceEntryMappingEnd();
                    break;

                case ParseState.FlowMappingEmptyValue:
                    ParseFlowMappingValue(true);
                    break;

                case ParseState.End:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        void ParseStreamStart()
        {
            if (CurrentTokenType == TokenType.None)
            {
                yamlTokenizer.Read();
            }
            ThrowIfCurrentTokenUnless(TokenType.StreamStart);
            currentState = ParseState.ImplicitDocumentStart;
            yamlTokenizer.Read();
            CurrentEventType = ParseEventType.StreamStart;
        }

        void ParseDocumentStart(bool implicitStarted)
        {
            if (!implicitStarted)
            {
                while (yamlTokenizer.CurrentTokenType == TokenType.DocumentEnd)
                {
                    yamlTokenizer.Read();
                }
            }

            switch (yamlTokenizer.CurrentTokenType)
            {
                case TokenType.StreamEnd:
                    currentState = ParseState.End;
                    yamlTokenizer.Read();
                    CurrentEventType = ParseEventType.StreamEnd;
                    break;

                case TokenType.VersionDirective:
                case TokenType.TagDirective:
                case TokenType.DocumentStart:
                    ParseExplicitDocumentStart();
                    break;

                default:
                    if (implicitStarted)
                    {
                        ProcessDirectives();
                        PushState(ParseState.DocumentEnd);
                        currentState = ParseState.BlockNode;
                        CurrentEventType = ParseEventType.DocumentStart;
                    }
                    else
                    {
                        ParseExplicitDocumentStart();
                    }
                    break;
            }
        }

        void ParseExplicitDocumentStart()
        {
            ProcessDirectives();
            ThrowIfCurrentTokenUnless(TokenType.DocumentStart);
            PushState(ParseState.DocumentEnd);
            currentState = ParseState.DocumentContent;
            yamlTokenizer.Read();
            CurrentEventType = ParseEventType.DocumentStart;
        }

        void ParseDocumentContent()
        {
            switch (yamlTokenizer.CurrentTokenType)
            {
                case TokenType.VersionDirective:
                case TokenType.TagDirective:
                case TokenType.DocumentStart:
                case TokenType.DocumentEnd:
                case TokenType.StreamEnd:
                    PopState();
                    EmptyScalar();
                    break;
                default:
                    ParseNode(true, false);
                    break;
            }
        }

        void ParseDocumentEnd()
        {
            var _implicit = true;
            if (CurrentTokenType == TokenType.DocumentEnd)
            {
                _implicit = false;
                yamlTokenizer.Read();
            }

            // TODO tag handling
            currentState = ParseState.DocumentStart;
            CurrentEventType = ParseEventType.DocumentEnd;
        }

        void ParseNode(bool block, bool indentlessSequence)
        {
            CurrentAnchorId = -1;
            currentTag = null;

            switch (CurrentTokenType)
            {
                case TokenType.Alias:
                    PopState();

                    var name = yamlTokenizer.TakeCurrentTokenContent<Scalar>();
                    yamlTokenizer.Read();

                    if (anchors.TryGetValue(name.ToString(), out var aliasId)) // TODO: Avoid `ToString`
                    {
                        CurrentAnchorId = aliasId;
                        CurrentEventType = ParseEventType.Alias;
                        return;
                    }
                    throw new YamlParserException(CurrentMark, "While parsing node, found unknown anchor");

                case TokenType.Anchor:
                {
                    var anchorName = yamlTokenizer.TakeCurrentTokenContent<Scalar>();
                    CurrentAnchorId = RegisterAnchor(anchorName);
                    yamlTokenizer.Read();
                    if (CurrentTokenType == TokenType.Tag)
                    {
                        currentTag = yamlTokenizer.TakeCurrentTokenContent<Tag>();
                        yamlTokenizer.Read();
                    }
                    break;
                }
                case TokenType.Tag:
                {
                    currentTag = yamlTokenizer.TakeCurrentTokenContent<Tag>();
                    yamlTokenizer.Read();
                    if (CurrentTokenType == TokenType.Anchor)
                    {
                        var anchorName = yamlTokenizer.TakeCurrentTokenContent<Scalar>();
                        CurrentAnchorId = RegisterAnchor(anchorName);
                        yamlTokenizer.Read();
                    }
                    break;
                }
            }

            switch (CurrentTokenType)
            {
                case TokenType.BlockEntryStart when indentlessSequence:
                    currentState = ParseState.IndentlessSequenceEntry;
                    CurrentEventType = ParseEventType.SequenceStart;
                    break;

                case TokenType.PlainScalar:
                case TokenType.FoldedScalar:
                case TokenType.LiteralScalar:
                case TokenType.SingleQuotedScaler:
                case TokenType.DoubleQuotedScaler:
                    PopState();
                    currentScalar = yamlTokenizer.TakeCurrentTokenContent<Scalar>();
                    yamlTokenizer.Read();
                    CurrentEventType = ParseEventType.Scalar;
                    break;

                case TokenType.FlowSequenceStart:
                    currentState = ParseState.FlowSequenceFirstEntry;
                    CurrentEventType = ParseEventType.SequenceStart;
                    break;

                case TokenType.FlowMappingStart:
                    currentState = ParseState.FlowMappingFirstKey;
                    CurrentEventType = ParseEventType.MappingStart;
                    break;

                case TokenType.BlockSequenceStart when block:
                    currentState = ParseState.BlockSequenceFirstEntry;
                    CurrentEventType = ParseEventType.SequenceStart;
                    break;

                case TokenType.BlockMappingStart when block:
                    currentState = ParseState.BlockMappingFirstKey;
                    CurrentEventType = ParseEventType.MappingStart;
                    break;

                // ex 7.2, an empty scalar can follow a secondary tag
                case var _ when CurrentAnchorId >= 0 || currentTag != null:
                    PopState();
                    EmptyScalar();
                    break;

                default:
                {
                    throw new YamlTokenizerException(yamlTokenizer.CurrentMark,
                        "while parsing a node, did not find expected node content");
                }
            }
        }

        void ParseBlockMappingKey(bool first)
        {
            // skip BlockMappingStart
            if (first)
            {
                yamlTokenizer.Read();
            }

            switch (CurrentTokenType)
            {
                case TokenType.KeyStart:
                    yamlTokenizer.Read();
                    if (CurrentTokenType is
                        TokenType.KeyStart or
                        TokenType.ValueStart or
                        TokenType.BlockEnd)
                    {
                        currentState = ParseState.BlockMappingValue;
                        EmptyScalar();
                    }
                    else
                    {
                        PushState(ParseState.BlockMappingValue);
                        ParseNode(true, true);
                    }
                    break;

                case TokenType.ValueStart:
                    currentState = ParseState.BlockMappingValue;
                    EmptyScalar();
                    break;

                case TokenType.BlockEnd:
                    PopState();
                    yamlTokenizer.Read();
                    CurrentEventType = ParseEventType.MappingEnd;
                    break;

                default:
                    throw new YamlParserException(CurrentMark,
                        "while parsing a block mapping, did not find expected key");
            }
        }

        void ParseBlockMappingValue()
        {
            if (CurrentTokenType == TokenType.ValueStart)
            {
                yamlTokenizer.Read();
                if (CurrentTokenType is
                    TokenType.KeyStart or
                    TokenType.ValueStart or
                    TokenType.BlockEnd)
                {
                    currentState = ParseState.BlockMappingKey;
                    EmptyScalar();
                }
                else
                {
                    PushState(ParseState.BlockMappingKey);
                    ParseNode(true, true);
                }
            }
            else
            {
                currentState = ParseState.BlockMappingKey;
                EmptyScalar();
            }
        }

        void ParseBlockSequenceEntry(bool first)
        {
            // BLOCK-SEQUENCE-START
            if (first)
            {
                yamlTokenizer.Read();
            }

            switch (CurrentTokenType)
            {
                case TokenType.BlockEnd:
                    PopState();
                    yamlTokenizer.Read();
                    CurrentEventType = ParseEventType.SequenceEnd;
                    break;

                case TokenType.BlockEntryStart:
                    yamlTokenizer.Read();
                    if (CurrentTokenType is TokenType.BlockEntryStart or TokenType.BlockEnd)
                    {
                        currentState = ParseState.BlockSequenceEntry;
                        EmptyScalar();
                        break;
                    }

                    PushState(ParseState.BlockSequenceEntry);
                    ParseNode(true, false);
                    break;

                default:
                    throw new YamlParserException(CurrentMark,
                        "while parsing a block collection, did not find expected '-' indicator");
            }
        }

        void ParseFlowSequenceEntry(bool first)
        {
            // skip FlowMappingStart
            if (first)
            {
                yamlTokenizer.Read();
            }

            switch (CurrentTokenType)
            {
                case TokenType.FlowSequenceEnd:
                    PopState();
                    yamlTokenizer.Read();
                    CurrentEventType =  ParseEventType.SequenceEnd;
                    return;

                case TokenType.FlowEntryStart when !first:
                    yamlTokenizer.Read();
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
                    yamlTokenizer.Read();
                    CurrentEventType = ParseEventType.SequenceEnd;
                    break;

                case TokenType.KeyStart:
                    currentState = ParseState.FlowSequenceEntryMappingKey;
                    yamlTokenizer.Read();
                    CurrentEventType = ParseEventType.MappingStart;
                    break;

                default:
                    PushState(ParseState.FlowSequenceEntry);
                    ParseNode(false, false);
                    break;
            }
        }

        void ParseFlowMappingKey(bool first)
        {
            if (first)
            {
                yamlTokenizer.Read();
            }

            if (CurrentTokenType == TokenType.FlowMappingEnd)
            {
                PopState();
                yamlTokenizer.Read();
                CurrentEventType = ParseEventType.MappingEnd;
                return;
            }

            if (!first)
            {
                if (CurrentTokenType == TokenType.FlowEntryStart)
                {
                    yamlTokenizer.Read();
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
                    yamlTokenizer.Read();
                    if (CurrentTokenType is
                        TokenType.ValueStart or
                        TokenType.FlowEntryStart or
                        TokenType.FlowMappingEnd)
                    {
                        currentState = ParseState.FlowMappingValue;
                        EmptyScalar();
                        break;
                    }
                    PushState(ParseState.FlowMappingValue);
                    ParseNode(false, false);
                    break;

                case TokenType.ValueStart:
                    currentState = ParseState.FlowMappingValue;
                    EmptyScalar();
                    break;

                case TokenType.FlowMappingEnd:
                    PopState();
                    yamlTokenizer.Read();
                    CurrentEventType = ParseEventType.MappingEnd;
                    break;

                default:
                    PushState(ParseState.FlowMappingEmptyValue);
                    ParseNode(false, false);
                    break;
            }
        }

        void ParseFlowMappingValue(bool empty)
        {
            if (empty)
            {
                currentState = ParseState.FlowMappingKey;
                EmptyScalar();
                return;
            }

            if (CurrentTokenType == TokenType.ValueStart)
            {
                yamlTokenizer.Read();
                if (CurrentTokenType != TokenType.FlowEntryStart &&
                    CurrentTokenType != TokenType.FlowMappingEnd)
                {
                    PushState(ParseState.FlowMappingKey);
                    ParseNode(false, false);
                    return;
                }
            }

            currentState = ParseState.FlowMappingKey;
            EmptyScalar();
        }

        void ParseIndentlessSequenceEntry()
        {
            if (CurrentTokenType != TokenType.BlockEntryStart)
            {
                PopState();
                CurrentEventType = ParseEventType.SequenceEnd;
                return;
            }

            yamlTokenizer.Read();

            if (CurrentTokenType is
                TokenType.KeyStart or
                TokenType.ValueStart or
                TokenType.BlockEnd)
            {
                currentState = ParseState.IndentlessSequenceEntry;
                EmptyScalar();
            }
            else
            {
                PushState(ParseState.IndentlessSequenceEntry);
                ParseNode(true, false);
            }
        }

        void ParseFlowSequenceEntryMappingKey()
        {
            if (CurrentTokenType is
                TokenType.ValueStart or
                TokenType.FlowEntryStart or
                TokenType.FlowSequenceEnd)
            {
                yamlTokenizer.Read();
                currentState = ParseState.FlowSequenceEntryMappingValue;
                EmptyScalar();
            }
            else
            {
                PushState(ParseState.FlowSequenceEntryMappingValue);
                ParseNode(false, false);
            }
        }

        void ParseFlowSequenceEntryMappingValue()
        {
            if (CurrentTokenType == TokenType.ValueStart)
            {
                yamlTokenizer.Read();
                currentState = ParseState.FlowSequenceEntryMappingValue;
                if (CurrentTokenType is
                    TokenType.FlowEntryStart or
                    TokenType.FlowSequenceEnd)
                {
                    currentState = ParseState.FlowSequenceEntryMappingEnd;
                    EmptyScalar();
                }
                else
                {
                    PushState(ParseState.FlowSequenceEntryMappingEnd);
                    ParseNode(false, false);
                }
            }
            else
            {
                currentState = ParseState.FlowSequenceEntryMappingEnd;
                EmptyScalar();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ParseFlowSequenceEntryMappingEnd()
        {
            currentState = ParseState.FlowSequenceEntry;
            CurrentEventType = ParseEventType.MappingEnd;
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
        void EmptyScalar()
        {
            currentScalar = null;
            CurrentEventType = ParseEventType.Scalar;
        }

        void ProcessDirectives()
        {
            while (true)
            {
                switch (yamlTokenizer.CurrentTokenType)
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
                yamlTokenizer.Read();
            }
        }

        int RegisterAnchor(Scalar anchorName)
        {
            var newId = ++lastAnchorId;
            anchors[anchorName.ToString()] = newId; // TODO: Avoid `ToString`
            return newId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ThrowIfCurrentTokenUnless(TokenType expectedTokenType)
        {
            if (CurrentTokenType != expectedTokenType)
            {
                throw new YamlParserException(yamlTokenizer.CurrentMark,
                    $"Did not find expected token of  `{expectedTokenType}`");
            }
        }
    }
}
