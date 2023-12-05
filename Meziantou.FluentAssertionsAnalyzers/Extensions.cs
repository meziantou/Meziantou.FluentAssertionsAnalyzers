using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.FluentAssertionsAnalyzers;

internal static class Extensions
{
    public static bool Equals(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        if (symbol is null)
            return false;

        return symbol.Equals(compilation.GetTypeByMetadataName(type), SymbolEqualityComparer.Default);
    }

    public static bool IsOrImplements(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        if (symbol is null)
            return false;

        var otherSymbol = compilation.GetTypeByMetadataName(type);
        if (symbol.Equals(otherSymbol, SymbolEqualityComparer.Default))
            return true;

        foreach (var s in symbol.AllInterfaces)
        {
            if (s.OriginalDefinition.Equals(otherSymbol, SymbolEqualityComparer.Default))
                return true;
        }

        return false;
    }

    public static bool IsOrInheritsFrom(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        if (symbol is null)
            return false;

        var otherSymbol = compilation.GetTypeByMetadataName(type);
        if (otherSymbol is null)
            return false;

        do
        {
            if (symbol.Equals(otherSymbol, SymbolEqualityComparer.Default))
                return true;

            symbol = symbol.BaseType;
        } while (symbol is not null);

        return false;
    }

    public static bool IsOrInheritsFrom(this ITypeSymbol symbol, INamedTypeSymbol baseSymbol)
    {
        if (symbol is null || baseSymbol is null)
            return false;

        do
        {
            if (symbol.Equals(baseSymbol, SymbolEqualityComparer.Default))
                return true;

            symbol = symbol.BaseType;
        } while (symbol is not null);

        return false;
    }

    public static IOperation RemoveImplicitConversion(this IOperation operation)
    {
        while (operation is IConversionOperation conversionOperation && conversionOperation.IsImplicit)
        {
            operation = conversionOperation.Operand;
        }

        return operation;
    }
}
