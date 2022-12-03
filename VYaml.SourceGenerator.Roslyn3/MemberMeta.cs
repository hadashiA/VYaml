using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VYaml.SourceGenerator;

class MemberMeta
{
    public ISymbol Symbol { get; }
    public string Name { get; }
    public ITypeSymbol MemberType { get; }
    public INamedTypeSymbol? CustomFormatter { get; }
    public string? CustomFormatterName { get; }
    public bool IsField { get; }
    public bool IsProperty { get; }
    public bool IsSettable { get; }
    public bool IsAssignable { get; }
    public bool IsConstructorParameter { get; }
    public int Order { get; }
    public bool HasExplicitOrder { get; }

    public byte[] NameUtf8Bytes => nameUtf8Bytes ??= System.Text.Encoding.UTF8.GetBytes(Name);
    byte[]? nameUtf8Bytes;

    public MemberMeta(ISymbol symbol, ReferenceSymbols references, int sequentialOrder)
    {
        Symbol = symbol;
        Name = symbol.Name;
        Order = sequentialOrder;
        // var memberAttribute = symbol.GetAttribute(references.YamlMemberAttribute);

        if (symbol is IFieldSymbol f)
        {
            IsProperty = false;
            IsField = true;
            IsSettable = !f.IsReadOnly; // readonly field can not set.
            IsAssignable = IsSettable;
            MemberType = f.Type;

        }
        else if (symbol is IPropertySymbol p)
        {
            IsProperty = true;
            IsField = false;
            IsSettable = !p.IsReadOnly;
            IsAssignable = IsSettable;
            MemberType = p.Type;
        }
        else
        {
            throw new Exception("member is not field or property.");
        }
    }

    public Location GetLocation(TypeDeclarationSyntax fallback)
    {
        var location = Symbol.Locations.FirstOrDefault() ?? fallback.Identifier.GetLocation();
        return location;
    }
}
