#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using VYaml.Annotations;

namespace VYaml.Serialization
{
    public class GeneratedResolver : IYamlFormatterResolver
    {
        static class Check<T>
        {
            internal static bool Registered;
        }

        static class Cache<T>
        {
            internal static IYamlFormatter<T>? Formatter;

            static Cache()
            {
                if (Check<T>.Registered) return;

                TryInvokeRegisterYamlFormatter<T>();
            }
        }

        // Reflection fallback for environments where the module-initializer registration has
        // not run yet: the JIT runs a module's initializer lazily (first method call or static
        // field access of that module), so a type whose first contact is a generic
        // Deserialize<T>() from another assembly may not be registered when Cache<T> warms up.
        // It also serves assemblies compiled with a pre-module-initializer source generator.
        // On Native AOT module initializers run eagerly before Main, so this path is never
        // required for assemblies compiled with the current generator.
        [UnconditionalSuppressMessage("Trimming", "IL2090",
            Justification = "Fallback only. Assemblies compiled with the VYaml source generator register their formatters from a module initializer, which statically roots the generated members; this reflection path is for JIT lazy module initialization and legacy generated code.")]
        [UnconditionalSuppressMessage("Trimming", "IL2075",
            Justification = "Fallback only. See IL2090 justification.")]
        [UnconditionalSuppressMessage("Trimming", "IL2072",
            Justification = "Generated formatter types always have a public parameterless constructor and are rooted by the module-initializer registration.")]
        static bool TryInvokeRegisterYamlFormatter<T>()
        {
            var type = typeof(T);
            if (type.GetCustomAttribute<YamlObjectAttribute>() == null) return false;

            if (type.IsInterface)
            {
                var generatedFormatterType = type.GetNestedType($"{type.Name}GeneratedFormatter");
                if (generatedFormatterType == null) return false;

                var formatterInstance = (IYamlFormatter<T>)Activator.CreateInstance(generatedFormatterType)!;
                Register(formatterInstance);

                return true;
            }

            var m = type.GetMethod("__RegisterVYamlFormatter",
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static);

            if (m == null) return false;

            m.Invoke(null, null); // Cache<T>.formatter will set from method
            return true;
        }

        [Preserve]
        public static void Register<T>(IYamlFormatter<T> formatter)
        {
            Check<T>.Registered = true; // avoid to call Cache() constructor called.
            Cache<T>.Formatter = formatter;
            if (formatter is IYamlSchemaInfoProvider schemaInfoProvider)
            {
                YamlSchemaInfoRegistry.Register(typeof(T), schemaInfoProvider);
            }
        }

        public static readonly GeneratedResolver Instance = new();

        public IYamlFormatter<T>? GetFormatter<T>() => Cache<T>.Formatter;
    }
}
