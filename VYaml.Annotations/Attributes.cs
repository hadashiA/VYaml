using System;

namespace VYaml.Annotations
{
    public enum NamingConvention
    {
        LowerCamelCase,
        UpperCamelCase,
        SnakeCase,
        KebabCase,
    }

    /// <summary>
    /// How a union ([YamlObjectUnion]) identifies the subtype in YAML.
    /// </summary>
    public enum YamlUnionStyle
    {
        /// <summary>
        /// A YAML tag (<c>!circle { radius: 1 }</c>). The default.
        /// </summary>
        Tag,

        /// <summary>
        /// The value of <see cref="YamlObjectUnionAttribute.DiscriminatorKey"/> (<c>{ type: circle, radius: 1 }</c>).
        /// Implied by setting DiscriminatorKey.
        /// </summary>
        DiscriminatorKey,

        /// <summary>
        /// A mapping with exactly one key, the tag string, whose value is the subtype (<c>{ circle: { radius: 1 } }</c>).
        /// </summary>
        Wrapped,
    }

    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct |
                    AttributeTargets.Interface |
                    AttributeTargets.Enum,
        Inherited = false)]
    public class YamlObjectAttribute : Attribute
    {
        public NamingConvention NamingConvention { get; }

        public YamlObjectAttribute(NamingConvention namingConvention = NamingConvention.LowerCamelCase)
        {
            NamingConvention = namingConvention;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class YamlMemberAttribute : Attribute
    {
        public string? Name { get; }
        public int Order { get; set; }

        public YamlMemberAttribute(string? name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class YamlIgnoreAttribute : Attribute
    {
    }

    /// <summary>
    /// Makes the key of the member required when deserializing: a missing key throws YamlSerializerException.
    /// Members with the C# <c>required</c> modifier are also required.
    /// </summary>
    /// <remarks>
    /// A key with a null value (<c>key: ~</c>) satisfies it. Required members are always serialized regardless of
    /// YamlSerializerOptions.DefaultIgnoreCondition.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class YamlRequiredAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class YamlConstructorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Interface |
                    AttributeTargets.Class,
        AllowMultiple = true,
        Inherited = false)]
    public class YamlObjectUnionAttribute : Attribute
    {
        /// <summary>
        /// The YAML tag of the subtype (e.g. <c>!circle</c>), the value of <see cref="DiscriminatorKey"/>, or the key
        /// of <see cref="YamlUnionStyle.Wrapped"/>.
        /// </summary>
        public string Tag { get; }
        public Type SubType { get; }

        /// <summary>
        /// Identifies the subtype by the value of this key instead of a YAML tag: <see cref="Tag"/> is used as the value.
        /// When used, every [YamlObjectUnion] of the type must have the same DiscriminatorKey.
        /// </summary>
        /// <example>
        /// <code>
        /// [YamlObjectUnion("circle", typeof(Circle), DiscriminatorKey = "type")]
        /// [YamlObjectUnion("rect", typeof(Rect), DiscriminatorKey = "type")]
        /// public partial interface IShape { }
        ///
        /// // type: circle
        /// // radius: 1.0
        /// </code>
        /// </example>
        /// <remarks>
        /// The key is always written first. It can be anywhere in the mapping when deserializing, but it is faster as
        /// the first key.
        /// </remarks>
        public string? DiscriminatorKey { get; set; }

        /// <summary>
        /// How the subtype is identified. <see cref="YamlUnionStyle.DiscriminatorKey"/> is implied by setting
        /// <see cref="DiscriminatorKey"/>, so this is mainly for <see cref="YamlUnionStyle.Wrapped"/>: the mapping has
        /// exactly one key, the tag string, and its value is the subtype. An empty value (<c>meditate:</c>) is an
        /// empty mapping.
        /// When used, every [YamlObjectUnion] of the type must have the same Style.
        /// </summary>
        /// <example>
        /// <code>
        /// [YamlObjectUnion("violence", typeof(ViolenceSkill), Style = YamlUnionStyle.Wrapped)]
        /// [YamlObjectUnion("meditate", typeof(MeditateSkill), Style = YamlUnionStyle.Wrapped)]
        /// public partial interface ISkill { }
        ///
        /// // - violence: { name: Punch, power: 1 }
        /// // - meditate: {}
        /// </code>
        /// </example>
        public YamlUnionStyle Style { get; set; }

        public YamlObjectUnionAttribute(string tagString, Type subType)
        {
            Tag = tagString;
            SubType = subType;
        }
    }

    /// <summary>
    /// Declares a serialization root for ahead-of-time environments (Native AOT / trimming).
    ///
    /// The formatters for types that appear as members of a [YamlObject] are registered
    /// automatically at module initialization. A shape that only ever appears as a top-level
    /// argument of YamlSerializer.Serialize/Deserialize (e.g. List&lt;MyObject&gt;,
    /// Dictionary&lt;string, MyEnum&gt;, MyGeneric&lt;int&gt;) is invisible to that analysis;
    /// declaring it here makes the source generator statically construct and register the
    /// formatters for the whole reachable closure of the type.
    /// </summary>
    /// <remarks>
    /// The declared type must be closed (no open generics / type parameters).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class YamlSerializableAttribute : Attribute
    {
        public Type Type { get; }

        public YamlSerializableAttribute(Type type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// Preserve for Unity IL2CPP(internal but used for code generator)
    /// </summary>
    /// <remarks>
    /// > For 3rd party libraries that do not want to take on a dependency on UnityEngine.dll, it is also possible to define their own PreserveAttribute. The code stripper will respect that too, and it will consider any attribute with the exact name "PreserveAtribute" as a reason not to strip the thing it is applied on, regardless of the namespace or assembly of the attribute.
    /// </remarks>
    public sealed class PreserveAttribute : Attribute
    {
    }
}
