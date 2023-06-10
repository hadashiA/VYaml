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
        };
    }

    public INamedTypeSymbol YamlObjectAttribute { get; private set; } = null!;
    public INamedTypeSymbol YamlMemberAttribute { get; private set; } = null!;
    public INamedTypeSymbol YamlIgnoreAttribute { get; private set; } = null!;
    public INamedTypeSymbol YamlConstructorAttribute { get; private set; } = null!;
    public INamedTypeSymbol YamlObjectUnionAttribute { get; private set; } = null!;
}
