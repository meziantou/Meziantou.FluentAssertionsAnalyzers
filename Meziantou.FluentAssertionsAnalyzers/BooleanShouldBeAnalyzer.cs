using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.FluentAssertionsAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BooleanShouldBeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ShouldBeTrueRule = new(
      "MFA004",
      title: "Simplify Should().Be(true)",
      messageFormat: "Simplify Should().Be(true)",
      description: "",
      category: "Design",
      defaultSeverity: DiagnosticSeverity.Info,
      isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ShouldBeFalseRule = new(
       "MFA005",
       title: "Simplify Should().Be(false)",
       messageFormat: "Simplify Should().Be(false)",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Info,
       isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ShouldBeTrueRule, ShouldBeFalseRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(context =>
        {
            var booleanAssertionsOfTSymbol = context.Compilation.GetTypeByMetadataName("FluentAssertions.Primitives.BooleanAssertions`1");
            var booleanAssertionsSymbol = context.Compilation.GetTypeByMetadataName("FluentAssertions.Primitives.BooleanAssertions");
            if (booleanAssertionsSymbol is null || booleanAssertionsOfTSymbol is null)
                return;

            var booleanAssertionsFullSymbol = booleanAssertionsOfTSymbol.Construct(booleanAssertionsSymbol);

            context.RegisterOperationAction(context => AnalyzeInvocation(context, booleanAssertionsFullSymbol), OperationKind.Invocation);
        });
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol booleanAssertionsFullSymbol)
    {
        var op = (IInvocationOperation)context.Operation;
        if (op.TargetMethod.Name is "Be" or "NotBe" && op.TargetMethod.ContainingType.Equals(booleanAssertionsFullSymbol, SymbolEqualityComparer.Default) && op.Arguments.Length >= 1 && op.Arguments[0].Value.ConstantValue.HasValue && op.Arguments[0].Value.ConstantValue.Value is bool constant)
        {
            switch ((op.TargetMethod.Name, constant))
            {
                case ("Be", false):
                case ("NotBe", true):
                    context.ReportDiagnostic(Diagnostic.Create(ShouldBeFalseRule, op.Syntax.GetLocation()));
                    break;

                case ("Be", true):
                case ("NotBe", false):
                    context.ReportDiagnostic(Diagnostic.Create(ShouldBeTrueRule, op.Syntax.GetLocation()));
                    break;
            }
        }
    }
}

