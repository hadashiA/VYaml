namespace VYaml.Parser
{
    public enum TokenType : byte
    {
        None,
        StreamStart, //(TEncoding),
        StreamEnd,
        /// major, minor
        VersionDirective, //(u32, u32),
        /// handle, prefix
        TagDirective, //(String, String),
        DocumentStart,
        DocumentEnd,
        BlockSequenceStart,
        BlockMappingStart,
        BlockEnd,
        FlowSequenceStart,
        FlowSequenceEnd,
        FlowMappingStart,
        FlowMappingEnd,
        BlockEntryStart,
        FlowEntryStart,
        KeyStart,
        ValueStart,
        Alias,
        Anchor,
        Tag,
        PlainScalar,
        SingleQuotedScaler,
        DoubleQuotedScaler,
        LiteralScalar,
        FoldedScalar,
    }
}
