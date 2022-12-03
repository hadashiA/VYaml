using Microsoft.CodeAnalysis;

namespace VYaml.SourceGenerator;

public class ReferenceSymbols
{
    public static ReferenceSymbols? Create(Compilation compilation)
    {
        var yamlObjectAttribute = compilation.GetTypeByMetadataName("VYaml.Serialization.YamlObjectAttribute");
        if (yamlObjectAttribute is null)
            return null;

        return new ReferenceSymbols
        {
            YamlObjectAttribute = yamlObjectAttribute,
            YamlMemberAttribute = compilation.GetTypeByMetadataName("VYaml.Serialization.YamlMemberAttribute")!,
            YamlIgnoreAttribute = compilation.GetTypeByMetadataName("VYaml.Serialization.YamlIgnoreAttribute")!,
            YamlConstructorAttribute = compilation.GetTypeByMetadataName("VYaml.Serialization.YamlConstructor")!,
        };
    }

    public INamedTypeSymbol YamlObjectAttribute { get; private set; } = null!;
    public INamedTypeSymbol YamlMemberAttribute { get; private set; } = null!;
    public INamedTypeSymbol YamlIgnoreAttribute { get; private set; } = null!;
    public INamedTypeSymbol YamlConstructorAttribute { get; private set; } = null!;
}