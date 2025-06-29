namespace VYaml.Serialization
{
    /// <summary>
    /// Specifies the condition under which properties are ignored during serialization.
    /// </summary>
    public enum YamlIgnoreCondition
    {
        /// <summary>
        /// Property is never ignored during serialization.
        /// </summary>
        Never = 0,

        /// <summary>
        /// Property is ignored when its value is null.
        /// </summary>
        WhenWritingNull = 1,

        /// <summary>
        /// Property is ignored when its value is the default for its type.
        /// For reference types and nullable value types, the default is null.
        /// For non-nullable value types, the default is the natural default (0 for numeric types, false for bool, etc.).
        /// </summary>
        WhenWritingDefault = 2,
    }
}