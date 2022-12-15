using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VYaml.SourceGenerator;

class UnionMeta
{
    public string SubTypeTag { get; set; }
    public INamedTypeSymbol SubTypeSymbol { get; set; }
}

class TypeMeta
{
    public TypeDeclarationSyntax Syntax { get; }
    public INamedTypeSymbol Symbol { get; }
    public AttributeData YamlObjectAttribute { get; }
    public string TypeName { get; }
    public string FullTypeName { get; }

    public IReadOnlyList<UnionMeta> UnionMetas { get; }

    public bool IsUnion => UnionMetas.Count > 0;

    ReferenceSymbols references;
    MemberMeta[]? memberMetas;

    public TypeMeta(
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol symbol,
        AttributeData yamlObjectAttribute,
        ReferenceSymbols references)
    {
        Syntax = syntax;
        Symbol = symbol;
        this.references = references;

        TypeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        FullTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        YamlObjectAttribute = yamlObjectAttribute;

        UnionMetas = symbol.GetAttributes()
            .Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, references.YamlObjectUnionAttribute))
            .Where(x => x.ConstructorArguments.Length == 2)
            .Select(x => new UnionMeta
            {
                SubTypeTag = (string)x.ConstructorArguments[0].Value!,
                SubTypeSymbol = (INamedTypeSymbol)x.ConstructorArguments[1].Value!
            })
            .ToArray();
    }

    public MemberMeta[] GetSerializeMembers()
    {
        if (memberMetas == null)
        {
            memberMetas = Symbol.GetAllMembers() // iterate includes parent type
                .Where(x => x is (IFieldSymbol or IPropertySymbol) and { IsStatic: false, IsImplicitlyDeclared: false })
                .Where(x =>
                {
                    if (x.ContainsAttribute(references.YamlIgnoreAttribute)) return false;
                    if (x.DeclaredAccessibility != Accessibility.Public) return false;

                    if (x is IPropertySymbol p)
                    {
                        // set only can't be serializable member
                        if (p.GetMethod == null && p.SetMethod != null)
                        {
                            return false;
                        }
                        if (p.IsIndexer) return false;
                    }
                    return true;
                })
                .Select((x, i) => new MemberMeta(x, references, i))
                .OrderBy(x => x.Order)
                .ToArray();
        }
        return memberMetas;
    }

    public bool IsPartial()
    {
        return Syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    public bool IsNested()
    {
        return Syntax.Parent is TypeDeclarationSyntax;
    }
}
