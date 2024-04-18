#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VYaml.Internal;

namespace VYaml.Parser
{
    public class YamlParserException : Exception
    {
        public static void Throw(in Marker marker, string message)
        {
            throw new YamlParserException(marker, message);
        }

        public YamlParserException(in Marker marker, string message) : base($"{message} at {marker}")
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

    public ref partial struct YamlParser
    {
        [ThreadStatic]
        static Dictionary<string, int>? anchorsBufferStatic;

        [ThreadStatic]
        static ExpandBuffer<ParseState>? stateStackBufferStatic;

        public static YamlParser FromBytes(Memory<byte> bytes)
        {
            var sequence = new ReadOnlySequence<byte>(bytes);
            return new YamlParser(sequence);
        }

        public static YamlParser FromSequence(in ReadOnlySequence<byte> sequence)
        {
            return new YamlParser(sequence);
        }

        public ParseEventType CurrentEventType { get; private set; }
        public bool UnityStrippedMark { get; private set; }

        public readonly Marker CurrentMark
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => tokenizer.CurrentMark;
        }

        public bool End => CurrentEventType == ParseEventType.StreamEnd;

        TokenType CurrentTokenType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => tokenizer.CurrentTokenType;
        }

        Utf8YamlTokenizer tokenizer;
        ParseState currentState;
        Scalar? currentScalar;
        Tag? currentTag;
        Anchor? currentAnchor;
        int lastAnchorId;

        readonly Dictionary<string, int> anchors;
        readonly ExpandBuffer<ParseState> stateStack;

        public YamlParser(ReadOnlySequence<byte> sequence)
        {
            tokenizer = new Utf8YamlTokenizer(sequence);
            currentState = ParseState.StreamStart;
            CurrentEventType = default;
            lastAnchorId = -1;

            anchors = anchorsBufferStatic ??= new Dictionary<string, int>();
            anchors.Clear();

            stateStack = stateStackBufferStatic ??= new ExpandBuffer<ParseState>(16);
            stateStack.Clear();

            currentScalar = null;
            currentTag = null;
            currentAnchor = null;

            UnityStrippedMark = false;
        }

        public YamlParser(ref Utf8YamlTokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            currentState = ParseState.StreamStart;
            CurrentEventType = default;
            lastAnchorId = -1;
            anchors = new Dictionary<string, int>();
            stateStack = new ExpandBuffer<ParseState>(16);

            currentScalar = null;
            currentTag = null;
            currentAnchor = null;

            UnityStrippedMark = false;
        }

