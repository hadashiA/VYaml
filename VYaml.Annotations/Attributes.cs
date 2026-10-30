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
        public string Tag { get; }
        public Type SubType { get; }

        public YamlObjectUnionAttribute(string tagString, Type subType)
        {
            Tag = tagString;
            SubType = subType;
        }
    }

    /// <summary>
    /// Makes a union ([YamlObjectUnion]) identify the subtype by a mapping key instead of a YAML tag.
    /// The tag string of each [YamlObjectUnion] is used as the value of the key.
    /// </summary>
    /// <example>
    /// <code>
    /// [YamlObject]
    /// [YamlObjectUnionDiscriminator("type")]
    /// [YamlObjectUnion("circle", typeof(Circle))]
    /// public partial interface IShape { }
    ///
    /// // type: circle
    /// // radius: 1.0
    /// </code>
    /// </example>
    /// <remarks>
    /// The discriminator key can be anywhere in the mapping, but deserialization is faster when it is the first key
    /// (it is always written first).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface |
                    AttributeTargets.Class,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class YamlObjectUnionDiscriminatorAttribute : Attribute
    {
        public string KeyName { get; }

        public YamlObjectUnionDiscriminatorAttribute(string keyName)
        {
            KeyName = keyName;
        }
    }

    /// <summary>
    /// Declares a subtype of a union identified by the presence of a key in the mapping, like the steps of
    /// GitHub Actions workflows (a step with <c>uses:</c> or a step with <c>run:</c>).
    /// </summary>
    /// <remarks>
    /// <para>The key is a member of the subtype, specified by its key name or its C# member name, and follows the
    /// same naming rules as the member. When a mapping contains the keys of several subtypes, the subtype declared
    /// first wins.</para>
    /// <para>Cannot be combined with [YamlObjectUnion].</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [YamlObject]
    /// [YamlObjectUnionKey("uses", typeof(ActionStep))]
    /// [YamlObjectUnionKey("run", typeof(RunStep))]
    /// public partial interface IStep { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Interface |
                    AttributeTargets.Class,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class YamlObjectUnionKeyAttribute : Attribute
    {
        public string KeyName { get; }
        public Type SubType { get; }

        public YamlObjectUnionKeyAttribute(string keyName, Type subType)
        {
            KeyName = keyName;
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
