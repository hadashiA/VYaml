namespace VYaml.Serialization
{
    /// <summary>
    /// Specifies how keys of a mapping that do not match any member of a [YamlObject] type are handled during deserialization.
    /// </summary>
    public enum YamlUnmappedMemberHandling
    {
        /// <summary>
        /// Unmapped keys are silently skipped.
        /// </summary>
        Skip = 0,

        /// <summary>
        /// Unmapped keys throw <see cref="YamlSerializerException"/>.
        /// </summary>
        Disallow = 1,
    }
}