        public bool Read()
        {
            if (currentScalar is { } scalar)
            {
                ScalarPool.Shared.Return(scalar);
                currentScalar = null;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadWithVerify(ParseEventType eventType)
        {
            if (CurrentEventType != eventType)
                throw new YamlParserException(CurrentMark, $"Did not find expected event : `{eventType}`");
            Read();
        }

        public void SkipHeader()
        {
            while (CurrentEventType is ParseEventType.StreamStart or ParseEventType.DocumentStart or ParseEventType.Nothing)
            {
                if (!Read())
                {
                    break;
                }
            }
        }

        public void SkipAfter(ParseEventType eventType)
        {
            while (CurrentEventType != eventType)
            {
                if (!Read())
                {
                    break;
                }
            }
            if (CurrentEventType == eventType)
            {
                Read();
            }
        }

        public void SkipCurrentNode()
        {
            switch (CurrentEventType)
            {
                case ParseEventType.Alias:
                case ParseEventType.Scalar:
                    Read();
                    break;

                case ParseEventType.SequenceStart:
                {
                    var depth = 1;
                    while (Read())
                    {
                        switch (CurrentEventType)
                        {
                            case ParseEventType.SequenceStart:
                                ++depth;
                                break;
                            case ParseEventType.SequenceEnd when --depth <= 0:
                                Read();
                                return;
                        }
                    }
                    break;
                }
                case ParseEventType.MappingStart:
                {
                    var depth = 1;
                    while (Read())
                    {
                        switch (CurrentEventType)
                        {
                            case ParseEventType.MappingStart:
                                ++depth;
                                break;
                            case ParseEventType.MappingEnd when --depth <= 0:
                                Read();
                                return;
                        }
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void ParseStreamStart()
        {
            if (CurrentTokenType == TokenType.None)
            {
                tokenizer.Read();
            }
            ThrowIfCurrentTokenUnless(TokenType.StreamStart);
            currentState = ParseState.ImplicitDocumentStart;
            tokenizer.Read();
            CurrentEventType = ParseEventType.StreamStart;
        }

        void ParseDocumentStart(bool implicitStarted)
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
            tokenizer.Read();
            CurrentEventType = ParseEventType.DocumentStart;
        }

        void ParseDocumentContent()
        {
            switch (tokenizer.CurrentTokenType)
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
            // var _implicit = true;
            if (CurrentTokenType == TokenType.DocumentEnd)
            {
                // _implicit = false;
                tokenizer.Read();
            }

            // TODO tag handling
            currentState = ParseState.DocumentStart;
            CurrentEventType = ParseEventType.DocumentEnd;
        }

        void ParseNode(bool block, bool indentlessSequence)
        {
            currentAnchor = null;
            currentTag = null;
            UnityStrippedMark = false;

            switch (CurrentTokenType)
            {
                case TokenType.Alias:
                    PopState();

                    var name = tokenizer.TakeCurrentTokenContent<Scalar>().ToString();  // TODO: Avoid `ToString`
                    tokenizer.Read();

                    if (anchors.TryGetValue(name, out var aliasId))
                    {
                        currentAnchor = new Anchor(name, aliasId);
                        CurrentEventType = ParseEventType.Alias;
                        return;
                    }
                    throw new YamlParserException(CurrentMark, "While parsing node, found unknown anchor");

                case TokenType.Anchor:
                {
                    var anchorName = tokenizer.TakeCurrentTokenContent<Scalar>().ToString(); // TODO: Avoid `ToString`
                    var anchorId = RegisterAnchor(anchorName);
                    currentAnchor = new Anchor(anchorName, anchorId);
                    tokenizer.Read();
                    if (CurrentTokenType == TokenType.Tag)
                    {
                        currentTag = tokenizer.TakeCurrentTokenContent<Tag>();
                        tokenizer.Read();
                    }
                    break;
                }
                case TokenType.Tag:
                {
                    currentTag = tokenizer.TakeCurrentTokenContent<Tag>();
                    tokenizer.Read();
                    if (CurrentTokenType == TokenType.Anchor)
                    {
                        var anchorName = tokenizer.TakeCurrentTokenContent<Scalar>().ToString();
                        var anchorId = RegisterAnchor(anchorName);
                        currentAnchor = new Anchor(anchorName, anchorId);

                        // Unity compatible mode
                        if (CurrentEventType == ParseEventType.DocumentStart &&
                            currentTag?.Handle == "!u!")
                        {
                            UnityStrippedMark = tokenizer.TrySkipUnityStrippedSymbol();
                        }
                        tokenizer.Read();
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
                    currentScalar = tokenizer.TakeCurrentTokenContent<Scalar>();
                    tokenizer.Read();
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
                case var _ when currentAnchor != null || currentTag != null:
                    PopState();
                    EmptyScalar();
                    break;

                // consider empty entry in sequence ("- ") as null
                case TokenType.BlockEntryStart when currentState == ParseState.IndentlessSequenceEntry:
                    PopState();
                    EmptyScalar();
                    break;

                default:
                {
                    throw new YamlTokenizerException(tokenizer.CurrentMark,
                        "while parsing a node, did not find expected node content");
                }
            }
        }

        void ParseBlockMappingKey(bool first)
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
                    tokenizer.Read();
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
                tokenizer.Read();
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
                tokenizer.Read();
            }

            switch (CurrentTokenType)
            {
                case TokenType.BlockEnd:
                    PopState();
                    tokenizer.Read();
                    CurrentEventType = ParseEventType.SequenceEnd;
                    break;

                case TokenType.BlockEntryStart:
                    tokenizer.Read();
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
                tokenizer.Read();
            }

            switch (CurrentTokenType)
            {
                case TokenType.FlowSequenceEnd:
                    PopState();
                    tokenizer.Read();
                    CurrentEventType =  ParseEventType.SequenceEnd;
                    return;

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
                    CurrentEventType = ParseEventType.SequenceEnd;
                    break;

                case TokenType.KeyStart:
                    currentState = ParseState.FlowSequenceEntryMappingKey;
                    tokenizer.Read();
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
                tokenizer.Read();
            }

            if (CurrentTokenType == TokenType.FlowMappingEnd)
            {
                PopState();
                tokenizer.Read();
                CurrentEventType = ParseEventType.MappingEnd;
                return;
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
                    tokenizer.Read();
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
                tokenizer.Read();
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

            tokenizer.Read();

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
                tokenizer.Read();
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
                tokenizer.Read();
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

        int RegisterAnchor(string anchorName)
        {
            var newId = ++lastAnchorId;
            anchors[anchorName] = newId; // TODO: Avoid `ToString`
            return newId;
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

