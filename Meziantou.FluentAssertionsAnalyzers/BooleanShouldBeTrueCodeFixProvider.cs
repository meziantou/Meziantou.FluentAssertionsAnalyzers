using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Meziantou.FluentAssertionsAnalyzers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class BooleanShouldBeTrueCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MFA004");

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: true);
        if (nodeToFix == null)
            return;

        var title = "Use BeTrue()";
        var codeAction = CodeAction.Create(
            title,
            ct => Rewrite(context.Document, nodeToFix, context.CancellationToken),
            equivalenceKey: title);

        context.RegisterCodeFix(codeAction, context.Diagnostics);
    }

    private static async Task<Document> Rewrite(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var originalMethod = (InvocationExpressionSyntax)nodeToFix;
        var originalArguments = originalMethod.ArgumentList.Arguments;

        var newInvocation = originalMethod
            .WithExpression(((MemberAccessExpressionSyntax)originalMethod.Expression).WithName(IdentifierName("BeTrue")))
            .WithArgumentList(ArgumentList(originalArguments.RemoveAt(0)));

        editor.ReplaceNode(originalMethod, newInvocation);
        return editor.GetChangedDocument();
    }
}
