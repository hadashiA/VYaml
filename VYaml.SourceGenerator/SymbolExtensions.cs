using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace VYaml.SourceGenerator;

static class SymbolExtensions
{
    // For an override property, the attributes of the properties it overrides are also looked up (the nearest one wins),
    // like Attribute.GetCustomAttribute(inherit: true).
    public static bool ContainsAttribute(this ISymbol symbol, INamedTypeSymbol attribute)
    {
        return symbol.GetAttribute(attribute) != null;
    }

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attribute)
    {
        for (var s = symbol; s != null; s = (s as IPropertySymbol)?.OverriddenProperty)
        {
            var found = s.GetAttributes().FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribute));
            if (found != null) return found;
        }
        return null;
    }

    public static AttributeData? GetImplAttribute(this ISymbol symbol, INamedTypeSymbol implAttribute)
    {
        return symbol.GetAttributes().FirstOrDefault(x =>
        {
            if (x.AttributeClass == null) return false;
            if (x.AttributeClass.EqualsUnconstructedGenericType(implAttribute)) return true;

            foreach (var item in x.AttributeClass.GetAllBaseTypes())
            {
                if (item.EqualsUnconstructedGenericType(implAttribute))
                {
                    return true;
                }
            }
            return false;
        });
    }

    public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol)
    {
        // Iterate Parent -> Derived.
        // An override property replaces the property it overrides, at the position of the parent's declaration,
        // so that the attributes declared on the override take effect.
        var members = symbol.BaseType != null
            ? GetAllMembers(symbol.BaseType).ToList()
            : new List<ISymbol>();

        foreach (var item in symbol.GetMembers())
        {
            if (!item.IsOverride)
            {
                members.Add(item);
                continue;
            }
            if (item is IPropertySymbol { OverriddenProperty: { } overridden })
            {
                var index = members.FindIndex(x => SymbolEqualityComparer.Default.Equals(x, overridden));
                if (index >= 0)
                {
                    members[index] = item;
                }
            }
        }
        return members;
    }

    public static bool InheritsFrom(this INamedTypeSymbol symbol, INamedTypeSymbol baseSymbol)
    {
        var baseName = baseSymbol.ToString();
        while (true)
        {
            if (symbol.ToString() == baseName)
            {
                return true;
            }
            if (symbol.BaseType != null)
            {
                symbol = symbol.BaseType;
                continue;
            }
            break;
        }
        return false;
    }

    public static IEnumerable<INamedTypeSymbol> GetAllBaseTypes(this INamedTypeSymbol symbol)
    {
        var t = symbol.BaseType;
        while (t != null)
        {
            yield return t;
            t = t.BaseType;
        }
    }

    public static bool EqualsUnconstructedGenericType(this INamedTypeSymbol left, INamedTypeSymbol right)
    {
        var l = left.IsGenericType ? left.ConstructUnboundGenericType() : left;
        var r = right.IsGenericType ? right.ConstructUnboundGenericType() : right;
        return SymbolEqualityComparer.Default.Equals(l, r);
    }
}
