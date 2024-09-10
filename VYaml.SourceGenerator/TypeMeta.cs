using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VYaml.SourceGenerator;

public enum NamingConvention
{
    LowerCamelCase,
    UpperCamelCase,
    SnakeCase,
    KebabCase,
}

class UnionMeta
{
    public string SubTypeTag { get; set; }
    public INamedTypeSymbol SubTypeSymbol { get; set; }
    public string FullTypeName { get; }

    public UnionMeta(string subTypeTag, INamedTypeSymbol subTypeSymbol)
    {
        SubTypeTag = subTypeTag;
        SubTypeSymbol = subTypeSymbol;
        FullTypeName = subTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}

class TypeMeta
{
    public TypeDeclarationSyntax Syntax { get; }
    public INamedTypeSymbol Symbol { get; }
    public AttributeData YamlObjectAttribute { get; }
    public string TypeName { get; }
    public string FullTypeName { get; }
    public IReadOnlyList<IMethodSymbol> Constructors { get; }
    public IReadOnlyList<UnionMeta> UnionMetas { get; }
    public NamingConvention NamingConvention { get; }

    public IReadOnlyList<MemberMeta> MemberMetas => memberMetas ??= GetSerializeMembers();
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

        foreach (var arg in YamlObjectAttribute.ConstructorArguments)
        {
            if (SymbolEqualityComparer.Default.Equals(arg.Type, references.NamingConventionEnum))
            {
                NamingConvention = arg.Value != null
                    ? (NamingConvention)arg.Value
                    : NamingConvention.LowerCamelCase;
                break;
            }
        }

        Constructors = symbol.InstanceConstructors
            .Where(x => !x.IsImplicitlyDeclared) // remove empty ctor(struct always generate it), record's clone ctor
            .ToArray();

        UnionMetas = symbol.GetAttributes()
            .Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, references.YamlObjectUnionAttribute))
            .Where(x => x.ConstructorArguments.Length == 2)
            .Select(x => new UnionMeta(
                (string)x.ConstructorArguments[0].Value!,
                (INamedTypeSymbol)x.ConstructorArguments[1].Value!))
            .ToArray();
    }

    public bool IsPartial()
    {
        return Syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    public bool IsNested()
    {
        return Syntax.Parent is TypeDeclarationSyntax;
    }

    MemberMeta[] GetSerializeMembers()
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
                .Select((x, i) => new MemberMeta(x, references, NamingConvention, i))
                .OrderBy(x => x.Order)
                .ToArray();
        }
        return memberMetas;
    }
}
