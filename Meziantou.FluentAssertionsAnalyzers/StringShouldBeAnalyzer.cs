using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.FluentAssertionsAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StringShouldBeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ShouldBeEmptyRule = new(
      "MFA006",
      title: "Simplify Should().Be(string.Empty)",
      messageFormat: "Simplify Should().Be(string.Empty)",
      description: "",
      category: "Design",
      defaultSeverity: DiagnosticSeverity.Info,
      isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ShouldNotBeEmptyRule = new(
      "MFA007",
      title: "Simplify Should().NotBe(string.Empty)",
      messageFormat: "Simplify Should().NotBe(string.Empty)",
      description: "",
      category: "Design",
      defaultSeverity: DiagnosticSeverity.Info,
      isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ShouldBeEmptyRule, ShouldNotBeEmptyRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(ctx =>
        {
            if (ctx.Compilation.GetTypeByMetadataName("FluentAssertions.Primitives.StringAssertions") is null)
                return;

            ctx.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        });
    }

    private void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var assertionsOfTSymbol = context.Compilation.GetTypeByMetadataName("FluentAssertions.Primitives.StringAssertions`1");
        var assertionsSymbol = context.Compilation.GetTypeByMetadataName("FluentAssertions.Primitives.StringAssertions");
        if (assertionsSymbol is null || assertionsOfTSymbol is null)
            return;

        var assertionsFullSymbol = assertionsOfTSymbol.Construct(assertionsSymbol);

        var op = (IInvocationOperation)context.Operation;
        if (op.TargetMethod.Name == "Be" && op.TargetMethod.ContainingType.Equals(assertionsFullSymbol, SymbolEqualityComparer.Default) && op.Arguments.Length >= 1)
        {
            var value = op.Arguments[0].Value;
            if (value.ConstantValue.HasValue && value.ConstantValue.Value is "")
            {
                context.ReportDiagnostic(Diagnostic.Create(ShouldBeEmptyRule, op.Syntax.GetLocation()));
            }
            else if (value is IMemberReferenceOperation member && member.Member.Name == "Empty" && member.Member.ContainingType.SpecialType == SpecialType.System_String)
            {
                context.ReportDiagnostic(Diagnostic.Create(ShouldBeEmptyRule, op.Syntax.GetLocation()));
            }
        }
        else if (op.TargetMethod.Name == "NotBe" && op.TargetMethod.ContainingType.Equals(assertionsFullSymbol, SymbolEqualityComparer.Default) && op.Arguments.Length >= 1)
        {
            var value = op.Arguments[0].Value;
            if (value.ConstantValue.HasValue && value.ConstantValue.Value is "")
            {
                context.ReportDiagnostic(Diagnostic.Create(ShouldNotBeEmptyRule, op.Syntax.GetLocation()));
            }
            else if (value is IMemberReferenceOperation member && member.Member.Name == "Empty" && member.Member.ContainingType.SpecialType == SpecialType.System_String)
            {
                context.ReportDiagnostic(Diagnostic.Create(ShouldNotBeEmptyRule, op.Syntax.GetLocation()));
            }
        }
    }
}

