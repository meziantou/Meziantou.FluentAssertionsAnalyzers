using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Meziantou.FluentAssertionsAnalyzers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class XUnitAssertAnalyzerCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MFA001");

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: true);
        if (nodeToFix == null)
            return;

        var title = "Use FluentAssertions";
        var codeAction = CodeAction.Create(
            title,
            ct => Rewrite(context.Document, nodeToFix, context.CancellationToken),
            equivalenceKey: title);

        context.RegisterCodeFix(codeAction, context.Diagnostics);
    }

    private static async Task<Document> Rewrite(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var generator = editor.Generator;

        var originalMethod = (InvocationExpressionSyntax)nodeToFix;
        var originalArguments = originalMethod.ArgumentList.Arguments;
        var method = (IMethodSymbol)editor.SemanticModel.GetSymbolInfo(originalMethod, cancellationToken).Symbol;
        if (method == null)
            return document;

        var methodName = method.Name;
        var semanticModel = editor.SemanticModel;
        var compilation = editor.SemanticModel.Compilation;

        SyntaxNode result;
        if (methodName is "True" or "False")
        {
            result = RewriteTrueOrFalse(originalMethod, originalArguments, methodName);
        }
        else if (methodName is "Null" or "NotNull")
        {
            var newMethodName = methodName is "Null" ? "BeNull" : "NotBeNull";
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), newMethodName));
        }
        else if (methodName is "Collection")
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "SatisfyRespectively"),
                ArgumentListWithoutParameterName(originalMethod.ArgumentList.Arguments.Skip(1).ToArray()));
        }
        else if (methodName is "Contains" or "DoesNotContain" && method.Parameters.Length == 2 && method.ConstructedFrom.Parameters[0].Type.IsDefinition)
        {
            var newMethodName = methodName is "Contains" ? "Contain" : "NotContain";
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), newMethodName),
                ArgumentListWithoutParameterName(originalMethod.ArgumentList.Arguments[0]));
        }
        else if (methodName is "Contains" or "DoesNotContain" && method.Parameters.Length == 2 && method.Parameters[1].Type.OriginalDefinition.Equals(compilation, "System.Predicate`1"))
        {
            var newMethodName = methodName is "Contains" ? "Contain" : "NotContain";
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), newMethodName),
                ArgumentListWithoutParameterName(originalMethod.ArgumentList.Arguments[1]));
        }
        else if (methodName is "Empty")
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "BeEmpty"));
        }
        else if (methodName is "NotEmpty")
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "NotBeEmpty"));
        }
        else if (methodName is "Single" && method.Parameters.Length == 1)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "ContainSingle"));
        }
        else if (methodName is "Single" && method.Parameters.Length == 2 && method.ConstructedFrom.Parameters[1].Type.OriginalDefinition.Equals(compilation, "System.Predicate`1"))
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "ContainSingle"),
                ArgumentListWithoutParameterName(originalMethod.ArgumentList.Arguments[1]));
        }
        else if (methodName is "Equal" or "NotEqual" &&
                 method.Parameters.Length == 2 &&
                 method.Parameters[0].Type.SpecialType != SpecialType.System_String &&
                 method.Parameters[0].Type.OriginalDefinition.IsOrImplements(compilation, "System.Collections.Generic.IEnumerable`1") &&
                 method.Parameters[1].Type.OriginalDefinition.IsOrImplements(compilation, "System.Collections.Generic.IEnumerable`1"))
        {
            var newMethodName = methodName is "Equal" ? "Equal" : "NotEqual";
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), newMethodName),
                ArgumentListWithoutParameterName(originalMethod.ArgumentList.Arguments[0]));
        }
        else if (methodName is "Equal" or "NotEqual" && method.Parameters.Length == 2)
        {
            var newMethodName = methodName is "Equal" ? "Be" : "NotBe";
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), newMethodName),
                ArgumentListWithoutParameterName(originalMethod.ArgumentList.Arguments[0]));
        }
        else if (methodName is "Equal" or "NotEqual" &&
                 method.Parameters.Length == 3 &&
                 method.Parameters[0].Type.SpecialType is SpecialType.System_Double or SpecialType.System_Decimal &&
                 method.Parameters[2].Type.SpecialType is SpecialType.System_Int32)
        {
            var newMethodName = methodName is "Equal" ? "Be" : "NotBe";
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(MathRound(originalArguments[1].Expression, originalArguments[2].Expression)), newMethodName),
                ArgumentList(MathRound(originalMethod.ArgumentList.Arguments[0].Expression, originalArguments[2].Expression)));

            static ExpressionSyntax MathRound(ExpressionSyntax expression, ExpressionSyntax precision) =>
                InvocationExpression(MemberAccessExpression("System", "Math", "Round"), ArgumentList(expression, precision));
        }
        else if (methodName is "Equal" or "NotEqual" &&
                 method.Parameters.Length == 3 &&
                 method.Parameters[0].Type.SpecialType is SpecialType.System_Double or SpecialType.System_Decimal or SpecialType.System_Single &&
                 method.Parameters[2].Type.SpecialType is SpecialType.System_Double or SpecialType.System_Decimal or SpecialType.System_Single)
        {
            var newMethodName = methodName is "Equal" ? "BeApproximately" : "NotBeApproximately";

            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), newMethodName),
                ArgumentList(originalMethod.ArgumentList.Arguments[0].Expression, originalMethod.ArgumentList.Arguments[2].Expression));
        }
        else if (methodName is "Equal" or "NotEqual" && method.Parameters.Length == 3 && method.Parameters[0].Type.SpecialType is SpecialType.System_DateTime)
        {
            var newMethodName = methodName is "Equal" ? "BeCloseTo" : "NotBeCloseTo";
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), newMethodName),
                ArgumentList(originalMethod.ArgumentList.Arguments[0].Expression, originalMethod.ArgumentList.Arguments[2].Expression));
        }
        else if (methodName is "Same")
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "BeSameAs"),
                ArgumentList(originalMethod.ArgumentList.Arguments[0].Expression));
        }
        else if (methodName is "NotSame")
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "NotBeSameAs"),
                ArgumentList(originalMethod.ArgumentList.Arguments[0].Expression));
        }
        else if (methodName is "IsAssignableFrom" && method.Parameters.Length == 1)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "BeAssignableTo", (TypeSyntax)generator.TypeExpression(method.TypeArguments[0])));
        }
        else if (methodName is "IsAssignableFrom" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "BeAssignableTo"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "IsType" && method.Parameters.Length == 1)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "BeOfType", (TypeSyntax)generator.TypeExpression(method.TypeArguments[0])));
        }
        else if (methodName is "IsType" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "BeOfType"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "IsNotType" && method.Parameters.Length == 1)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "NotBeOfType", (TypeSyntax)generator.TypeExpression(method.TypeArguments[0])));
        }
        else if (methodName is "IsNotType" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "NotBeOfType"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "StartsWith" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "StartWith"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "StartsWith" && method.Parameters.Length == 3 && IsStringComparisonOrdinal(originalArguments[2].Expression, semanticModel))
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "StartWith"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "EndsWith" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "EndWith"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "EndsWith" && method.Parameters.Length == 3 && IsStringComparisonOrdinal(originalArguments[2].Expression, semanticModel))
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "EndWith"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "Contains" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "Contain"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "Contains" && method.Parameters.Length == 3 && IsStringComparisonOrdinal(originalArguments[2].Expression, semanticModel))
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "Contain"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "DoesNotContain" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "NotContain"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "DoesNotContain" && method.Parameters.Length == 3 && IsStringComparisonOrdinal(originalArguments[2].Expression, semanticModel))
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "NotContain"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "Matches" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "MatchRegex"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "DoesNotMatch" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "NotMatchRegex"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "InRange" && method.Parameters.Length == 3)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "BeInRange"),
                ArgumentListWithoutParameterName(originalArguments[1], originalArguments[2]));
        }
        else if (methodName is "NotInRange" && method.Parameters.Length == 3)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "NotBeInRange"),
                ArgumentListWithoutParameterName(originalArguments[1], originalArguments[2]));
        }
        else if (methodName is "Subset" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[1]), "BeSubsetOf"),
                ArgumentListWithoutParameterName(originalArguments[0]));
        }
        else if (methodName is "Superset" && method.Parameters.Length == 2)
        {
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(originalArguments[0]), "BeSubsetOf"),
                ArgumentListWithoutParameterName(originalArguments[1]));
        }
        else if (methodName is "Throws" && method.Parameters.Length == 1)
        {
            var expression = WrapLambdaExpressionIfNeeded(originalArguments[0].Expression, method.Parameters[0].Type);
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(expression), "ThrowExactly", (TypeSyntax)generator.TypeExpression(method.TypeArguments[0])));
        }
        else if (methodName is "ThrowsAny" && method.Parameters.Length == 1)
        {
            var expression = WrapLambdaExpressionIfNeeded(originalArguments[0].Expression, method.Parameters[0].Type);
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(expression), "Throw", (TypeSyntax)generator.TypeExpression(method.TypeArguments[0])));
        }
        else if (methodName is "ThrowsAsync" && method.Parameters.Length == 1)
        {
            var expression = WrapLambdaExpressionIfNeeded(originalArguments[0].Expression, method.Parameters[0].Type);
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(expression), "ThrowExactlyAsync", (TypeSyntax)generator.TypeExpression(method.TypeArguments[0])));
        }
        else if (methodName is "ThrowsAnyAsync" && method.Parameters.Length == 1)
        {
            var expression = WrapLambdaExpressionIfNeeded(originalArguments[0].Expression, method.Parameters[0].Type);
            result = InvocationExpression(
                MemberAccessExpression(InvokeShould(expression), "ThrowAsync", (TypeSyntax)generator.TypeExpression(method.TypeArguments[0])));
        }
        else
        {
            // Not yet supported:
            // public static void All<T>(IEnumerable<T> collection, Action<T> action)
            // public static void Contains<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
            // public static TValue Contains<TKey, TValue>(TKey expected, IReadOnlyDictionary<TKey, TValue> collection)
            // public static TValue Contains<TKey, TValue>(TKey expected, IDictionary<TKey, TValue> collection)
            // public static void DoesNotContain<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
            // public static TValue DoesNotContain<TKey, TValue>(TKey expected, IReadOnlyDictionary<TKey, TValue> collection)
            // public static TValue DoesNotContain<TKey, TValue>(TKey expected, IDictionary<TKey, TValue> collection)
            // public static void Single(IEnumerable collection, object expected)
            // public static void Equal<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer)
            // public static void Equal<T>(T expected, T actual, IEqualityComparer<T> comparer)
            // public static void NotEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer)
            // public static void NotEqual<T>(T expected, T actual, IEqualityComparer<T> comparer)
            // public static void Equal(string expected, string actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
            // public static void DoesNotMatch(Regex expectedRegex, string actualString)
            // public static void Matches(Regex expectedRegex, string actualString)
            // public static void InRange<T>(T actual, T low, T high, IComparer<T> comparer)
            // public static void NotInRange<T>(T actual, T low, T high, IComparer<T> comparer)
            // public static void StrictEqual<T>(T expected, T actual)
            // public static void NotStrictEqual<T>(T expected, T actual)
            // public static void ProperSubset<T>(ISet<T> expectedSuperset, ISet<T> actual)
            // public static void ProperSuperset<T>(ISet<T> expectedSubset, ISet<T> actual)
            // public static Exception Throws(Type exceptionType, Action testCode)
            // public static Exception Throws(Type exceptionType, Func<object> testCode)
            // public static T Throws<T>(string paramName, Action testCode) where T : ArgumentException
            // public static T Throws<T>(string paramName, Func<object> testCode) where T : ArgumentException
            // public static async Task<Exception> ThrowsAsync(Type exceptionType, Func<Task> testCode)
            // public static async Task<T> ThrowsAsync<T>(string paramName, Func<Task> testCode) where T : ArgumentException
            // public static RaisedEvent<T> Raises<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Action testCode) where T : EventArgs
            // public static RaisedEvent<T> RaisesAny<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Action testCode) where T : EventArgs
            // public static async Task<RaisedEvent<T>> RaisesAsync<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Func<Task> testCode) where T : EventArgs
            // public static async Task<RaisedEvent<T>> RaisesAnyAsync<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Func<Task> testCode) where T : EventArgs
            // public static void PropertyChanged(INotifyPropertyChanged @object, string propertyName, Action testCode)
            // public static async Task PropertyChangedAsync(INotifyPropertyChanged @object, string propertyName, Func<Task> testCode)
            //
            // Partially supported:
            // public static void StartsWith(string expectedStartString, string actualString, StringComparison comparisonType)
            // public static void EndsWith(string expectedEndString, string actualString, StringComparison comparisonType)
            // public static void DoesNotContain(string expectedSubstring, string actualString, StringComparison comparisonType)
            // public static void Contains(string expectedSubstring, string actualString, StringComparison comparisonType)
            return document;
        }

        // Add using
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        root = root.ReplaceNode(originalMethod, result
                .WithTriviaFrom(originalMethod)
                .WithAdditionalAnnotations(Simplifier.Annotation));

        document = document.WithSyntaxRoot(root);

        var unit = (CompilationUnitSyntax)root;
        if (!unit.Usings.OfType<UsingDirectiveSyntax>().Any(import => import.Name.ToString() == "FluentAssertions"))
        {
            var import = UsingDirective(IdentifierName("FluentAssertions"));
            root = ((CompilationUnitSyntax)root).AddUsings(import);
            document = document.WithSyntaxRoot(root);
            document = await Formatter.OrganizeImportsAsync(document, cancellationToken).ConfigureAwait(false);
        }

        return document;

        ExpressionSyntax WrapLambdaExpressionIfNeeded(ExpressionSyntax expression, ITypeSymbol type)
        {
            if (expression is AnonymousFunctionExpressionSyntax)
            {
                return ObjectCreationExpression((TypeSyntax)generator.TypeExpression(type))
                    .WithArgumentList(ArgumentList(expression));
            }

            return expression;
        }
    }

    private static SyntaxNode RewriteTrueOrFalse(InvocationExpressionSyntax originalMethod, SeparatedSyntaxList<ArgumentSyntax> originalArguments, string methodName)
    {
        var should = InvokeShould(originalArguments[0]);
        should = InvocationExpression(MemberAccessExpression(should, "Be" + methodName));
        if (originalMethod.ArgumentList.Arguments.Count == 2)
            should = should.AddArgumentListArguments(originalArguments[1]);

        return should;
    }

    private static InvocationExpressionSyntax InvokeShould(ArgumentSyntax argument)
    {
        return InvokeShould(argument.Expression);
    }

    private static InvocationExpressionSyntax InvokeShould(ExpressionSyntax expression)
    {
        return InvocationExpression(
                MemberAccessExpression(Parenthesize(expression), "Should"));
    }

    private static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, string memberName, TypeSyntax genericParameterType)
    {
        return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expression,
                    GenericName(memberName).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(genericParameterType))));
    }

    private static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, string memberName)
    {
        return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expression,
                    IdentifierName(memberName));
    }

    private static MemberAccessExpressionSyntax MemberAccessExpression(string expression, string memberName, params string[] memberNames)
    {
        var result = MemberAccessExpression(IdentifierName(expression), memberName);
        foreach (var name in memberNames)
        {
            result = MemberAccessExpression(result, name);
        }

        return result;
    }

    private static ExpressionSyntax Parenthesize(ExpressionSyntax expression)
    {
        var withoutTrivia = expression.WithoutTrivia();
        var parenthesized = ParenthesizedExpression(withoutTrivia);
        return parenthesized.WithTriviaFrom(expression).WithAdditionalAnnotations(Simplifier.Annotation);
    }

    private static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] expressions)
    {
        return SyntaxFactory.ArgumentList(SeparatedList(expressions.Select(expr => Argument(expr))));
    }

    private static ArgumentListSyntax ArgumentListWithoutParameterName(params ArgumentSyntax[] arguments)
    {
        return SyntaxFactory.ArgumentList(SeparatedList(arguments.Select(arg => Argument(arg.Expression))));
    }

    private static bool IsStringComparisonOrdinal(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var operation = semanticModel.GetOperation(expression);
        if (operation is IMemberReferenceOperation memberReferenceOperation)
        {
            return memberReferenceOperation.Member.Name == nameof(StringComparison.Ordinal) &&
                   memberReferenceOperation.Member.ContainingType.Equals(semanticModel.Compilation, "System.StringComparison");
        }

        return false;
    }
}
