using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.FluentAssertionsAnalyzers;

internal static class Extensions
{
    public static bool Equals(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        if(symbol == null)
            return false;

        return symbol.Equals(compilation.GetTypeByMetadataName(type), SymbolEqualityComparer.Default);
    }

    public static bool IsOrImplements(this ITypeSymbol symbol, Compilation compilation, string type)
    {
        if (symbol == null)
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
        if (symbol == null)
            return false;

        var otherSymbol = compilation.GetTypeByMetadataName(type);
        if (otherSymbol == null)
            return false;

        do
        {
            if (symbol.Equals(otherSymbol, SymbolEqualityComparer.Default))
                return true;

            symbol = symbol.BaseType;
        } while (symbol != null);

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
