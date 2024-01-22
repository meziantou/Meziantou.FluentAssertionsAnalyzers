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
public sealed class NunitAssertAnalyzerCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MFA003");

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: true);
        if (nodeToFix is null)
            return;

        var title = "Use FluentAssertions";
        var codeAction = CodeAction.Create(
            title,
            ct => Rewrite(context.Document, nodeToFix, ct),
            equivalenceKey: title);

        context.RegisterCodeFix(codeAction, context.Diagnostics);
    }

    /*  
    Partial
    Exception Assert.CatchAsync(Type expectedExceptionType, AsyncTestDelegate code, string message, object[] args)
    Exception Assert.CatchAsync(Type expectedExceptionType, AsyncTestDelegate code)
    
    Not yet supported:   
    void Assert.ByVal(object actual, IResolveConstraint expression)
    void Assert.ByVal(object actual, IResolveConstraint expression, string message, object[] args)
    void Assert.Contains(object expected, ICollection actual, string message, object[] args)
    void Assert.Contains(object expected, ICollection actual)
    void Assert.Fail(string message, object[] args)
    void Assert.Fail(string message)
    void Assert.Fail()
    void Assert.Greater(IComparable arg1, IComparable arg2, string message, object[] args)
    void Assert.Greater(IComparable arg1, IComparable arg2)
    void Assert.GreaterOrEqual(IComparable arg1, IComparable arg2, string message, object[] args)
    void Assert.GreaterOrEqual(IComparable arg1, IComparable arg2)
    void Assert.Ignore(string message, object[] args)
    void Assert.Ignore(string message)
    void Assert.Ignore()
    void Assert.Inconclusive(string message, object[] args)
    void Assert.Inconclusive(string message)
    void Assert.Inconclusive()
    void Assert.IsAssignableFrom(Type expected, object actual, string message, object[] args)
    void Assert.IsAssignableFrom(Type expected, object actual)
    void Assert.IsAssignableFrom<TExpected>(object actual, string message, object[] args)
    void Assert.IsAssignableFrom<TExpected>(object actual)
    void Assert.IsEmpty(IEnumerable collection, string message, object[] args)
    void Assert.IsEmpty(IEnumerable collection)
    void Assert.IsNotAssignableFrom(Type expected, object actual, string message, object[] args)
    void Assert.IsNotAssignableFrom(Type expected, object actual)
    void Assert.IsNotAssignableFrom<TExpected>(object actual, string message, object[] args)
    void Assert.IsNotAssignableFrom<TExpected>(object actual)
    void Assert.IsNotEmpty(IEnumerable collection, string message, object[] args)
    void Assert.IsNotEmpty(IEnumerable collection)
    void Assert.Less(IComparable arg1, IComparable arg2, string message, object[] args)
    void Assert.Less(IComparable arg1, IComparable arg2)
    void Assert.LessOrEqual(IComparable arg1, IComparable arg2, string message, object[] args)
    void Assert.LessOrEqual(IComparable arg1, IComparable arg2)
    void Assert.Multiple(TestDelegate testDelegate)
    void Assert.Multiple(AsyncTestDelegate testDelegate)
    void Assert.Pass(string message, object[] args)
    void Assert.Pass(string message)
    void Assert.That(bool condition, Func<string> getExceptionMessage)
    void Assert.That(Func<bool> condition, string message, object[] args)
    void Assert.That(Func<bool> condition)
    void Assert.That(Func<bool> condition, Func<string> getExceptionMessage)
    void Assert.That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr)
    void Assert.That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr, string message, object[] args)
    void Assert.That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr, Func<string> getExceptionMessage)
    void Assert.That(TestDelegate code, IResolveConstraint constraint)
    void Assert.That(TestDelegate code, IResolveConstraint constraint, string message, object[] args)
    void Assert.That(TestDelegate code, IResolveConstraint constraint, Func<string> getExceptionMessage)
    void Assert.That<TActual>(TActual actual, IResolveConstraint expression)
    void Assert.That<TActual>(TActual actual, IResolveConstraint expression, string message, object[] args)
    void Assert.That<TActual>(TActual actual, IResolveConstraint expression, Func<string> getExceptionMessage)
    Exception Assert.Throws(IResolveConstraint expression, TestDelegate code, string message, object[] args)
    Exception Assert.Throws(IResolveConstraint expression, TestDelegate code)
    Exception Assert.ThrowsAsync(IResolveConstraint expression, AsyncTestDelegate code, string message, object[] args)
    Exception Assert.ThrowsAsync(IResolveConstraint expression, AsyncTestDelegate code)
    void Assert.Warn(string message, object[] args)
    void Assert.Warn(string message)

    void CollectionAssert.AreEqual(IEnumerable expected, IEnumerable actual, IComparer comparer)
    void CollectionAssert.AreEqual(IEnumerable expected, IEnumerable actual, IComparer comparer, string message, object[] args)
    void CollectionAssert.AreNotEqual(IEnumerable expected, IEnumerable actual, IComparer comparer)
    void CollectionAssert.AreNotEqual(IEnumerable expected, IEnumerable actual, IComparer comparer, string message, object[] args)
    void CollectionAssert.IsNotSupersetOf(IEnumerable superset, IEnumerable subset)
    void CollectionAssert.IsNotSupersetOf(IEnumerable superset, IEnumerable subset, string message, object[] args)
    void CollectionAssert.IsOrdered(IEnumerable collection, IComparer comparer, string message, object[] args)
    void CollectionAssert.IsOrdered(IEnumerable collection, IComparer comparer)
    void CollectionAssert.IsSupersetOf(IEnumerable superset, IEnumerable subset)
    void CollectionAssert.IsSupersetOf(IEnumerable superset, IEnumerable subset, string message, object[] args)
    
    void DirectoryAssert.AreEqual(DirectoryInfo expected, DirectoryInfo actual, string message, object[] args)
    void DirectoryAssert.AreEqual(DirectoryInfo expected, DirectoryInfo actual)
    void DirectoryAssert.AreNotEqual(DirectoryInfo expected, DirectoryInfo actual, string message, object[] args)
    void DirectoryAssert.AreNotEqual(DirectoryInfo expected, DirectoryInfo actual)
    void DirectoryAssert.DoesNotExist(DirectoryInfo actual, string message, object[] args)
    void DirectoryAssert.DoesNotExist(DirectoryInfo actual)
    void DirectoryAssert.DoesNotExist(string actual, string message, object[] args)
    void DirectoryAssert.DoesNotExist(string actual)
    void DirectoryAssert.Exists(DirectoryInfo actual, string message, object[] args)
    void DirectoryAssert.Exists(DirectoryInfo actual)
    void DirectoryAssert.Exists(string actual, string message, object[] args)
    void DirectoryAssert.Exists(string actual)
    
    void FileAssert.AreEqual(Stream expected, Stream actual, string message, object[] args)
    void FileAssert.AreEqual(Stream expected, Stream actual)
    void FileAssert.AreEqual(FileInfo expected, FileInfo actual, string message, object[] args)
    void FileAssert.AreEqual(FileInfo expected, FileInfo actual)
    void FileAssert.AreEqual(string expected, string actual, string message, object[] args)
    void FileAssert.AreEqual(string expected, string actual)
    void FileAssert.AreNotEqual(Stream expected, Stream actual, string message, object[] args)
    void FileAssert.AreNotEqual(Stream expected, Stream actual)
    void FileAssert.AreNotEqual(FileInfo expected, FileInfo actual, string message, object[] args)
    void FileAssert.AreNotEqual(FileInfo expected, FileInfo actual)
    void FileAssert.AreNotEqual(string expected, string actual, string message, object[] args)
    void FileAssert.AreNotEqual(string expected, string actual)
    void FileAssert.DoesNotExist(FileInfo actual, string message, object[] args)
    void FileAssert.DoesNotExist(FileInfo actual)
    void FileAssert.DoesNotExist(string actual, string message, object[] args)
    void FileAssert.DoesNotExist(string actual)
    void FileAssert.Exists(FileInfo actual, string message, object[] args)
    void FileAssert.Exists(FileInfo actual)
    void FileAssert.Exists(string actual, string message, object[] args)
    void FileAssert.Exists(string actual)    
     */

    private static async Task<Document> Rewrite(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var generator = editor.Generator;

        if (nodeToFix is not InvocationExpressionSyntax invocationExpression)
            return document;

        var arguments = invocationExpression.ArgumentList.Arguments;
        var symbolInfo = editor.SemanticModel.GetSymbolInfo(invocationExpression, cancellationToken);
        var method = (IMethodSymbol)(symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault());
        if (method is null)
            return document;

        var methodName = method.Name;
        var semanticModel = editor.SemanticModel;
        var compilation = editor.SemanticModel.Compilation;

        var stringSymbol = compilation.GetTypeByMetadataName("System.String");
        var exceptionSymbol = compilation.GetTypeByMetadataName("System.Exception");
        var typeSymbol = compilation.GetTypeByMetadataName("System.Type");
        var resolveConstraintSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.IResolveConstraint");

        var isDynamic = semanticModel.GetOperation(invocationExpression, cancellationToken)?.Type is IDynamicTypeSymbol;
        var rewrite = new Rewriter(isDynamic);

        SyntaxNode result = null;
        var addImports = true;
        if (method.ContainingType.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.Assert"), SymbolEqualityComparer.Default)
            || method.ContainingType.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.Legacy.ClassicAssert"), SymbolEqualityComparer.Default))
        {
            if (methodName is "AreEqual")
            {
                var (left, right) = GetLeftRight(arguments, semanticModel, cancellationToken);
                var leftType = semanticModel.GetTypeInfo(left.Expression, cancellationToken).Type;
                if (left.Expression is LiteralExpressionSyntax { Token.Value: null })
                {
                    result = rewrite.UsingShould(right, "BeNull", arguments.Skip(2));
                }
                else if (leftType is null)
                {
                    // Not supported
                }
                else if (IsCollection(leftType))
                {
                    result = rewrite.UsingShould(right, "Equal", ArgumentList(left, arguments.Skip(2)));
                }
                else if (leftType.SpecialType != SpecialType.System_String && leftType.IsReferenceType)
                {
                    result = rewrite.UsingShould(right, "BeSameAs", ArgumentList(left, arguments.Skip(2)));
                }
                else
                {
                    var useBeApproximately = leftType.SpecialType is SpecialType.System_Double or SpecialType.System_Single
                                             && arguments.FirstOrDefault(x => x.NameColon?.Name.Identifier.ValueText is "delta") is not null;

                    result = rewrite.UsingShould(right, useBeApproximately ? "BeApproximately" : "Be", ArgumentList(left, arguments.Skip(2)));
                }
            }
            else if (methodName is "AreNotEqual")
            {
                var (left, right) = GetLeftRight(arguments, semanticModel, cancellationToken);
                var leftType = semanticModel.GetTypeInfo(left.Expression, cancellationToken).Type;
                if (left.Expression is LiteralExpressionSyntax { Token.Value: null })
                {
                    result = rewrite.UsingShould(right, "NotBeNull", arguments.Skip(2));
                }
                else if (leftType is null)
                {
                    // Not supported
                }
                else if (IsCollection(leftType))
                {
                    result = rewrite.UsingShould(right, "NotEqual", ArgumentList(left, arguments.Skip(2)));
                }
                else if (leftType.SpecialType != SpecialType.System_String && leftType.IsReferenceType)
                {
                    result = rewrite.UsingShould(right, "NotBeSameAs", ArgumentList(left, arguments.Skip(2)));
                }
                else
                {
                    result = rewrite.UsingShould(right, "NotBe", ArgumentList(left, arguments.Skip(2)));
                }
            }
            else if (methodName is "AreSame")
            {
                result = rewrite.UsingShould(arguments[1], "BeSameAs", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreNotSame")
            {
                result = rewrite.UsingShould(arguments[1], "NotBeSameAs", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsEmpty" && method.Parameters[0].Type.SpecialType == SpecialType.System_String)
            {
                result = rewrite.UsingShould(arguments[0], "BeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsEmpty")
            {
                result = rewrite.UsingShould(arguments[0], "BeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsNotEmpty" && method.Parameters[0].Type.SpecialType == SpecialType.System_String)
            {
                result = rewrite.UsingShould(arguments[0], "NotBeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsNotEmpty")
            {
                result = rewrite.UsingShould(arguments[0], "NotBeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsInstanceOf" && method.TypeArguments.Length == 0)
            {
                result = rewrite.UsingShould(arguments[1], "BeOfType", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsInstanceOf" && method.TypeArguments.Length == 1)
            {
                var genericType = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = rewrite.UsingShould(arguments[0], "BeOfType", genericType, arguments.Skip(1));
            }
            else if (methodName is "IsNotInstanceOf" && method.TypeArguments.Length == 0)
            {
                result = rewrite.UsingShould(arguments[1], "NotBeOfType", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsNotInstanceOf" && method.TypeArguments.Length == 1)
            {
                var genericType = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = rewrite.UsingShould(arguments[0], "NotBeOfType", genericType, arguments.Skip(1));
            }
            else if (methodName is "IsNaN")
            {
                result = rewrite.UsingShould(arguments[0], "Be", ArgumentList(MemberAccessExpression((ExpressionSyntax)generator.TypeExpression(SpecialType.System_Double), "NaN"), arguments.Skip(1)));
            }
            else if (methodName is "False" or "IsFalse")
            {
                result = rewrite.TrueOrFalse(arguments, "False");
            }
            else if (methodName is "Greater")
            {
                result = rewrite.UsingShould(arguments[0], "BeGreaterThan", ArgumentList(arguments[1], arguments.Skip(2)));
            }
            else if (methodName is "GreaterOrEqual")
            {
                result = rewrite.UsingShould(arguments[0], "BeGreaterThanOrEqualTo", ArgumentList(arguments[1], arguments.Skip(2)));
            }
            else if (methodName is "Less")
            {
                result = rewrite.UsingShould(arguments[0], "BeLessThan", ArgumentList(arguments[1], arguments.Skip(2)));
            }
            else if (methodName is "LessOrEqual")
            {
                result = rewrite.UsingShould(arguments[0], "BeLessThanOrEqualTo", ArgumentList(arguments[1], arguments.Skip(2)));
            }
            else if (methodName is "Negative")
            {
                result = rewrite.UsingShould(arguments[0], "BeNegative", arguments.Skip(1));
            }
            else if (methodName is "Null" or "IsNull")
            {
                result = rewrite.UsingShould(arguments[0], "BeNull", arguments.Skip(1));
            }
            else if (methodName is "NotNull" or "IsNotNull")
            {
                result = rewrite.UsingShould(arguments[0], "NotBeNull", arguments.Skip(1));
            }
            else if (methodName is "NotZero")
            {
                result = rewrite.UsingShould(arguments[0], "NotBe", ArgumentList(NumericLiteral(0), arguments.Skip(1)));
            }
            else if (methodName is "Positive")
            {
                result = rewrite.UsingShould(arguments[0], "BePositive", arguments.Skip(1));
            }
            else if (methodName is "True" or "IsTrue")
            {
                result = rewrite.TrueOrFalse(arguments, "True");
            }
            else if (methodName is "Pass" && arguments.Count == 0)
            {
                if (nodeToFix.Parent is ExpressionStatementSyntax)
                {
                    // Assert.Pass() => return;
                    result = ReturnStatement();
                    nodeToFix = nodeToFix.Parent;
                    addImports = false;
                }
            }
            else if (methodName is "Zero")
            {
                result = rewrite.UsingShould(arguments[0], "Be", ArgumentList(NumericLiteral(0), arguments.Skip(1)));
            }
            else if (methodName is "Catch" && method.TypeArguments.Length == 1)
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = rewrite.UsingShould(action, "Throw", exception, arguments.Skip(1));
            }
            else if (methodName is "Catch" && method.TypeArguments.Length == 0 && !method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionSymbol);
                result = rewrite.UsingShould(action, "Throw", exception, arguments.Skip(1));
            }
            else if (methodName is "Catch" && method.TypeArguments.Length == 0 && method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var exceptionType = GetConstantTypeValue(semanticModel, arguments[0].Expression, cancellationToken);
                if (exceptionType is null)
                    return document;

                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[1].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionType);
                result = rewrite.UsingShould(action, "Throw", exception, arguments.Skip(2));
            }
            else if (methodName is "CatchAsync" && method.TypeArguments.Length == 1)
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = rewrite.UsingShould(action, "ThrowAsync", exception, arguments.Skip(1));
            }
            else if (methodName is "CatchAsync" && method.TypeArguments.Length == 0 && !method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionSymbol);
                result = rewrite.UsingShould(action, "ThrowAsync", exception, arguments.Skip(1));
            }
            else if (methodName is "CatchAsync" && method.TypeArguments.Length == 0 && method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var exceptionType = GetConstantTypeValue(semanticModel, arguments[0].Expression, cancellationToken);
                if (exceptionType is null)
                    return document;

                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[1].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionType);
                result = rewrite.UsingShould(action, "ThrowAsync", exception, arguments.Skip(2));
            }
            else if (methodName is "Throws" && method.TypeArguments.Length == 1)
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = rewrite.UsingShould(action, "ThrowExactly", exception, arguments.Skip(1));
            }
            else if (methodName is "Throws" && method.TypeArguments.Length == 0 && !method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionSymbol);
                result = rewrite.UsingShould(action, "ThrowExactly", exception, arguments.Skip(1));
            }
            else if (methodName is "Throws" && method.TypeArguments.Length == 0 && method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var exceptionType = GetConstantTypeValue(semanticModel, arguments[0].Expression, cancellationToken);
                if (exceptionType is null)
                    return document;

                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[1].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionType);
                result = rewrite.UsingShould(action, "ThrowExactly", exception, arguments.Skip(2));
            }
            else if (methodName is "ThrowsAsync" && method.TypeArguments.Length == 1)
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = rewrite.UsingShould(action, "ThrowExactlyAsync", exception, arguments.Skip(1));
            }
            else if (methodName is "ThrowsAsync" && method.TypeArguments.Length == 0 && !method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionSymbol);
                result = rewrite.UsingShould(action, "ThrowExactlyAsync", exception, arguments.Skip(1));
            }
            else if (methodName is "ThrowsAsync" && method.TypeArguments.Length == 0 && method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var exceptionType = GetConstantTypeValue(semanticModel, arguments[0].Expression, cancellationToken);
                if (exceptionType is null)
                    return document;

                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[1].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionType);
                result = rewrite.UsingShould(action, "ThrowExactlyAsync", exception, arguments.Skip(2));
            }
            else if (methodName is "DoesNotThrow")
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                result = rewrite.UsingShould(action, "NotThrow", arguments.Skip(1));
            }
            else if (methodName is "DoesNotThrowAsync")
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                result = rewrite.UsingShould(action, "NotThrowAsync", arguments.Skip(1));
            }
            else if (methodName is "That")
            {
                if (method.Parameters.Length == 1 || (method.Parameters.Length >= 2 && method.Parameters[1].Type.Equals(stringSymbol, SymbolEqualityComparer.Default)))
                {
                    result = rewrite.UsingShould(arguments[0], "BeTrue", arguments.Skip(1));
                }
                else if (method.Parameters.Length >= 2 && method.Parameters[1].Type.Equals(resolveConstraintSymbol, SymbolEqualityComparer.Default))
                {
                    var isSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.Is");
                    var hasSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.Has");
                    var doesSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.Does");
                    var containsSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.Contains");
                    var throwsSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.Throws");
                    var constraintExpressionSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.ConstraintExpression");

                    var nullableBooleanSymbol = compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(compilation.GetSpecialType(SpecialType.System_Boolean));

                    var op = semanticModel.GetOperation(arguments[1].Expression, cancellationToken)?.RemoveImplicitConversion();
                    if (op is not null)
                    {
                        if (method.Parameters[0].Type.SpecialType == SpecialType.System_Boolean && Is(isSymbol, "True"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeTrue", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.SpecialType == SpecialType.System_Boolean && Is(isSymbol, "False"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeFalse", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.SpecialType == SpecialType.System_Boolean && Is(isSymbol, "Not", "True"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeFalse", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.SpecialType == SpecialType.System_Boolean && Is(isSymbol, "Not", "False"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeTrue", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.Equals(nullableBooleanSymbol, SymbolEqualityComparer.Default) && Is(isSymbol, "True"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeTrue", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.Equals(nullableBooleanSymbol, SymbolEqualityComparer.Default) && Is(isSymbol, "False"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeFalse", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.Equals(nullableBooleanSymbol, SymbolEqualityComparer.Default) && Is(isSymbol, "Not", "True"))
                        {
                            result = rewrite.UsingShould(arguments[0], "NotBeTrue", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.Equals(nullableBooleanSymbol, SymbolEqualityComparer.Default) && Is(isSymbol, "Not", "False"))
                        {
                            result = rewrite.UsingShould(arguments[0], "NotBeFalse", Array.Empty<ArgumentSyntax>());
                        }
                        else if (Is(isSymbol, "Empty"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeEmpty", arguments.Skip(2));
                        }
                        else if (Is(isSymbol, "Not", "Empty"))
                        {
                            result = rewrite.UsingShould(arguments[0], "NotBeEmpty", arguments.Skip(2));
                        }
                        else if (Is(isSymbol, "Null"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeNull", arguments.Skip(2));
                        }
                        else if (Is(isSymbol, "Not", "Null"))
                        {
                            result = rewrite.UsingShould(arguments[0], "NotBeNull", arguments.Skip(2));
                        }
                        else if (Is(isSymbol, "Null", "Or", "Empty") || Is(isSymbol, "Empty", "Or", "Null"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeNullOrEmpty", arguments.Skip(2));
                        }
                        else if (Is(isSymbol, "Not", "Null", "Or", "Empty") || Is(isSymbol, "Not", "Empty", "Or", "Null"))
                        {
                            result = rewrite.UsingShould(arguments[0], "NotBeNullOrEmpty", arguments.Skip(2));
                        }
                        else if (Is(hasSymbol, "One", "Items"))
                        {
                            result = rewrite.UsingShould(arguments[0], "HaveCount", ArgumentList(NumericLiteral(1), arguments.Skip(2)));
                        }
                        else if (Is(hasSymbol, "Count", "Zero") || Is(hasSymbol, "Length", "Zero"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeEmpty", arguments.Skip(2));
                        }
                        else if (IsMethod(out var expected, isSymbol, "EqualTo"))
                        {
                            var (isCollection, isMigrationSupported) = IsCollection(arguments[0]);
                            if (isMigrationSupported)
                            {
                                var replacementMethodName = isCollection ? "Equal" : "Be";
                                result = rewrite.UsingShould(arguments[0], replacementMethodName, ArgumentList(expected, arguments.Skip(2)));
                            }
                        }
                        else if (IsMethod(out expected, isSymbol, "EqualTo", "IgnoreCase"))
                        {
                            // Don't provide a fix if the assertion is applied to a string collection, as there is no straight conversion.
                            var isString = method.Parameters[0].Type.SpecialType == SpecialType.System_String;
                            if (isString)
                                result = rewrite.UsingShould(arguments[0], "BeEquivalentTo", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "Not", "EqualTo"))
                        {
                            var (isCollection, isMigrationSupported) = IsCollection(arguments[0]);
                            if (isMigrationSupported)
                            {
                                var replacementMethodName = isCollection ? "NotEqual" : "NotBe";
                                result = rewrite.UsingShould(arguments[0], replacementMethodName, ArgumentList(expected, arguments.Skip(2)));
                            }
                        }
                        else if (IsMethod(out expected, isSymbol, "Not", "EqualTo", "IgnoreCase"))
                        {
                            // Don't provide a fix if the assertion is applied to a string collection, as there is no straight conversion.
                            var isString = method.Parameters[0].Type.SpecialType == SpecialType.System_String;
                            if (isString)
                                result = rewrite.UsingShould(arguments[0], "NotBeEquivalentTo", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "SameAs"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeSameAs", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "Not", "SameAs"))
                        {
                            result = rewrite.UsingShould(arguments[0], "NotBeSameAs", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "EquivalentTo"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeEquivalentTo", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "Not", "EquivalentTo"))
                        {
                            result = rewrite.UsingShould(arguments[0], "NotBeEquivalentTo", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, hasSymbol, "Count", "EqualTo") || IsMethod(out expected, hasSymbol, "Length", "EqualTo"))
                        {
                            var argumentTypeSymbol = semanticModel.GetTypeInfo(arguments[0].Expression, cancellationToken).Type;
                            var replacementMethodName = argumentTypeSymbol?.SpecialType == SpecialType.System_String ? "HaveLength" : "HaveCount";

                            result = rewrite.UsingShould(arguments[0], replacementMethodName, ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, hasSymbol, "Exactly", "Items"))
                        {
                            result = rewrite.UsingShould(arguments[0], "HaveCount", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, containsSymbol, "Substring"))
                        {
                            result = rewrite.UsingShould(arguments[0], "Contain", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "Contain"))
                        {
                            result = rewrite.UsingShould(arguments[0], "Contain", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "Not", "Contain"))
                        {
                            result = rewrite.UsingShould(arguments[0], "NotContain", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, containsSymbol, "Item"))
                        {
                            result = rewrite.UsingShould(arguments[0], "Contain", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "EndWith"))
                        {
                            result = rewrite.UsingShould(arguments[0], "EndWith", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "Not", "EndWith") || IsMethod(out expected, doesSymbol, "Not", "EndsWith"))
                        {
                            result = rewrite.UsingShould(arguments[0], "NotEndWith", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "StartWith"))
                        {
                            result = rewrite.UsingShould(arguments[0], "StartWith", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "Not", "StartWith") || IsMethod(out expected, doesSymbol, "Not", "StartsWith"))
                        {
                            result = rewrite.UsingShould(arguments[0], "NotStartWith", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "GreaterThan"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeGreaterThan", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "GreaterThanOrEqualTo"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeGreaterThanOrEqualTo", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "LessThan"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeLessThan", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "LessThanOrEqualTo"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeLessThanOrEqualTo", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "InstanceOf"))
                        {
                            result = rewrite.UsingShould(arguments[0], "BeOfType", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsGenericMethod(out var instanceOfType, isSymbol, "InstanceOf"))
                        {
                            var exception = (TypeSyntax)generator.TypeExpression(instanceOfType);
                            result = rewrite.UsingShould(arguments[0], "BeOfType", exception, arguments.Skip(2));
                        }
                        else if (IsMethod(out expected, throwsSymbol, "InstanceOf") && semanticModel.GetOperation(expected, cancellationToken) is ITypeOfOperation typeOfOperation)
                        {
                            var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                            var exception = (TypeSyntax)generator.TypeExpression(typeOfOperation.TypeOperand);
                            result = rewrite.UsingShould(action, "Throw", exception, arguments.Skip(2));
                        }
                        else if (IsGenericMethod(out var type, throwsSymbol, "InstanceOf"))
                        {
                            var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                            var exception = (TypeSyntax)generator.TypeExpression(type);
                            result = rewrite.UsingShould(action, "Throw", exception, arguments.Skip(2));
                        }
                    }

                    (bool isCollection, bool isMigrationSupported) IsCollection(ArgumentSyntax argumentSyntax)
                    {
                        var argumentTypeSymbol = semanticModel.GetTypeInfo(argumentSyntax.Expression, cancellationToken).Type;
                        if (argumentTypeSymbol is null || argumentTypeSymbol.SpecialType == SpecialType.System_String)
                            return (false, true);

                        var isCollection = NunitAssertAnalyzerCodeFixProvider.IsCollection(argumentTypeSymbol);

                        var isSupportedCollection = argumentTypeSymbol.OriginalDefinition.TypeKind == TypeKind.Array
                            || argumentTypeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
                            || argumentTypeSymbol.OriginalDefinition.AllInterfaces.Any(i =>
                                i.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);

                        var isMigrationSupported = !isCollection || isSupportedCollection;

                        return (isCollection, isMigrationSupported);
                    }

                    bool Is(ITypeSymbol root, params string[] memberNames)
                    {
                        var currentOp = op;
                        for (var i = memberNames.Length - 1; i >= 0; i--)
                        {
                            if (currentOp is not IMemberReferenceOperation member)
                                return false;

                            if (member.Member.Name != memberNames[i])
                                return false;

                            if (i == 0)
                                return SymbolEqualityComparer.Default.Equals(member.Member.ContainingType, root);

                            if (member.Instance is null)
                                break;

                            currentOp = member.Instance;
                        }

                        return false;
                    }

                    bool IsMethod(out ExpressionSyntax argument, ITypeSymbol root, params string[] memberNames)
                    {
                        argument = null;

                        var currentOp = op;
                        for (var i = memberNames.Length - 1; i >= 0; i--)
                        {
                            if (currentOp is IInvocationOperation invocation)
                            {
                                if (argument is not null || invocation.Arguments.Length != 1)
                                    return false;

                                if (invocation.TargetMethod.Name != memberNames[i])
                                    return false;

                                argument = (ExpressionSyntax)invocation.Arguments[0].Value.Syntax;

                                if (i == 0)
                                    return SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, root);

                                currentOp = invocation.Instance;
                            }
                            else
                            {
                                if (currentOp is not IMemberReferenceOperation member)
                                    return false;

                                if (member.Member.Name != memberNames[i])
                                    return false;

                                if (i == 0)
                                    return SymbolEqualityComparer.Default.Equals(member.Member.ContainingType, root);

                                if (member.Instance is null)
                                    break;

                                currentOp = member.Instance;
                            }
                        }

                        return false;
                    }

                    bool IsGenericMethod(out ITypeSymbol typeArgument, ITypeSymbol root, params string[] memberNames)
                    {
                        typeArgument = null;

                        var currentOp = op;
                        for (var i = memberNames.Length - 1; i >= 0; i--)
                        {
                            if (currentOp is IInvocationOperation invocation)
                            {
                                if (typeArgument is not null || invocation.TargetMethod.TypeArguments.Length == 0)
                                    return false;

                                if (invocation.TargetMethod.Name != memberNames[i])
                                    return false;

                                typeArgument = invocation.TargetMethod.TypeArguments[0];

                                if (i == 0)
                                    return SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, root);

                                currentOp = invocation.Instance;
                            }
                            else
                            {
                                if (currentOp is not IMemberReferenceOperation member)
                                    return false;

                                if (member.Member.Name != memberNames[i])
                                    return false;

                                if (i == 0)
                                    return SymbolEqualityComparer.Default.Equals(member.Member.ContainingType, root);

                                if (member.Instance is null)
                                    break;

                                currentOp = member.Instance;
                            }
                        }

                        return false;
                    }
                }
            }
        }
        else if (method.ContainingType.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.StringAssert"), SymbolEqualityComparer.Default))
        {
            if (methodName is "AreEqualIgnoringCase")
            {
                result = rewrite.UsingShould(arguments[1], "BeEquivalentTo", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreNotEqualIgnoringCase")
            {
                result = rewrite.UsingShould(arguments[1], "NotBeEquivalentTo", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "Contains")
            {
                result = rewrite.UsingShould(arguments[1], "Contain", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "DoesNotContain")
            {
                result = rewrite.UsingShould(arguments[1], "NotContain", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "DoesNotEndWith")
            {
                result = rewrite.UsingShould(arguments[1], "NotEndWith", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "DoesNotMatch")
            {
                result = rewrite.UsingShould(arguments[1], "NotMatchRegex", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "DoesNotStartWith")
            {
                result = rewrite.UsingShould(arguments[1], "NotStartWith", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "EndsWith")
            {
                result = rewrite.UsingShould(arguments[1], "EndWith", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsMatch")
            {
                result = rewrite.UsingShould(arguments[1], "MatchRegex", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "StartsWith")
            {
                result = rewrite.UsingShould(arguments[1], "StartWith", ArgumentList(arguments[0], arguments.Skip(2)));
            }
        }
        else if (method.ContainingType.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.CollectionAssert"), SymbolEqualityComparer.Default))
        {
            if (methodName is "AllItemsAreInstancesOfType")
            {
                result = rewrite.UsingShould(arguments[0], "AllBeOfType", ArgumentList(arguments[1], arguments.Skip(2)));
            }
            else if (methodName is "AllItemsAreNotNull")
            {
                result = rewrite.UsingShould(arguments[0], "NotContainNulls", arguments.Skip(1));
            }
            else if (methodName is "AllItemsAreUnique")
            {
                result = rewrite.UsingShould(arguments[0], "OnlyHaveUniqueItems", arguments.Skip(1));
            }
            else if (methodName is "AreEqual" && (method.Parameters.Length <= 2 || !method.Parameters[2].Type.Equals(compilation.GetTypeByMetadataName("System.Collection.IComparer"), SymbolEqualityComparer.Default)))
            {
                result = rewrite.UsingShould(arguments[1], "Equal", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreNotEqual" && (method.Parameters.Length <= 2 || !method.Parameters[2].Type.Equals(compilation.GetTypeByMetadataName("System.Collection.IComparer"), SymbolEqualityComparer.Default)))
            {
                result = rewrite.UsingShould(arguments[1], "NotEqual", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreEquivalent")
            {
                result = rewrite.UsingShould(arguments[1], "BeEquivalentTo", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreNotEquivalent")
            {
                result = rewrite.UsingShould(arguments[1], "NotBeEquivalentTo", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "Contains")
            {
                result = rewrite.UsingShould(arguments[0], "Contain", ArgumentList(arguments[1], arguments.Skip(2)));
            }
            else if (methodName is "DoesNotContain")
            {
                result = rewrite.UsingShould(arguments[0], "NotContain", ArgumentList(arguments[1], arguments.Skip(2)));
            }
            else if (methodName is "IsNotEmpty")
            {
                result = rewrite.UsingShould(arguments[0], "NotBeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsEmpty")
            {
                result = rewrite.UsingShould(arguments[0], "BeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsSubsetOf")
            {
                result = rewrite.UsingShould(arguments[1], "BeSubsetOf", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsNotSubsetOf")
            {
                result = rewrite.UsingShould(arguments[1], "NotBeSubsetOf", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsOrdered" && (method.Parameters.Length < 2 || !method.Parameters[1].Type.Equals(compilation.GetTypeByMetadataName("System.Collection.IComparer"), SymbolEqualityComparer.Default)))
            {
                result = rewrite.UsingShould(arguments[0], "BeInAscendingOrder", arguments.Skip(1));
            }
        }

        // Not yet supported
        if (result is null)
            return document;

        // Rewrite document
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        root = root.ReplaceNode(nodeToFix, result
              .WithTriviaFrom(nodeToFix)
              .WithAdditionalAnnotations(Simplifier.Annotation));

        document = document.WithSyntaxRoot(root);
        if (addImports)
        {
            var unit = (CompilationUnitSyntax)root;
            if (!unit.Usings.OfType<UsingDirectiveSyntax>().Any(import => import.Name.ToString() == "FluentAssertions"))
            {
                var import = UsingDirective(IdentifierName("FluentAssertions"));
                root = ((CompilationUnitSyntax)root).AddUsings(import).WithAdditionalAnnotations(Simplifier.Annotation);
            }

            document = document.WithSyntaxRoot(root);
            document = await Formatter.OrganizeImportsAsync(document, cancellationToken).ConfigureAwait(false);
        }

        return document;
    }

    private static bool IsCollection(ITypeSymbol argumentTypeSymbol)
    {
        if (argumentTypeSymbol.SpecialType == SpecialType.System_String)
            return false;

        return argumentTypeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Collections_IEnumerable
               || argumentTypeSymbol.OriginalDefinition.AllInterfaces.Any(i =>
                   i.SpecialType == SpecialType.System_Collections_IEnumerable);
    }

    private static (ArgumentSyntax left, ArgumentSyntax right) GetLeftRight(SeparatedSyntaxList<ArgumentSyntax> arguments, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var left = arguments[0];
        var right = arguments[1];
        var leftValue = semanticModel.GetConstantValue(left.Expression, cancellationToken);
        var rightValue = semanticModel.GetConstantValue(right.Expression, cancellationToken);

        // Don't invert if both are constant
        if (leftValue.HasValue && rightValue.HasValue)
        {
            return (left, right);
        }

        // Invert if right is constant
        if (rightValue.HasValue)
        {
            return (right, left);
        }

        return (left, right);
    }

    private static ITypeSymbol GetConstantTypeValue(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
    {
        var operation = semanticModel.GetOperation(expression, cancellationToken);
        if (operation is ITypeOfOperation typeOfOperation)
            return typeOfOperation.TypeOperand;

        return null;
    }

    private static LiteralExpressionSyntax NumericLiteral(int value)
    {
        return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
    }

    private static List<ArgumentSyntax> ArgumentList(ArgumentSyntax argument, IEnumerable<ArgumentSyntax> arguments)
    {
        return ArgumentList(argument.Expression, arguments);
    }

    private static List<ArgumentSyntax> ArgumentList(ExpressionSyntax expression, IEnumerable<ArgumentSyntax> arguments)
    {
        return [Argument(expression), .. arguments];
    }

    private sealed class Rewriter(bool isDynamic)
    {
        public InvocationExpressionSyntax UsingShould(ArgumentSyntax subject, string methodName, IEnumerable<ArgumentSyntax> arguments)
        {
            return UsingShould(subject.Expression, methodName, genericParameterType: null, arguments);
        }

        public InvocationExpressionSyntax UsingShould(ExpressionSyntax subject, string methodName, IEnumerable<ArgumentSyntax> arguments)
        {
            return UsingShould(subject, methodName, genericParameterType: null, arguments);
        }

        public InvocationExpressionSyntax UsingShould(ArgumentSyntax subject, string methodName, TypeSyntax genericParameterType, IEnumerable<ArgumentSyntax> arguments)
        {
            return UsingShould(subject.Expression, methodName, genericParameterType, arguments);
        }

        public InvocationExpressionSyntax UsingShould(ExpressionSyntax subject, string methodName, TypeSyntax genericParameterType, IEnumerable<ArgumentSyntax> arguments)
        {
            var should = InvokeShould(subject);
            should = InvocationExpression(MemberAccessExpression(should, methodName, genericParameterType));
            should = should.AddArgumentListArguments(arguments.Select(arg => Argument(arg.Expression)).ToArray());
            return should;
        }

        public InvocationExpressionSyntax TrueOrFalse(SeparatedSyntaxList<ArgumentSyntax> originalArguments, string methodName)
        {
            return UsingShould(originalArguments[0], "Be" + methodName, originalArguments.Skip(1).ToArray());
        }

        private InvocationExpressionSyntax InvokeShould(ExpressionSyntax expression)
        {
            var expr = expression;
            if (isDynamic)
            {
                expr = SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName("object"), expression);
            }
            return InvocationExpression(
                    MemberAccessExpression(expr.Parenthesize(), "Should"))
                .WithAdditionalAnnotations(Simplifier.Annotation);
        }
    }

    private static ExpressionSyntax InvokeFluentActionsInvoking(Compilation compilation, SyntaxGenerator generator, ExpressionSyntax expression)
    {
        var type = generator.TypeExpression(compilation.GetTypeByMetadataName("FluentAssertions.FluentActions"));
        var memberAccess = generator.MemberAccessExpression(type, "Invoking");
        return (ExpressionSyntax)generator.InvocationExpression(memberAccess, expression);
    }

    private static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, string memberName, TypeSyntax genericParameterType)
    {
        SimpleNameSyntax name;
        if (genericParameterType is not null)
        {
            name = GenericName(memberName).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(genericParameterType)));
        }
        else
        {
            name = IdentifierName(memberName);
        }

        return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, name);
    }

    private static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, string memberName)
    {
        return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expression,
                    IdentifierName(memberName));
    }


}
