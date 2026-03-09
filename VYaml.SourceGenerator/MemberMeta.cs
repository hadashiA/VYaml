using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VYaml.SourceGenerator;

class MemberMeta
{
    public ISymbol Symbol { get; }
    public string Name { get; }
    public string FullTypeName { get; }
    public ITypeSymbol MemberType { get; }
    public bool IsField { get; }
    public bool IsProperty { get; }
    public bool IsSettable { get; }
    public int Order { get; }
    public bool HasExplicitOrder { get; }
    public bool HasKeyNameAlias { get; }
    public string KeyName { get; }
    public NamingConvention NamingConventionByType { get; }

    public bool IsConstructorParameter { get; set; }
    public bool HasExplicitDefaultValueFromConstructor { get; set; }
    public object? ExplicitDefaultValueFromConstructor { get; set; }

    public byte[] KeyNameUtf8Bytes => keyNameUtf8Bytes ??= System.Text.Encoding.UTF8.GetBytes(KeyName);
    byte[]? keyNameUtf8Bytes;

    public MemberMeta(ISymbol symbol, ReferenceSymbols references, int sequentialOrder, NamingConvention namingConventionByType = default)
    {
        Symbol = symbol;
        Name = symbol.Name;
        Order = sequentialOrder;
        NamingConventionByType = namingConventionByType;
        KeyName = NamingConventionMutator.Mutate(Name, NamingConventionByType);

        var memberAttribute = symbol.GetAttribute(references.YamlMemberAttribute);
        if (memberAttribute != null)
        {
            if (memberAttribute.ConstructorArguments.Length > 0 &&
                memberAttribute.ConstructorArguments[0].Value is string aliasValue)
            {
                HasKeyNameAlias = true;
                KeyName = aliasValue;
            }

            var orderProp = memberAttribute.NamedArguments.FirstOrDefault(x => x.Key == "Order");
            if (orderProp is { Key: "Order", Value.Value: { } explicitOrder })
            {
                HasExplicitOrder = true;
                Order = (int)explicitOrder;
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
            throw new InvalidOperationException("member is not field or property.");
        }
        FullTypeName = MemberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public Location GetLocation(TypeDeclarationSyntax fallback)
    {
        var location = Symbol.Locations.FirstOrDefault() ?? fallback.Identifier.GetLocation();
        return location;
    }

    public string EmitDefaultValue()
    {
        if (!HasExplicitDefaultValueFromConstructor)
        {
            return (MemberType is { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated or NullableAnnotation.None })
                ? $"default({FullTypeName})!"
                : $"default({FullTypeName})";
        }

        if (ExplicitDefaultValueFromConstructor is null)
        {
            return $"default({FullTypeName})";
        }

        // Use MemberType.SpecialType instead of runtime type pattern matching,
        // because Roslyn may box numeric default values as int regardless of the parameter type.
        return MemberType.SpecialType switch
        {
            SpecialType.System_String => $"\"{ExplicitDefaultValueFromConstructor}\"",
            SpecialType.System_Single => $"{ExplicitDefaultValueFromConstructor}f",
            SpecialType.System_Double => $"{ExplicitDefaultValueFromConstructor}d",
            SpecialType.System_Decimal => $"{ExplicitDefaultValueFromConstructor}m",
            SpecialType.System_Boolean => (bool)ExplicitDefaultValueFromConstructor ? "true" : "false",
            SpecialType.System_Int32 => $"{ExplicitDefaultValueFromConstructor}",
            SpecialType.System_Int64 => $"{ExplicitDefaultValueFromConstructor}L",
            SpecialType.System_UInt32 => $"{ExplicitDefaultValueFromConstructor}u",
            SpecialType.System_UInt64 => $"{ExplicitDefaultValueFromConstructor}ul",
            SpecialType.System_Int16 => $"(short){ExplicitDefaultValueFromConstructor}",
            SpecialType.System_UInt16 => $"(ushort){ExplicitDefaultValueFromConstructor}",
            SpecialType.System_Byte => $"(byte){ExplicitDefaultValueFromConstructor}",
            SpecialType.System_SByte => $"(sbyte){ExplicitDefaultValueFromConstructor}",
            SpecialType.System_Char => $"(char){ExplicitDefaultValueFromConstructor}",
            _ when MemberType.TypeKind == TypeKind.Enum => $"({FullTypeName}){ExplicitDefaultValueFromConstructor}",
            _ => ExplicitDefaultValueFromConstructor.ToString()
        };
    }
}
