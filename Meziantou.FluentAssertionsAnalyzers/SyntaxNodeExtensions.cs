using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Meziantou.FluentAssertionsAnalyzers;

internal static class SyntaxNodeExtensions
{
    public static ParenthesizedExpressionSyntax Parenthesize(this ExpressionSyntax expression)
    {
        var withoutTrivia = expression.WithoutTrivia();
        var parenthesized = ParenthesizedExpression(withoutTrivia);
        return parenthesized.WithTriviaFrom(expression).WithAdditionalAnnotations(Simplifier.Annotation);
    }
}
