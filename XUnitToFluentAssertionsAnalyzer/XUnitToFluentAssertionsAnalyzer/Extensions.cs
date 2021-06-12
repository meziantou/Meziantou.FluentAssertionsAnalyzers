using Microsoft.CodeAnalysis;

namespace XUnitToFluentAssertionsAnalyzer
{
    internal static class Extensions
    {
        public static bool Equals(this ITypeSymbol symbol, Compilation compilation, string type)
        {
            return symbol.Equals(compilation.GetTypeByMetadataName(type), SymbolEqualityComparer.Default);
        }

        public static bool IsOrImplements(this ITypeSymbol symbol, Compilation compilation, string type)
        {
            INamedTypeSymbol otherSymbol = compilation.GetTypeByMetadataName(type);
            if (symbol.Equals(otherSymbol, SymbolEqualityComparer.Default))
                return true;

            foreach (var s in symbol.AllInterfaces)
            {
                if (s.OriginalDefinition.Equals(otherSymbol, SymbolEqualityComparer.Default))
                    return true;
            }

            return false;
        }
    }
}
