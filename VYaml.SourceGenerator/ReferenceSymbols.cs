using Microsoft.CodeAnalysis;

namespace VYaml.SourceGenerator;

public class ReferenceSymbols
{
    public static ReferenceSymbols? Create(Compilation compilation)
    {
        var yamlObjectAttribute = compilation.GetTypeByMetadataName("VYaml.Annotations.YamlObjectAttribute");
        if (yamlObjectAttribute is null)
            return null;

        return new ReferenceSymbols
        {
            YamlObjectAttribute = yamlObjectAttribute,
            YamlMemberAttribute = compilation.GetTypeByMetadataName("VYaml.Annotations.YamlMemberAttribute")!,
            YamlIgnoreAttribute = compilation.GetTypeByMetadataName("VYaml.Annotations.YamlIgnoreAttribute")!,
            YamlConstructorAttribute = compilation.GetTypeByMetadataName("VYaml.Annotations.YamlConstructorAttribute")!,
            YamlObjectUnionAttribute = compilation.GetTypeByMetadataName("VYaml.Annotations.YamlObjectUnionAttribute")!,
            NamingConventionEnum = compilation.GetTypeByMetadataName("VYaml.Annotations.NamingConvention")!
        };
    }

    public INamedTypeSymbol YamlObjectAttribute { get; private set; } = default!;
    public INamedTypeSymbol YamlMemberAttribute { get; private set; } = default!;
    public INamedTypeSymbol YamlIgnoreAttribute { get; private set; } = default!;
    public INamedTypeSymbol YamlConstructorAttribute { get; private set; } = default!;
    public INamedTypeSymbol YamlObjectUnionAttribute { get; private set; } = default!;
    public INamedTypeSymbol NamingConventionEnum { get; private set; } = default!;
}
