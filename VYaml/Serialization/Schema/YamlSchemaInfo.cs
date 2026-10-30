using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using VYaml.Annotations;

namespace VYaml.Serialization
{
    /// <summary>
    /// Implemented by source-generated formatters so that <see cref="YamlSchemaGenerator"/> can describe
    /// a [YamlObject] type with exactly the same member selection / key naming as the formatter itself.
    /// </summary>
    public interface IYamlSchemaInfoProvider
    {
        YamlObjectSchemaInfo GetSchemaInfo();
    }

    public sealed class YamlObjectSchemaInfo
    {
        public Type Type { get; }
        public YamlMemberSchemaInfo[] Members { get; }
        public YamlUnionSchemaInfo[] Unions { get; }
        public string? Description { get; }

        /// <summary>
        /// <see cref="YamlObjectUnionAttribute.DiscriminatorKey"/>. null for unions by tag or by required key.
        /// </summary>
        public string? UnionDiscriminatorKeyName { get; }

        public bool IsUnion => Unions.Length > 0;

        public YamlObjectSchemaInfo(
            Type type,
            YamlMemberSchemaInfo[] members,
            YamlUnionSchemaInfo[] unions,
            string? description = null,
            string? unionDiscriminatorKeyName = null)
        {
            Type = type;
            Members = members;
            Unions = unions;
            Description = description;
            UnionDiscriminatorKeyName = unionDiscriminatorKeyName;
        }
    }

    public sealed class YamlMemberSchemaInfo
    {
        /// <summary>
        /// The C# member name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The key name. When <see cref="IsKeyNameFixed"/> is false, it is in lowerCamelCase and is mutated by
        /// <see cref="YamlSerializerOptions.NamingConvention"/> (same as the generated formatter).
        /// </summary>
        public string KeyName { get; }
        public bool IsKeyNameFixed { get; }
        public Type Type { get; }
        public bool IsNullable { get; }
        /// <summary>
        /// [YamlRequired] or the C# <c>required</c> modifier: deserialization fails when the key is missing.
        /// </summary>
        public bool IsRequired { get; }
        public bool HasDefaultValue { get; }
        public object? DefaultValue { get; }
        public string? Description { get; }

        public YamlMemberSchemaInfo(
            string name,
            string keyName,
            bool isKeyNameFixed,
            Type type,
            bool isNullable,
            bool isRequired = false,
            bool hasDefaultValue = false,
            object? defaultValue = null,
            string? description = null)
        {
            Name = name;
            KeyName = keyName;
            IsKeyNameFixed = isKeyNameFixed;
            Type = type;
            IsNullable = isNullable;
            IsRequired = isRequired;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
            Description = description;
        }
    }

    public sealed class YamlUnionSchemaInfo
    {
        /// <summary>
        /// The YAML tag, or the value of the discriminator key. Empty for unions by required key.
        /// </summary>
        public string Tag { get; }
        public Type SubType { get; }

        /// <summary>
        /// <see cref="YamlObjectUnionAttribute.RequiredKey"/>, resolved to <see cref="YamlMemberSchemaInfo.KeyName"/>
        /// of the subtype's member. null for unions by tag or by discriminator key.
        /// </summary>
        public string? RequiredKey { get; }

        public YamlUnionSchemaInfo(string tag, Type subType, string? requiredKey = null)
        {
            Tag = tag;
            SubType = subType;
            RequiredKey = requiredKey;
        }
    }

    public static class YamlSchemaInfoRegistry
    {
        static readonly ConcurrentDictionary<Type, IYamlSchemaInfoProvider> Providers = new();

        [Preserve]
        public static void Register(Type type, IYamlSchemaInfoProvider provider)
        {
            Providers[type] = provider;
        }

        public static bool TryGetSchemaInfo(Type type, [NotNullWhen(true)] out YamlObjectSchemaInfo? schemaInfo)
        {
            if (Providers.TryGetValue(type, out var provider) ||
                (TryInvokeRegisterYamlFormatter(type) && Providers.TryGetValue(type, out provider)))
            {
                schemaInfo = provider.GetSchemaInfo();
                return true;
            }
            schemaInfo = null;
            return false;
        }

        // Same fallback as GeneratedResolver: the module initializer of the declaring assembly may not
        // have run yet on JIT (it runs lazily), so trigger the generated registration via reflection.
        [UnconditionalSuppressMessage("Trimming", "IL2070",
            Justification = "Fallback only. Assemblies compiled with the VYaml source generator register their formatters from a module initializer, which statically roots the generated members.")]
        [UnconditionalSuppressMessage("Trimming", "IL2072",
            Justification = "Generated formatter types always have a public parameterless constructor and are rooted by the module-initializer registration.")]
        static bool TryInvokeRegisterYamlFormatter(Type type)
        {
            if (type.GetCustomAttribute<YamlObjectAttribute>() == null) return false;
            if (type.ContainsGenericParameters) return false;

            if (type.IsInterface)
            {
                var generatedFormatterType = type.GetNestedType($"{type.Name}GeneratedFormatter");
                if (generatedFormatterType == null) return false;

                if (Activator.CreateInstance(generatedFormatterType) is IYamlSchemaInfoProvider provider)
                {
                    Register(type, provider);
                    return true;
                }
                return false;
            }

            var m = type.GetMethod("__RegisterVYamlFormatter",
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static);
            if (m == null) return false;

            m.Invoke(null, null);
            return true;
        }
    }
}
