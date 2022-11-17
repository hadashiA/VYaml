using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using VYaml.Internal;

namespace VYaml
{
    class YamlTokenizerException : Exception
    {
        public YamlTokenizerException(in Marker marker, string message)
            : base($"{message} at {marker}")
        {
        }
    }

    struct SimpleKeyState
    {
        public bool Possible;
        public bool Required;
        public int TokenNumber;
        public Marker Start;
    }

    struct Token
    {
        public readonly TokenType Type;
        public readonly Marker Start;
        public Scalar? Scalar;
        public Tag? Tag;

        public Token(
            TokenType type,
            in Marker start,
            Scalar scalar = null,
            Tag tag = null)
        {
            Type = type;
            Start = start;
            Scalar = scalar;
            Tag = tag;
        }

        public Scalar TakeScalar()
        {
            var scalar = Scalar!;
            Scalar = null;
            return scalar;
        }

        public override string ToString() => $"{Type} \"{Scalar}\"";
    }

    public ref partial struct Utf8Tokenizer
    {
        public TokenType CurrentTokenType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => currentToken.Type;
        }

        public Marker CurrentMark
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => currentToken.Start;
        }

        SequenceReader<byte> reader;
        Marker mark;
        Token currentToken;

        bool streamStartProduced;
        bool streamEndProduced;
        int indent;
        bool simpleKeyAllowed;
        int adjacentValueAllowedAt;
        int flowLevel;
        int tokensParsed;
        bool tokenAvailable;

        readonly InsertionQueue<Token> tokens;
        readonly ExpandBuffer<SimpleKeyState> simpleKeyCandidates;
        readonly ExpandBuffer<int> indents;
        readonly ExpandBuffer<byte> whitespaces;
        readonly ScalarPool scalarPool;

        public Utf8Tokenizer(in ReadOnlySequence<byte> sequence)
        {
            if (!sequence.IsSingleSegment)
            {
                throw new NotSupportedException();
            }

            reader = new SequenceReader<byte>(sequence);
            mark = new Marker(0, 1, 0);
            tokens = new InsertionQueue<Token>(16);
            simpleKeyCandidates = new ExpandBuffer<SimpleKeyState>(16);
            indents = new ExpandBuffer<int>(16);
            whitespaces = new ExpandBuffer<byte>(256);
            scalarPool = new ScalarPool();

            indent = -1;
            flowLevel = 0;
            adjacentValueAllowedAt = 0;
            tokensParsed = 0;
            simpleKeyAllowed = false;
            streamStartProduced = false;
            streamEndProduced = false;
            tokenAvailable = false;

            currentToken = default;
        }

        public bool Read()
        {
            if (streamEndProduced)
            {
                return false;
            }

            if (!tokenAvailable)
            {
                ConsumeMoreTokens();
            }

            if (currentToken.Scalar is { } scalar)
            {
                scalarPool.Return(scalar);
            }
            currentToken = tokens.Dequeue();
            System.Console.WriteLine($"  TOKEN {currentToken}");
            tokenAvailable = false;
            tokensParsed += 1;

            if (currentToken.Type == TokenType.StreamEnd)
            {
                streamEndProduced = true;
            }
            return true;
        }

        internal Scalar TakeCurrentScalar() => currentToken.TakeScalar();
        internal void ReturnScalarToPool(Scalar scalar) => scalarPool.Return(scalar);

        void ConsumeMoreTokens()
        {
            while (true)
            {
                var needMore = tokens.Count <= 0;
                if (!needMore)
                {
                    StaleSimpleKeyCandidates();
                    var span = simpleKeyCandidates.AsSpan();
                    for (var i = 0; i < span.Length; i++)
                    {
                        ref var simpleKeyState = ref span[i];
                        if (simpleKeyState.Possible && simpleKeyState.TokenNumber == tokensParsed)
                        {
                            needMore = true;
                            break;
                        }
                    }
                }
                if (!needMore)
                {
                    break;
                }
                ConsumeNextToken();
            }
            tokenAvailable = true;
        }

        void ConsumeNextToken()
        {
            if (!streamStartProduced)
            {
                ConsumeStreamStart();
                return;
            }

            SkipToNextToken();
            StaleSimpleKeyCandidates();
            UnrollIndent(mark.Col);

            if (!reader.TryPeek(out var code))
            {
                ConsumeStreamEnd();
                return;
            }

            if (mark.Col == 0)
            {
                if (code == YamlCodes.DirectiveLine)
                {
                    ConsumeDirective();
                    return;
                }
                if (reader.IsNext(YamlCodes.StreamStart) && EmptyNext(YamlCodes.StreamStart.Length))
                {
                    ConsumeDocumentIndicator(TokenType.DocumentStart);
                    return;
                }
                if (reader.IsNext(YamlCodes.DocStart) && EmptyNext(YamlCodes.DocStart.Length))
                {
                    ConsumeDocumentIndicator(TokenType.DocumentEnd);
                    return;
                }
            }

            switch (code)
            {
                case YamlCodes.FlowSequenceStart:
                    ConsumeFlowCollectionStart(TokenType.FlowSequenceStart);
                    break;
                case YamlCodes.FlowMapStart:
                    ConsumeFlowCollectionStart(TokenType.FlowMappingStart);
                    break;
                case YamlCodes.FlowSequenceEnd:
                    ConsumeFlowCollectionEnd(TokenType.FlowSequenceEnd);
                    break;
                case YamlCodes.FlowMapEnd:
                    ConsumeFlowCollectionEnd(TokenType.FlowMappingEnd);
                    break;
                case YamlCodes.Comma:
                    ConsumeFlowEntryStart();
                    break;
                case YamlCodes.BlockEntryIndent when !reader.TryPeek(1L, out var nextCode) ||
                                                     YamlCodes.IsEmpty(nextCode):
                    ConsumeBlockEntry();
                    break;
                case YamlCodes.ExplicitKeyIndent when !reader.TryPeek(1L, out var nextCode) ||
                                                      YamlCodes.IsEmpty(nextCode):
                    ConsumeComplexKeyStart();
                    break;
                case YamlCodes.MapValueIndent
                    when !reader.TryPeek(1L, out var nextCode) ||
                         YamlCodes.IsEmpty(nextCode) ||
                        (flowLevel > 0 && (YamlCodes.IsAnyFlowSymbol(nextCode) ||
                                           mark.Position == adjacentValueAllowedAt)):
                    ConsumeValueStart();
                    break;
                case YamlCodes.Alias:
                    ConsumeAnchor(true);
                    break;
                case YamlCodes.Anchor:
                    ConsumeAnchor(false);
                    break;
                case YamlCodes.Tag:
                    ConsumeTag();
                    break;
                case YamlCodes.BlockScalerHeader1 when flowLevel == 0:
                    ConsumeBlockScaler(true);
                    break;
                case YamlCodes.BlockScalerHeader2 when flowLevel == 0:
                    ConsumeBlockScaler(false);
                    break;
                case YamlCodes.SingleQuote:
                    ConsumeFlowScaler(true);
                    break;
                case YamlCodes.DoubleQuote:
                    ConsumeFlowScaler(false);
                    break;
                // Plain Scaler
                case YamlCodes.BlockEntryIndent when !reader.TryPeek(1L, out var nextCode) ||
                                                     YamlCodes.IsBlank(nextCode):
                    ConsumePlainScaler();
                    break;
                case YamlCodes.MapValueIndent or YamlCodes.ExplicitKeyIndent
                    when flowLevel == 0 &&
                         (!reader.TryPeek(1L, out var nextCode) || YamlCodes.IsBlank(nextCode)):
                    ConsumePlainScaler();
                    break;
                case (byte)'%' or (byte)'@' or (byte)'`':
                    throw new YamlTokenizerException(in mark, $"Unexpected character: '{code}'");
                default:
                    ConsumePlainScaler();
                    break;
            }
        }

        void ConsumeStreamStart()
        {
            indent = -1;
            streamStartProduced = true;
            simpleKeyAllowed = true;
            tokens.Enqueue(new Token(TokenType.StreamStart, in mark));
            simpleKeyCandidates.Add(new SimpleKeyState());
        }

        void ConsumeStreamEnd()
        {
            // force new line
            if (mark.Col != 0)
            {
                mark.Col = 0;
                mark.Line += 1;
            }
            UnrollIndent(-1);
            RemoveSimpleKeyCandidate();
            simpleKeyAllowed = false;
            tokens.Enqueue(new Token(TokenType.StreamEnd, in mark));
        }

        void ConsumeDirective()
        {
            UnrollIndent(-1);
            RemoveSimpleKeyCandidate();
            simpleKeyAllowed = false;

            throw new NotImplementedException();
        }

        void ConsumeDocumentIndicator(TokenType tokenType)
        {
            UnrollIndent(-1);
            RemoveSimpleKeyCandidate();
            simpleKeyAllowed = false;
            Advance(3);
            tokens.Enqueue(new Token(tokenType, mark));
        }

        void ConsumeFlowCollectionStart(TokenType tokenType)
        {
            // The indicators '[' and '{' may start a simple key.
            SaveSimpleKeyCandidate();
            IncreaseFlowLevel();

            simpleKeyAllowed = true;

            Advance(1);
            tokens.Enqueue(new Token(tokenType, in mark));
        }

        void ConsumeFlowCollectionEnd(TokenType tokenType)
        {
            RemoveSimpleKeyCandidate();
            DecreaseFlowLevel();

            simpleKeyAllowed = false;

            Advance(1);
            tokens.Enqueue(new Token(tokenType, in mark));
        }

        void ConsumeFlowEntryStart()
        {
            RemoveSimpleKeyCandidate();
            simpleKeyAllowed = true;

            Advance(1);
            tokens.Enqueue(new Token(TokenType.FlowEntryStart, in mark));
        }

        void ConsumeBlockEntry()
        {
            if (flowLevel != 0)
            {
                throw new YamlTokenizerException(in mark, "'-' is only valid inside a block");
            }
            // Check if we are allowed to start a new entry.
            if (!simpleKeyAllowed)
            {
                throw new YamlTokenizerException(in mark, "Block sequence entries are not allowed in this context");
            }
            RollIndent(mark.Col, new Token(TokenType.BlockSequenceStart, in mark));
            RemoveSimpleKeyCandidate();
            simpleKeyAllowed = true;
            Advance(1);
            tokens.Enqueue(new Token(TokenType.BlockEntryStart, in mark));
        }

        void ConsumeComplexKeyStart()
        {
            if (flowLevel == 0)
            {
                // Check if we are allowed to start a new key (not necessarily simple).
                if (!simpleKeyAllowed)
                {
                    throw new YamlTokenizerException(in mark, "Mapping keys are not allowed in this context");
                }
                RollIndent(mark.Col, new Token(TokenType.BlockMappingStart, in mark));
            }
            RemoveSimpleKeyCandidate();

            simpleKeyAllowed = flowLevel == 0;
            Advance(1);
            tokens.Enqueue(new Token(TokenType.KeyStart, in mark));
        }

        void ConsumeValueStart()
        {
            ref var simpleKey = ref simpleKeyCandidates[^1];
            if (simpleKey.Possible)
            {
                // insert simple key
                var token = new Token(TokenType.KeyStart, simpleKey.Start);
                tokens.Insert(simpleKey.TokenNumber - tokensParsed, token);

                // Add the BLOCK-MAPPING-START token if needed
                RollIndent(simpleKey.Start.Col, new Token(TokenType.BlockMappingStart, in mark), simpleKey.TokenNumber);
                ref var lastKey = ref simpleKeyCandidates[^1];
                lastKey.Possible = false;
                simpleKeyAllowed = false;
            }
            else
            {
                // The ':' indicator follows a complex key.
                if (flowLevel == 0)
                {
                    if (!simpleKeyAllowed)
                    {
                        throw new YamlTokenizerException(in mark, "Mapping values are not allowed in this context");
                    }
                    RollIndent(mark.Col, new Token(TokenType.BlockMappingStart, in mark));
                }
                simpleKeyAllowed = flowLevel == 0;
            }
            Advance(1);
            tokens.Enqueue(new Token(TokenType.ValueStart, in mark));
        }

        void ConsumeAnchor(bool alias)
        {
            var scalar = scalarPool.Rent();
            Advance(1);

            while (reader.TryPeek(out var code) && YamlCodes.IsAlphaNumericDashOrUnderscore(code))
            {
                scalar.Write(code);
                Advance(1);
            }

            if (scalar.Length <= 0)
            {
                throw new YamlTokenizerException(mark,
                    "while scanning an anchor or alias, did not find expected alphabetic or numeric character");
            }
            var lastCode = scalar.AsSpan()[^1];
            if (!YamlCodes.IsBlank(lastCode) &&
                lastCode != '?' &&
                lastCode != ':' &&
                lastCode != ',' &&
                lastCode != ']' &&
                lastCode != '}' &&
                lastCode != '%' &&
                lastCode != '@' &&
                lastCode != '`')
            {
                throw new YamlTokenizerException(mark,
                    "while scanning an anchor or alias, did not find expected alphabetic or numeric character");
            }

            tokens.Enqueue(alias
                ? new Token(TokenType.Alias, mark, scalar)
                : new Token(TokenType.Anchor,mark, scalar));
        }

        void ConsumeTag()
        {
            throw new NotImplementedException();
            SaveSimpleKeyCandidate();
            simpleKeyAllowed = false;

            var tagHandle = scalarPool.Rent();
            var secondary = false;
            // let mut suffix;

            // Check if the tag is in the canonical form (verbatim).
            if (reader.TryPeek(1L, out var nextCode) && nextCode == '<')
            {
                // Eat '!<'
                Advance(2);
                // ConsumeTagUri(false, false);
            }
            else
            {

            }
            //     // The tag has either the '!suffix' or the '!handle!suffix'
            //     handle = self.scan_tag_handle(false, &start_mark)?;
            //     // Check if it is, indeed, handle.
            //     if handle.len() >= 2 && handle.starts_with('!') && handle.ends_with('!') {
            //         if handle == "!!" {
            //             secondary = true;
            //         }
            //         suffix = self.scan_tag_uri(false, secondary, &String::new(), &start_mark)?;
            //     } else {
            //         suffix = self.scan_tag_uri(false, false, &handle, &start_mark)?;
            //         handle = "!".to_owned();
            //         // A special case: the '!' tag.  Set the handle to '' and the
            //         // suffix to '!'.
            //         if suffix.is_empty() {
            //             handle.clear();
            //             suffix = "!".to_owned();
            //         }
            //     }
            // }
            //
            // self.lookahead(1);
            // if is_blankz(self.ch()) {
            //     // XXX: ex 7.2, an empty scalar can follow a secondary tag
            //     Ok(Token(start_mark, TokenType::Tag(handle, suffix)))
            // } else {
            //     Err(ScanError::new(
            //         start_mark,
            //         "while scanning a tag, did not find expected whitespace or line break",
            //     ))
            // }
        }

        void ConsumeBlockScaler(bool literal)
        {
            SaveSimpleKeyCandidate();
            simpleKeyAllowed = true;

            var startMark = mark;
            var chomping = 0;
            var increment = 0;
            var blockIndent = 0;

            var trailingBlank = false;
            var leadingBlank = false;

            var leadingBreak = LineBreakState.None;
            var trailingBreak = LineBreakState.None;

            var scalar = scalarPool.Rent();
            whitespaces.Clear();

            // skip '|' or '>'
            Advance(1);

            reader.TryPeek(out var code);
            if (code is (byte)'+' or (byte)'-')
            {
                chomping = code == (byte)'+' ? 1 : -1;
                Advance(1);
                if (reader.TryPeek(out code) && YamlCodes.IsNumber(code))
                {
                    if (code == (byte)'0')
                    {
                        throw new YamlTokenizerException(in mark,
                            "While scanning a block scalar, found an indentation indicator equal to 0");
                    }

                    increment = YamlCodes.AsHex(code);
                    Advance(1);
                }
            }
            else if (YamlCodes.IsNumber(code))
            {
                if (code == (byte)'0')
                {
                    throw new YamlTokenizerException(in mark,
                        "While scanning a block scalar, found an indentation indicator equal to 0");
                }
                increment = YamlCodes.AsHex(code);
                Advance(1);

                if (reader.TryPeek(out code) && code is (byte)'+' or (byte)'-')
                {
                    chomping = code == (byte)'+' ? 1 : -1;
                    Advance(1);
                }
            }

            // Eat whitespaces and comments to the end of the line.
            while (reader.TryPeek(out code) && YamlCodes.IsBlank(code))
            {
                Advance(1);
            }

            if (reader.TryPeek(out code) && code == YamlCodes.Comment)
            {
                while (reader.TryPeek(out code) && !YamlCodes.IsLineBreak(code))
                {
                    Advance(1);
                }
            }

            // Check if we are at the end of the line.
            if (!reader.TryPeek(out code) && !YamlCodes.IsLineBreak(code))
            {
                throw new YamlTokenizerException(in mark,
                    "While scanning a block scalar, did not find expected commnet or line break");
            }

            if (YamlCodes.IsLineBreak(code))
            {
                ConsumeLineBreaks();
            }

            if (increment > 0)
            {
                blockIndent = indent >= 0 ? indent + increment : increment;
            }

            // Scan the leading line breaks and determine the indentation level if needed.
            ConsumeBlockScalarBreaks(ref blockIndent, ref trailingBreak);

            while (mark.Col == blockIndent && reader.TryPeek(out code))
            {
                // We are at the beginning of a non-empty line.
                trailingBlank = YamlCodes.IsBlank(code);
                if (!literal &&
                    leadingBreak != LineBreakState.None &&
                    !leadingBlank &&
                    !trailingBlank)
                {
                    if (trailingBreak == LineBreakState.None)
                    {
                        scalar.Write(YamlCodes.Space);
                    }
                }
                else
                {
                    scalar.Write(leadingBreak);
                }

                scalar.Write(trailingBreak);
                leadingBreak = LineBreakState.None;
                trailingBreak = LineBreakState.None;
                leadingBlank = YamlCodes.IsBlank(code);

                while (reader.TryPeek(out code) && !YamlCodes.IsLineBreak(code))
                {
                    scalar.Write(code);
                    Advance(1);
                }
                // break on EOF
                if (reader.End) break;

                leadingBreak = ConsumeLineBreaks();
                // Eat the following indentation spaces and line breaks.
                ConsumeBlockScalarBreaks(ref blockIndent, ref trailingBreak);
            }

            // Chomp the tail.
            if (chomping != -1)
            {
                scalar.Write(leadingBreak);
            }
            if (chomping == 1)
            {
                scalar.Write(trailingBreak);
            }

            var tokenType = literal ? TokenType.LiteralScalar : TokenType.FoldedScalar;
            tokens.Enqueue(new Token(tokenType, startMark, scalar));
        }

        void ConsumeBlockScalarBreaks(ref int blockIndent, ref LineBreakState lineBreak)
        {
            var maxIndent = 0;
            while (true)
            {
                byte code;
                while (reader.TryPeek(out code) &&
                       (blockIndent == 0 || mark.Col < blockIndent) &&
                       code == YamlCodes.Space)
                {
                    Advance(1);
                }

                if (mark.Col > maxIndent)
                {
                    maxIndent = mark.Col;
                }

                // Check for a tab character messing the indentation.
                if ((blockIndent == 0 || mark.Col < blockIndent) && code == YamlCodes.Tab)
                {
                    throw new YamlTokenizerException(in mark,
                        "while scanning a block scalar, found a tab character where an indentation space is expected");
                }

                if (!YamlCodes.IsLineBreak(code))
                {
                    break;
                }
                lineBreak = ConsumeLineBreaks();
            }

            if (blockIndent == 0)
            {
                blockIndent = maxIndent;
                if (blockIndent < indent + 1)
                {
                    blockIndent = indent + 1;
                }
                else if (blockIndent < 1)
                {
                    blockIndent = 1;
                }
            }
        }

        void ConsumeFlowScaler(bool singleQuote)
        {
            SaveSimpleKeyCandidate();
            simpleKeyAllowed = false;

            var startMark = mark;
            var leadingBreak = default(LineBreakState);
            var trailingBreak = default(LineBreakState);
            var isLeadingBlanks = false;
            var scalar = scalarPool.Rent();
            whitespaces.Clear();

            // Eat the left quote
            Advance(1);

            while (true)
            {
                if (mark.Col == 0 &&
                    (reader.IsNext(YamlCodes.StreamStart) ||
                     reader.IsNext(YamlCodes.DocStart)) &&
                    !reader.TryPeek(3L, out _))
                {
                    throw new YamlTokenizerException(mark,
                        "while scanning a quoted scalar, found unexpected document indicator");
                }

                if (reader.End)
                {
                    throw new YamlTokenizerException(mark,
                        "while scanning a quoted scalar, found unexpected end of stream");
                }

                isLeadingBlanks = false;

                // Consume non-blank characters
                byte code;
                while (reader.TryPeek(out code) && !YamlCodes.IsBlank(code) && !YamlCodes.IsLineBreak(code))
                {
                    reader.TryPeek(1L, out var nextCode);
                    switch (code)
                    {
                        // Check for an escaped single quote
                        case YamlCodes.SingleQuote when nextCode == YamlCodes.SingleQuote && singleQuote:
                            scalar.Write((byte)'\'');
                            Advance(2);
                            break;
                        // Check for the right quote.
                        case YamlCodes.SingleQuote when singleQuote:
                        case YamlCodes.DoubleQuote when !singleQuote:
                            goto LOOPEND;
                        // Check for an escaped line break.
                        case (byte)'\\' when !singleQuote && YamlCodes.IsLineBreak(nextCode):
                            Advance(1);
                            ConsumeLineBreaks();
                            isLeadingBlanks = true;
                            break;
                        // Check for an escape sequence.
                        case (byte)'\\' when !singleQuote:
                            var codeLength = 0;
                            switch (nextCode)
                            {
                                case (byte)'0':
                                    scalar.Write((byte)'\0');
                                    break;
                                case (byte)'a':
                                    scalar.Write((byte)'\a');
                                    break;
                                case (byte)'b':
                                    scalar.Write((byte)'\b');
                                    break;
                                case (byte)'t':
                                    scalar.Write((byte)'\t');
                                    break;
                                case (byte)'n':
                                    scalar.Write((byte)'\n');
                                    break;
                                case (byte)'v':
                                    scalar.Write((byte)'\v');
                                    break;
                                case (byte)'f':
                                    scalar.Write((byte)'\f');
                                    break;
                                case (byte)'r':
                                    scalar.Write((byte)'\r');
                                    break;
                                case (byte)'e':
                                    scalar.Write(0x1b);
                                    break;
                                case (byte)' ':
                                    scalar.Write((byte)' ');
                                    break;
                                case (byte)'"':
                                    scalar.Write((byte)'"');
                                    break;
                                case (byte)'\'':
                                    scalar.Write((byte)'\'');
                                    break;
                                case (byte)'\\':
                                    scalar.Write((byte)'\\');
                                    break;
                                // NEL (#x85)
                                case (byte)'N':
                                    scalar.WriteUnicodeCodepoint(0x85);
                                    break;
                                // #xA0
                                case (byte)'_':
                                    scalar.WriteUnicodeCodepoint(0xA0);
                                    break;
                                // LS (#x2028)
                                case (byte)'L':
                                    scalar.WriteUnicodeCodepoint(0x2028);
                                    break;
                                // PS (#x2029)
                                case (byte)'P':
                                    scalar.WriteUnicodeCodepoint(0x2029);
                                    break;
                                case (byte)'x':
                                    codeLength = 2;
                                    break;
                                case (byte)'u':
                                    codeLength = 4;
                                    break;
                                case (byte)'U':
                                    codeLength = 8;
                                    break;
                                default:
                                    throw new YamlTokenizerException(mark,
                                        "while parsing a quoted scalar, found unknown escape character");
                            }

                            Advance(2);
                            // Consume an arbitrary escape code.
                            if (codeLength > 0)
                            {
                                var codepoint = 0;
                                for (var i = 0; i < codeLength; i++)
                                {
                                    if (reader.TryPeek(i, out var hex) && YamlCodes.IsHex(hex))
                                    {
                                        codepoint = (codepoint << 4) + YamlCodes.AsHex(hex);
                                    }
                                    else
                                    {
                                        throw new YamlTokenizerException(mark,
                                            "While parsing a quoted scalar, did not find expected hexadecimal number");
                                    }
                                }
                                scalar.WriteUnicodeCodepoint(codepoint);
                            }

                            Advance(codeLength);
                            break;
                        default:
                            scalar.Write(code);
                            Advance(1);
                            break;
                    }
                }

                // Consume blank characters.
                while (reader.TryPeek(out code) &&
                       (YamlCodes.IsBlank(code) || YamlCodes.IsLineBreak(code)))
                {
                    if (YamlCodes.IsBlank(code))
                    {
                        // Consume a space or a tab character.
                        if (!isLeadingBlanks)
                        {
                            whitespaces.Add(code);
                        }
                        Advance(1);
                    }
                    else
                    {
                        // Check if it is a first line break.
                        if (isLeadingBlanks)
                        {
                            trailingBreak = ConsumeLineBreaks();
                        }
                        else
                        {
                            whitespaces.Clear();
                            leadingBreak = ConsumeLineBreaks();
                            isLeadingBlanks = true;
                        }
                    }
                }

                // Join the whitespaces or fold line breaks.
                if (isLeadingBlanks)
                {
                    if (leadingBreak == LineBreakState.None)
                    {
                        scalar.Write(trailingBreak);
                        trailingBreak = LineBreakState.None;
                    }
                    else
                    {
                        if (trailingBreak == LineBreakState.None)
                        {
                            scalar.Write(YamlCodes.Space);
                        }
                        else
                        {
                            scalar.Write(trailingBreak);
                            trailingBreak = LineBreakState.None;
                        }
                        leadingBreak = LineBreakState.None;
                    }
                }
                else
                {
                    scalar.Write(whitespaces.AsSpan());
                    whitespaces.Clear();
                }
            }

            // Eat the right quote
            LOOPEND:
            Advance(1);
            simpleKeyAllowed = isLeadingBlanks;

            // From spec: To ensure JSON compatibility, if a key inside a flow mapping is JSON-like,
            // YAML allows the following value to be specified adjacent to the “:”.
            adjacentValueAllowedAt = mark.Position;

            tokens.Enqueue(new Token(singleQuote
                ? TokenType.SingleQuotedScaler
                : TokenType.DoubleQuotedScaler,
                startMark,
                scalar));
        }

        void ConsumePlainScaler()
        {
            SaveSimpleKeyCandidate();
            simpleKeyAllowed = false;

            var startMark = mark;
            var currentIndent = indent + 1;
            var leadingBreak = default(LineBreakState);
            var trailingBreak = default(LineBreakState);
            var isLeadingBlanks = false;
            var scalar = scalarPool.Rent();

            whitespaces.Clear();

            while (true)
            {
                // Check for a document indicator
                if (mark.Col == 0 &&
                    (reader.IsNext(YamlCodes.StreamStart) || reader.IsNext(YamlCodes.DocStart)) &&
                    EmptyNext(3))
                {
                    break;
                }

                if (reader.TryPeek(out var code) && code == YamlCodes.Comment)
                {
                    break;
                }

                while (reader.TryPeek(out code) && !YamlCodes.IsEmpty(code))
                {
                    if (code == YamlCodes.MapValueIndent)
                    {
                        var hasNext = reader.TryPeek(1, out var nextCode);
                        if (!hasNext ||
                            YamlCodes.IsEmpty(nextCode) ||
                            (flowLevel > 0 && YamlCodes.IsAnyFlowSymbol(nextCode)))
                        {
                            break;
                        }
                    }
                    else if (YamlCodes.IsAnyFlowSymbol(code) && flowLevel > 0)
                    {
                        break;
                    }

                    if (isLeadingBlanks || whitespaces.Length > 0)
                    {
                        if (isLeadingBlanks)
                        {
                            if (leadingBreak == LineBreakState.None)
                            {
                                scalar.Write(trailingBreak);
                                trailingBreak = LineBreakState.None;
                            }
                            else
                            {
                                if (trailingBreak == LineBreakState.None)
                                {
                                    scalar.Write(YamlCodes.Space);
                                }
                                else
                                {
                                    scalar.Write(trailingBreak);
                                    trailingBreak = LineBreakState.None;
                                }
                                leadingBreak = LineBreakState.None;
                            }
                            isLeadingBlanks = false;
                        }
                        else
                        {
                            scalar.Write(whitespaces.AsSpan());
                            whitespaces.Clear();
                        }
                    }

                    scalar.Write(code);
                    Advance(1);
                }

                // is the end?
                if (!YamlCodes.IsEmpty(code))
                {
                    break;
                }

                // whitespaces or line-breaks
                while (reader.TryPeek(out code) && YamlCodes.IsEmpty(code))
                {
                    // whitespaces
                    if (YamlCodes.IsBlank(code))
                    {
                        if (isLeadingBlanks && mark.Col < currentIndent && code == YamlCodes.Tab)
                        {
                            throw new YamlTokenizerException(mark, "While scanning a plain scaler, found a tab");
                        }
                        if (!isLeadingBlanks)
                            whitespaces.Add(code);
                        Advance(1);
                    }
                    // line-break
                    else
                    {
                        // Check if it is a first line break
                        if (isLeadingBlanks)
                        {
                            trailingBreak = ConsumeLineBreaks();
                        }
                        else
                        {
                            leadingBreak = ConsumeLineBreaks();
                            isLeadingBlanks = true;
                            whitespaces.Clear();
                        }
                    }
                }

                // check indentation level
                if (flowLevel == 0 && mark.Col < currentIndent)
                {
                    break;
                }
            }

            simpleKeyAllowed = isLeadingBlanks;
            tokens.Enqueue(new Token(TokenType.PlainScalar, startMark, scalar));
        }

        void SkipToNextToken()
        {
            while (reader.TryPeek(out var code))
            {
                switch (code)
                {
                    case YamlCodes.Space:
                        Advance(1);
                        break;
                    case YamlCodes.Tab when flowLevel > 0 || !simpleKeyAllowed:
                        Advance(1);
                        break;
                    case YamlCodes.Cr:
                    case YamlCodes.Lf:
                        ConsumeLineBreaks();
                        if (flowLevel == 0) simpleKeyAllowed = true;
                        break;
                    case YamlCodes.Comment:
                        while (!reader.End && !YamlCodes.IsLineBreak(code))
                        {
                            Advance(1);
                            reader.TryPeek(out code);
                        }
                        break;
                    case 0xFE when reader.IsNext(YamlCodes.Bom):
                        Advance(YamlCodes.Bom.Length);
                        break;
                    default:
                        return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Advance(int offset)
        {
            for (var i = 0; i < offset; i++)
            {
                EnsureMoreData(reader.TryRead(out var code));
                mark.Position += 1;
                if (code == YamlCodes.Lf)
                {
                    mark.Line += 1;
                    mark.Col = 0;
                }
                else
                {
                    mark.Col += 1;
                }
            }
        }

        LineBreakState ConsumeLineBreaks()
        {
            if (!reader.TryPeek(out var code))
                return LineBreakState.None;

            switch (code)
            {
                case YamlCodes.Cr:
                    if (reader.TryPeek(out var secondCode) && secondCode == YamlCodes.Lf)
                    {
                        Advance(2);
                        return LineBreakState.CrLf;
                    }
                    Advance(1);
                    return LineBreakState.Cr;
                case YamlCodes.Lf:
                    Advance(1);
                    return LineBreakState.Lf;
            }
            return LineBreakState.None;
        }

        void StaleSimpleKeyCandidates()
        {
            for (var i = 0; i < simpleKeyCandidates.Length; i++)
            {
                ref var simpleKey = ref simpleKeyCandidates[i];
                if (simpleKey.Possible &&
                    (simpleKey.Start.Line < mark.Line || simpleKey.Start.Position + 1024 < mark.Position))
                {
                    if (simpleKey.Required)
                    {
                        throw new YamlTokenizerException(mark, "Simple key expect ':'");
                    }
                    simpleKey.Possible = false;
                }
            }
        }

        void SaveSimpleKeyCandidate()
        {
            if (!simpleKeyAllowed)
            {
                return;
            }

            ref var last = ref simpleKeyCandidates[^1];
            if (last.Possible && last.Required)
            {
                throw new YamlTokenizerException(mark, "Simple key expected");
            }

            simpleKeyCandidates[^1] = new SimpleKeyState
            {
                Start = mark,
                Possible = true,
                Required = flowLevel > 0 && indent == mark.Col,
                TokenNumber = tokensParsed + tokens.Count
            };
        }

        void RemoveSimpleKeyCandidate()
        {
            ref var last = ref simpleKeyCandidates[^1];
            if (last.Possible && last.Required)
            {
                throw new YamlTokenizerException(mark, "Simple key expected");
            }
            last.Possible = false;
        }

        void RollIndent(int colTo, in Token nextToken, int insertNumber = -1)
        {
            if (flowLevel > 0 || indent >= colTo)
            {
                return;
            }

            indents.Add(indent);
            indent = colTo;
            if (insertNumber >= 0)
            {
                tokens.Insert(insertNumber - tokensParsed, nextToken);
            }
            else
            {
                tokens.Enqueue(nextToken);
            }
        }

        void UnrollIndent(int col)
        {
            if (flowLevel > 0)
            {
                return;
            }
            while (indent > col)
            {
                tokens.Enqueue(new Token(TokenType.BlockEnd, mark));
                indent = indents.Pop();
            }
        }

        void IncreaseFlowLevel()
        {
            simpleKeyCandidates.Add(new SimpleKeyState());
            flowLevel++;
        }

        void DecreaseFlowLevel()
        {
            if (flowLevel <= 0) return;
            flowLevel--;
            simpleKeyCandidates.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool EmptyNext(int offset)
        {
            var exits = reader.TryPeek(offset, out var code);
            return !exits || YamlCodes.IsEmpty(code);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void EnsureMoreData(bool condition)
        {
            if (!condition)
            {
                throw new EndOfStreamException();
            }
        }
    }
}
