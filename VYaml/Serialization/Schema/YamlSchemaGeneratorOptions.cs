using System;
using System.Collections.Generic;

namespace VYaml.Serialization
{
    /// <summary>
    /// Options of <see cref="YamlSchemaGenerator"/>.
    /// </summary>
    public sealed class YamlSchemaGeneratorOptions
    {
        /// <summary>
        /// Whether [YamlObject] types allow keys other than their members (JSON Schema <c>additionalProperties</c>).
        /// When null, follows <see cref="YamlSerializerOptions.UnmappedMemberHandling"/>: allowed unless it is
        /// <see cref="YamlUnmappedMemberHandling.Disallow"/>.
        /// </summary>
        public bool? AdditionalProperties { get; set; }

        readonly Dictionary<Type, string> typeSchemas = new();
        readonly Dictionary<(Type, string), string> memberSchemas = new();

        /// <summary>
        /// Uses <paramref name="jsonSchema"/> for <paramref name="type"/>, e.g. a type serialized by a custom formatter.
        /// </summary>
        /// <param name="jsonSchema">A JSON Schema as a JSON string. It is embedded as is.</param>
        public YamlSchemaGeneratorOptions AddTypeSchema(Type type, string jsonSchema)
        {
            typeSchemas[type] = jsonSchema;
            return this;
        }

        /// <inheritdoc cref="AddTypeSchema(Type, string)"/>
        public YamlSchemaGeneratorOptions AddTypeSchema<T>(string jsonSchema) => AddTypeSchema(typeof(T), jsonSchema);

        /// <summary>
        /// Uses <paramref name="jsonSchema"/> for the member <paramref name="memberName"/> of the [YamlObject] type <paramref name="type"/>.
        /// </summary>
        /// <param name="memberName">The C# name of the member, e.g. <c>nameof(Card.Rarity)</c>.</param>
        /// <param name="jsonSchema">A JSON Schema as a JSON string. It is embedded as is.</param>
        public YamlSchemaGeneratorOptions AddMemberSchema(Type type, string memberName, string jsonSchema)
        {
            memberSchemas[(type, memberName)] = jsonSchema;
            return this;
        }

        /// <inheritdoc cref="AddMemberSchema(Type, string, string)"/>
        public YamlSchemaGeneratorOptions AddMemberSchema<T>(string memberName, string jsonSchema) => AddMemberSchema(typeof(T), memberName, jsonSchema);

        internal bool TryGetTypeSchema(Type type, out string jsonSchema) => typeSchemas.TryGetValue(type, out jsonSchema!);

        internal bool TryGetMemberSchema(Type type, string memberName, out string jsonSchema) =>
            memberSchemas.TryGetValue((type, memberName), out jsonSchema!);
    }
}
