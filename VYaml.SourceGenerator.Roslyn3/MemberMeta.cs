using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VYaml.SourceGenerator;

class MemberMeta
{
    public ISymbol Symbol { get; }
    public string Name { get; }
    public string FullTypeName { get; }
    public ITypeSymbol MemberType { get; }
    public INamedTypeSymbol? CustomFormatter { get; }
    public string? CustomFormatterName { get; }
    public bool IsField { get; }
    public bool IsProperty { get; }
    public bool IsSettable { get; }
    public bool IsConstructorParameter { get; }
    public int Order { get; }
    public bool HasExplicitOrder { get; }
    public bool HasKeyNameAlias { get; }

    public string KeyName => keyName ??= KeyNameHelper.ToCamelCase(Name);
    public byte[] KeyNameUtf8Bytes => keyNameUtf8Bytes ??= System.Text.Encoding.UTF8.GetBytes(KeyName);

    string? keyName;
    byte[]? keyNameUtf8Bytes;

    public MemberMeta(ISymbol symbol, ReferenceSymbols references, int sequentialOrder)
    {
        Symbol = symbol;
        Name = symbol.Name;
        Order = sequentialOrder;

        var memberAttribute = symbol.GetAttribute(references.YamlMemberAttribute);
        if (memberAttribute != null)
        {
            if (memberAttribute.ConstructorArguments.Length > 0)
            {
                HasKeyNameAlias = true;
                keyName = (string?)memberAttribute.ConstructorArguments[0].Value;
            }
        }

        if (symbol is IFieldSymbol f)
        {
            IsProperty = false;
            IsField = true;
            IsSettable = !f.IsReadOnly; // readonly field can not set.
            MemberType = f.Type;

        }
        else if (symbol is IPropertySymbol p)
        {
            IsProperty = true;
            IsField = false;
            IsSettable = !p.IsReadOnly;
            MemberType = p.Type;
        }
        else
        {
            throw new Exception("member is not field or property.");
        }
        FullTypeName = MemberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public Location GetLocation(TypeDeclarationSyntax fallback)
    {
        var location = Symbol.Locations.FirstOrDefault() ?? fallback.Identifier.GetLocation();
        return location;
    }
}
