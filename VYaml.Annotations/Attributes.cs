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
        /// The YAML tag of the subtype (e.g. <c>!circle</c>), or the value of <see cref="DiscriminatorKey"/>.
        /// Empty for <see cref="RequiredKey"/>.
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
        /// Identifies the subtype by the presence of this key, like the steps of GitHub Actions workflows (a step with
        /// <c>uses:</c> or a step with <c>run:</c>). Use the constructor without a tag.
        /// When used, every [YamlObjectUnion] of the type must have a RequiredKey.
        /// </summary>
        /// <example>
        /// <code>
        /// [YamlObjectUnion(typeof(ActionStep), RequiredKey = "uses")]
        /// [YamlObjectUnion(typeof(RunStep), RequiredKey = "run")]
        /// public partial interface IStep { }
        /// </code>
        /// </example>
        /// <remarks>
        /// The key is a member of the subtype, specified by its key name or its C# member name, and follows the same
        /// naming rules as the member. When a mapping contains the keys of several subtypes, the subtype declared
        /// first wins.
        /// </remarks>
        public string? RequiredKey { get; set; }

        public YamlObjectUnionAttribute(string tagString, Type subType)
        {
            Tag = tagString;
            SubType = subType;
        }

        /// <summary>
        /// For <see cref="RequiredKey"/>.
        /// </summary>
        public YamlObjectUnionAttribute(Type subType)
        {
            Tag = "";
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
