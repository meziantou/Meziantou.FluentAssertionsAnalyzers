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
        if (nodeToFix == null)
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
        var method = (IMethodSymbol)editor.SemanticModel.GetSymbolInfo(invocationExpression, cancellationToken).Symbol;
        if (method == null)
            return document;

        var methodName = method.Name;
        var semanticModel = editor.SemanticModel;
        var compilation = editor.SemanticModel.Compilation;

        var stringSymbol = compilation.GetTypeByMetadataName("System.String");
        var exceptionSymbol = compilation.GetTypeByMetadataName("System.Exception");
        var typeSymbol = compilation.GetTypeByMetadataName("System.Type");
        var resolveConstraintSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.IResolveConstraint");

        SyntaxNode result = null;
        var addImports = true;
        if (method.ContainingType.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.Assert"), SymbolEqualityComparer.Default))
        {
            if (methodName is "AreEqual")
            {
                if (method.Parameters[0].Type.SpecialType == SpecialType.System_Double)
                {
                    result = RewriteUsingShould(arguments[1], "BeApproximately", ArgumentList(arguments[0], arguments.Skip(2)));
                }
                else
                {
                    result = RewriteUsingShould(arguments[1], "Be", ArgumentList(arguments[0], arguments.Skip(2)));
                }
            }
            else if (methodName is "AreNotEqual")
            {
                result = RewriteUsingShould(arguments[1], "NotBe", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreSame")
            {
                result = RewriteUsingShould(arguments[1], "BeSameAs", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreNotSame")
            {
                result = RewriteUsingShould(arguments[1], "NotBeSameAs", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsEmpty" && method.Parameters[0].Type.SpecialType == SpecialType.System_String)
            {
                result = RewriteUsingShould(arguments[0], "BeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsEmpty")
            {
                result = RewriteUsingShould(arguments[0], "BeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsNotEmpty" && method.Parameters[0].Type.SpecialType == SpecialType.System_String)
            {
                result = RewriteUsingShould(arguments[0], "NotBeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsNotEmpty")
            {
                result = RewriteUsingShould(arguments[0], "NotBeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsInstanceOf" && method.TypeArguments.Length == 0)
            {
                result = RewriteUsingShould(arguments[1], "BeOfType", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsInstanceOf" && method.TypeArguments.Length == 1)
            {
                var genericType = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = RewriteUsingShould(arguments[0], "BeOfType", genericType, arguments.Skip(1));
            }
            else if (methodName is "IsNotInstanceOf" && method.TypeArguments.Length == 0)
            {
                result = RewriteUsingShould(arguments[1], "NotBeOfType", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsNotInstanceOf" && method.TypeArguments.Length == 1)
            {
                var genericType = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = RewriteUsingShould(arguments[0], "NotBeOfType", genericType, arguments.Skip(1));
            }
            else if (methodName is "IsNaN")
            {
                result = RewriteUsingShould(arguments[0], "Be", ArgumentList(MemberAccessExpression((ExpressionSyntax)generator.TypeExpression(SpecialType.System_Double), "NaN"), arguments.Skip(1)));
            }
            else if (methodName is "False" or "IsFalse")
            {
                result = RewriteTrueOrFalse(arguments, "False");
            }
            else if (methodName is "Greater")
            {
                result = RewriteUsingShould(arguments[1], "BeGreaterThan", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "GreaterOrEqual")
            {
                result = RewriteUsingShould(arguments[1], "BeGreaterThanOrEqualTo", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "Less")
            {
                result = RewriteUsingShould(arguments[1], "BeLessThan", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "LessOrEqual")
            {
                result = RewriteUsingShould(arguments[1], "BeLessThanOrEqualTo", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "Negative")
            {
                result = RewriteUsingShould(arguments[0], "BeNegative", arguments.Skip(1));
            }
            else if (methodName is "Null" or "IsNull")
            {
                result = RewriteUsingShould(arguments[0], "BeNull", arguments.Skip(1));
            }
            else if (methodName is "NotNull" or "IsNotNull")
            {
                result = RewriteUsingShould(arguments[0], "NotBeNull", arguments.Skip(1));
            }
            else if (methodName is "NotZero")
            {
                result = RewriteUsingShould(arguments[0], "NotBe", ArgumentList(NumericLiteral(0), arguments.Skip(1)));
            }
            else if (methodName is "Positive")
            {
                result = RewriteUsingShould(arguments[0], "BePositive", arguments.Skip(1));
            }
            else if (methodName is "True" or "IsTrue")
            {
                result = RewriteTrueOrFalse(arguments, "True");
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
                result = RewriteUsingShould(arguments[0], "Be", ArgumentList(NumericLiteral(0), arguments.Skip(1)));
            }
            else if (methodName is "Catch" && method.TypeArguments.Length == 1)
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = RewriteUsingShould(action, "Throw", exception, arguments.Skip(1));
            }
            else if (methodName is "Catch" && method.TypeArguments.Length == 0 && !method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionSymbol);
                result = RewriteUsingShould(action, "Throw", exception, arguments.Skip(1));
            }
            else if (methodName is "Catch" && method.TypeArguments.Length == 0 && method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var exceptionType = GetConstantTypeValue(semanticModel, arguments[0].Expression, cancellationToken);
                if (exceptionType == null)
                    return document;

                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[1].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionType);
                result = RewriteUsingShould(action, "Throw", exception, arguments.Skip(2));
            }
            else if (methodName is "CatchAsync" && method.TypeArguments.Length == 1)
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = RewriteUsingShould(action, "ThrowAsync", exception, arguments.Skip(1));
            }
            else if (methodName is "CatchAsync" && method.TypeArguments.Length == 0 && !method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionSymbol);
                result = RewriteUsingShould(action, "ThrowAsync", exception, arguments.Skip(1));
            }
            else if (methodName is "CatchAsync" && method.TypeArguments.Length == 0 && method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var exceptionType = GetConstantTypeValue(semanticModel, arguments[0].Expression, cancellationToken);
                if (exceptionType == null)
                    return document;

                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[1].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionType);
                result = RewriteUsingShould(action, "ThrowAsync", exception, arguments.Skip(2));
            }
            else if (methodName is "Throws" && method.TypeArguments.Length == 1)
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = RewriteUsingShould(action, "ThrowExactly", exception, arguments.Skip(1));
            }
            else if (methodName is "Throws" && method.TypeArguments.Length == 0 && !method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionSymbol);
                result = RewriteUsingShould(action, "ThrowExactly", exception, arguments.Skip(1));
            }
            else if (methodName is "Throws" && method.TypeArguments.Length == 0 && method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var exceptionType = GetConstantTypeValue(semanticModel, arguments[0].Expression, cancellationToken);
                if (exceptionType == null)
                    return document;

                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[1].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionType);
                result = RewriteUsingShould(action, "ThrowExactly", exception, arguments.Skip(2));
            }
            else if (methodName is "ThrowsAsync" && method.TypeArguments.Length == 1)
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(method.TypeArguments[0]);
                result = RewriteUsingShould(action, "ThrowExactlyAsync", exception, arguments.Skip(1));
            }
            else if (methodName is "ThrowsAsync" && method.TypeArguments.Length == 0 && !method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionSymbol);
                result = RewriteUsingShould(action, "ThrowExactlyAsync", exception, arguments.Skip(1));
            }
            else if (methodName is "ThrowsAsync" && method.TypeArguments.Length == 0 && method.Parameters[0].Type.Equals(typeSymbol, SymbolEqualityComparer.Default))
            {
                var exceptionType = GetConstantTypeValue(semanticModel, arguments[0].Expression, cancellationToken);
                if (exceptionType == null)
                    return document;

                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[1].Expression);
                var exception = (TypeSyntax)generator.TypeExpression(exceptionType);
                result = RewriteUsingShould(action, "ThrowExactlyAsync", exception, arguments.Skip(2));
            }
            else if (methodName is "DoesNotThrow")
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                result = RewriteUsingShould(action, "NotThrow", arguments.Skip(1));
            }
            else if (methodName is "DoesNotThrowAsync")
            {
                var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                result = RewriteUsingShould(action, "NotThrowAsync", arguments.Skip(1));
            }
            else if (methodName is "That")
            {
                if (method.Parameters.Length == 1 || (method.Parameters.Length >= 2 && method.Parameters[1].Type.Equals(stringSymbol, SymbolEqualityComparer.Default)))
                {
                    result = RewriteUsingShould(arguments[0], "BeTrue", arguments.Skip(1));
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
                            result = RewriteUsingShould(arguments[0], "BeTrue", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.SpecialType == SpecialType.System_Boolean && Is(isSymbol, "False"))
                        {
                            result = RewriteUsingShould(arguments[0], "BeFalse", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.SpecialType == SpecialType.System_Boolean && Is(isSymbol, "Not", "True"))
                        {
                            result = RewriteUsingShould(arguments[0], "BeFalse", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.SpecialType == SpecialType.System_Boolean && Is(isSymbol, "Not", "False"))
                        {
                            result = RewriteUsingShould(arguments[0], "BeTrue", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.Equals(nullableBooleanSymbol, SymbolEqualityComparer.Default) && Is(isSymbol, "True"))
                        {
                            result = RewriteUsingShould(arguments[0], "BeTrue", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.Equals(nullableBooleanSymbol, SymbolEqualityComparer.Default) && Is(isSymbol, "False"))
                        {
                            result = RewriteUsingShould(arguments[0], "BeFalse", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.Equals(nullableBooleanSymbol, SymbolEqualityComparer.Default) && Is(isSymbol, "Not", "True"))
                        {
                            result = RewriteUsingShould(arguments[0], "NotBeTrue", Array.Empty<ArgumentSyntax>());
                        }
                        else if (method.Parameters[0].Type.Equals(nullableBooleanSymbol, SymbolEqualityComparer.Default) && Is(isSymbol, "Not", "False"))
                        {
                            result = RewriteUsingShould(arguments[0], "NotBeFalse", Array.Empty<ArgumentSyntax>());
                        }
                        else if (Is(isSymbol, "Empty"))
                        {
                            result = RewriteUsingShould(arguments[0], "BeEmpty", arguments.Skip(2));
                        }
                        else if (Is(isSymbol, "Not", "Empty"))
                        {
                            result = RewriteUsingShould(arguments[0], "NotBeEmpty", arguments.Skip(2));
                        }
                        else if (Is(isSymbol, "Null"))
                        {
                            result = RewriteUsingShould(arguments[0], "BeNull", arguments.Skip(2));
                        }
                        else if (Is(isSymbol, "Not", "Null"))
                        {
                            result = RewriteUsingShould(arguments[0], "NotBeNull", arguments.Skip(2));
                        }
                        else if (Is(isSymbol, "Null", "Or", "Empty") || Is(isSymbol, "Empty", "Or", "Null"))
                        {
                            result = RewriteUsingShould(arguments[0], "BeNullOrEmpty", arguments.Skip(2));
                        }
                        else if (Is(isSymbol, "Not", "Null", "Or", "Empty") || Is(isSymbol, "Not", "Empty", "Or", "Null"))
                        {
                            result = RewriteUsingShould(arguments[0], "NotBeNullOrEmpty", arguments.Skip(2));
                        }
                        else if (Is(hasSymbol, "One", "Items"))
                        {
                            result = RewriteUsingShould(arguments[0], "HaveCount", ArgumentList(NumericLiteral(1), arguments.Skip(2)));
                        }
                        else if (Is(hasSymbol, "Count", "Zero") || Is(hasSymbol, "Length", "Zero"))
                        {
                            result = RewriteUsingShould(arguments[0], "BeEmpty", arguments.Skip(2));
                        }
                        else if (IsMethod(out var expected, isSymbol, "EqualTo"))
                        {
                            result = RewriteUsingShould(arguments[0], "Be", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "Not", "EqualTo"))
                        {
                            result = RewriteUsingShould(arguments[0], "NotBe", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, hasSymbol, "Count", "EqualTo") || IsMethod(out expected, hasSymbol, "Length", "EqualTo"))
                        {
                            result = RewriteUsingShould(arguments[0], "HaveCount", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, hasSymbol, "Exactly", "Items"))
                        {
                            result = RewriteUsingShould(arguments[0], "HaveCount", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, containsSymbol, "Substring"))
                        {
                            result = RewriteUsingShould(arguments[0], "Contain", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "Contain"))
                        {
                            result = RewriteUsingShould(arguments[0], "Contain", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "Not", "Contain"))
                        {
                            result = RewriteUsingShould(arguments[0], "NotContain", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "EndWith"))
                        {
                            result = RewriteUsingShould(arguments[0], "EndWith", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "Not", "EndWith") || IsMethod(out expected, doesSymbol, "Not", "EndsWith"))
                        {
                            result = RewriteUsingShould(arguments[0], "NotEndWith", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "StartWith"))
                        {
                            result = RewriteUsingShould(arguments[0], "StartWith", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, doesSymbol, "Not", "StartWith") || IsMethod(out expected, doesSymbol, "Not", "StartsWith"))
                        {
                            result = RewriteUsingShould(arguments[0], "NotStartWith", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsMethod(out expected, isSymbol, "InstanceOf"))
                        {
                            result = RewriteUsingShould(arguments[0], "BeOfType", ArgumentList(expected, arguments.Skip(2)));
                        }
                        else if (IsGenericMethod(out var instanceOfType, isSymbol, "InstanceOf"))
                        {
                            var exception = (TypeSyntax)generator.TypeExpression(instanceOfType);
                            result = RewriteUsingShould(arguments[0], "BeOfType", exception, arguments.Skip(2));
                        }
                        else if (IsMethod(out expected, throwsSymbol, "InstanceOf") && semanticModel.GetOperation(expected, cancellationToken) is ITypeOfOperation typeOfOperation)
                        {
                            var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                            var exception = (TypeSyntax)generator.TypeExpression(typeOfOperation.TypeOperand);
                            result = RewriteUsingShould(action, "Throw", exception, arguments.Skip(2));
                        }
                        else if (IsGenericMethod(out var type, throwsSymbol, "InstanceOf"))
                        {
                            var action = InvokeFluentActionsInvoking(compilation, generator, arguments[0].Expression);
                            var exception = (TypeSyntax)generator.TypeExpression(type);
                            result = RewriteUsingShould(action, "Throw", exception, arguments.Skip(2));
                        }
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

                            if (member.Instance == null)
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
                                if (argument != null || invocation.Arguments.Length != 1)
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

                                if (member.Instance == null)
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
                                if (typeArgument != null || invocation.TargetMethod.TypeArguments.Length == 0)
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

                                if (member.Instance == null)
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
                result = RewriteUsingShould(arguments[1], "BeEquivalentTo", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreNotEqualIgnoringCase")
            {
                result = RewriteUsingShould(arguments[1], "NotBeEquivalentTo", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "Contains")
            {
                result = RewriteUsingShould(arguments[1], "Contain", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "DoesNotContain")
            {
                result = RewriteUsingShould(arguments[1], "NotContain", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "DoesNotEndWith")
            {
                result = RewriteUsingShould(arguments[1], "NotEndWith", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "DoesNotMatch")
            {
                result = RewriteUsingShould(arguments[1], "NotMatchRegex", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "DoesNotStartWith")
            {
                result = RewriteUsingShould(arguments[1], "NotStartWith", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "EndsWith")
            {
                result = RewriteUsingShould(arguments[1], "EndWith", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsMatch")
            {
                result = RewriteUsingShould(arguments[1], "MatchRegex", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "StartsWith")
            {
                result = RewriteUsingShould(arguments[1], "StartWith", ArgumentList(arguments[0], arguments.Skip(2)));
            }
        }
        else if (method.ContainingType.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.CollectionAssert"), SymbolEqualityComparer.Default))
        {
            if (methodName is "AllItemsAreInstancesOfType")
            {
                result = RewriteUsingShould(arguments[0], "AllBeOfType", ArgumentList(arguments[1], arguments.Skip(2)));
            }
            else if (methodName is "AllItemsAreNotNull")
            {
                result = RewriteUsingShould(arguments[0], "NotContainNulls", arguments.Skip(1));
            }
            else if (methodName is "AllItemsAreUnique")
            {
                result = RewriteUsingShould(arguments[0], "OnlyHaveUniqueItems", arguments.Skip(1));
            }
            else if (methodName is "AreEqual" && (method.Parameters.Length <= 2 || !method.Parameters[2].Type.Equals(compilation.GetTypeByMetadataName("System.Collection.IComparer"), SymbolEqualityComparer.Default)))
            {
                result = RewriteUsingShould(arguments[1], "Equal", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreNotEqual" && (method.Parameters.Length <= 2 || !method.Parameters[2].Type.Equals(compilation.GetTypeByMetadataName("System.Collection.IComparer"), SymbolEqualityComparer.Default)))
            {
                result = RewriteUsingShould(arguments[1], "NotEqual", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreEquivalent")
            {
                result = RewriteUsingShould(arguments[1], "BeEquivalentTo", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "AreNotEquivalent")
            {
                result = RewriteUsingShould(arguments[1], "NotBeEquivalentTo", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "Contains")
            {
                result = RewriteUsingShould(arguments[0], "Contain", ArgumentList(arguments[1], arguments.Skip(2)));
            }
            else if (methodName is "DoesNotContain")
            {
                result = RewriteUsingShould(arguments[0], "NotContain", ArgumentList(arguments[1], arguments.Skip(2)));
            }
            else if (methodName is "IsNotEmpty")
            {
                result = RewriteUsingShould(arguments[0], "NotBeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsEmpty")
            {
                result = RewriteUsingShould(arguments[0], "BeEmpty", arguments.Skip(1));
            }
            else if (methodName is "IsSubsetOf")
            {
                result = RewriteUsingShould(arguments[1], "BeSubsetOf", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsNotSubsetOf")
            {
                result = RewriteUsingShould(arguments[1], "NotBeSubsetOf", ArgumentList(arguments[0], arguments.Skip(2)));
            }
            else if (methodName is "IsOrdered" && (method.Parameters.Length < 2 || !method.Parameters[1].Type.Equals(compilation.GetTypeByMetadataName("System.Collection.IComparer"), SymbolEqualityComparer.Default)))
            {
                result = RewriteUsingShould(arguments[0], "BeInAscendingOrder", arguments.Skip(1));
            }
        }

        // Not yet supported
        if (result == null)
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

    private static ITypeSymbol GetConstantTypeValue(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
    {
        var operation = semanticModel.GetOperation(expression, cancellationToken);
        if (operation is ITypeOfOperation typeOfOperation)
            return typeOfOperation.TypeOperand;

        return null;
    }

    private static ExpressionSyntax NumericLiteral(int value)
    {
        return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
    }

    private static IEnumerable<ArgumentSyntax> ArgumentList(ArgumentSyntax argument, IEnumerable<ArgumentSyntax> arguments)
    {
        return ArgumentList(argument.Expression, arguments);
    }

    private static IEnumerable<ArgumentSyntax> ArgumentList(ExpressionSyntax expression, IEnumerable<ArgumentSyntax> arguments)
    {
        var list = new List<ArgumentSyntax>();
        list.Add(Argument(expression));
        list.AddRange(arguments);
        return list;
    }

    private static SyntaxNode RewriteUsingShould(ArgumentSyntax subject, string methodName, IEnumerable<ArgumentSyntax> arguments)
    {
        return RewriteUsingShould(subject.Expression, methodName, genericParameterType: null, arguments);
    }

    private static SyntaxNode RewriteUsingShould(ExpressionSyntax subject, string methodName, IEnumerable<ArgumentSyntax> arguments)
    {
        return RewriteUsingShould(subject, methodName, genericParameterType: null, arguments);
    }

    private static SyntaxNode RewriteUsingShould(ArgumentSyntax subject, string methodName, TypeSyntax genericParameterType, IEnumerable<ArgumentSyntax> arguments)
    {
        return RewriteUsingShould(subject.Expression, methodName, genericParameterType, arguments);
    }

    private static SyntaxNode RewriteUsingShould(ExpressionSyntax subject, string methodName, TypeSyntax genericParameterType, IEnumerable<ArgumentSyntax> arguments)
    {
        var should = InvokeShould(subject);
        should = InvocationExpression(MemberAccessExpression(should, methodName, genericParameterType));
        should = should.AddArgumentListArguments(arguments.Select(arg => Argument(arg.Expression)).ToArray());
        return should;
    }

    private static SyntaxNode RewriteTrueOrFalse(SeparatedSyntaxList<ArgumentSyntax> originalArguments, string methodName)
    {
        return RewriteUsingShould(originalArguments[0], "Be" + methodName, originalArguments.Skip(1).ToArray());
    }

    private static InvocationExpressionSyntax InvokeShould(ExpressionSyntax expression)
    {
        return InvocationExpression(
                MemberAccessExpression(Parenthesize(expression), "Should"))
            .WithAdditionalAnnotations(Simplifier.Annotation);
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
        if (genericParameterType != null)
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

    private static ExpressionSyntax Parenthesize(ExpressionSyntax expression)
    {
        var withoutTrivia = expression.WithoutTrivia();
        var parenthesized = ParenthesizedExpression(withoutTrivia);
        return parenthesized.WithTriviaFrom(expression).WithAdditionalAnnotations(Simplifier.Annotation);
    }
}
