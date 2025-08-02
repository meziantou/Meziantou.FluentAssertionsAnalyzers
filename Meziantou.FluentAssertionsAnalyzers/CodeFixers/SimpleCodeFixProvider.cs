using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Meziantou.FluentAssertionsAnalyzers.CodeFixers;

public abstract class SimpleCodeFixProvider : CodeFixProvider
{
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public abstract string NewMethodName { get; }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: true);
        if (nodeToFix is null)
            return;

        var title = $"Use {NewMethodName}()";
        var codeAction = CodeAction.Create(
            title,
            ct => Rewrite(context.Document, nodeToFix, context.CancellationToken),
            equivalenceKey: title);

        context.RegisterCodeFix(codeAction, context.Diagnostics);
    }

    private async Task<Document> Rewrite(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var originalMethod = (InvocationExpressionSyntax)nodeToFix;
        var originalArguments = originalMethod.ArgumentList.Arguments;

        var newInvocation = originalMethod
            .WithExpression(((MemberAccessExpressionSyntax)originalMethod.Expression).WithName(IdentifierName(NewMethodName)))
            .WithArgumentList(ArgumentList(originalArguments.RemoveAt(0)));

        editor.ReplaceNode(originalMethod, newInvocation);
        return editor.GetChangedDocument();
    }
}
