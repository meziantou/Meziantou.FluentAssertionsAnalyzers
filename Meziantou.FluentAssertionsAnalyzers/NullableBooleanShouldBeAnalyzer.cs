using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.FluentAssertionsAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullableBooleanShouldBeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ShouldBeTrueRule = new(
      "MFA008",
      title: "Simplify Should().Be(true)",
      messageFormat: "Simplify Should().Be(true)",
      description: "",
      category: "Design",
      defaultSeverity: DiagnosticSeverity.Info,
      isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ShouldBeFalseRule = new(
       "MFA009",
       title: "Simplify Should().Be(false)",
       messageFormat: "Simplify Should().Be(false)",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Info,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ShouldNotBeTrueRule = new(
      "MFA010",
      title: "Simplify Should().NotBe(true)",
      messageFormat: "Simplify Should().NotBe(true)",
      description: "",
      category: "Design",
      defaultSeverity: DiagnosticSeverity.Info,
      isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ShouldNotBeFalseRule = new(
       "MFA011",
       title: "Simplify Should().NotBe(false)",
       messageFormat: "Simplify Should().NotBe(false)",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Info,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ShouldBeNullRule = new(
      "MFA012",
      title: "Simplify Should().Be(null)",
      messageFormat: "Simplify Should().Be(null)",
      description: "",
      category: "Design",
      defaultSeverity: DiagnosticSeverity.Info,
      isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ShouldNotBeNullRule = new(
       "MFA013",
       title: "Simplify Should().NotBe(null)",
       messageFormat: "Simplify Should().NotBe(null)",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Info,
       isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        ShouldBeTrueRule, ShouldNotBeTrueRule,
        ShouldBeFalseRule, ShouldNotBeFalseRule,
        ShouldBeNullRule, ShouldNotBeNullRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(context =>
        {
            var booleanAssertionsOfTSymbol = context.Compilation.GetTypeByMetadataName("FluentAssertions.Primitives.NullableBooleanAssertions`1");
            var booleanAssertionsSymbol = context.Compilation.GetTypeByMetadataName("FluentAssertions.Primitives.NullableBooleanAssertions");
            if (booleanAssertionsSymbol is null || booleanAssertionsOfTSymbol is null)
                return;

            var booleanAssertionsFullSymbol = booleanAssertionsOfTSymbol.Construct(booleanAssertionsSymbol);

            context.RegisterOperationAction(context => AnalyzeInvocation(context, booleanAssertionsFullSymbol), OperationKind.Invocation);
        });
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol booleanAssertionsFullSymbol)
    {
        var op = (IInvocationOperation)context.Operation;
        if (op.TargetMethod.Name is "Be" && op.TargetMethod.ContainingType.Equals(booleanAssertionsFullSymbol, SymbolEqualityComparer.Default) && op.Arguments.Length >= 1)
        {
            var constantValue = op.Arguments[0].Value.RemoveImplicitConversion().ConstantValue;
            if (!constantValue.HasValue)
                return;

            if (constantValue.Value is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(ShouldBeNullRule, op.Syntax.GetLocation()));
            }
            else if (constantValue.Value is false)
            {
                context.ReportDiagnostic(Diagnostic.Create(ShouldBeFalseRule, op.Syntax.GetLocation()));
            }
            else if (constantValue.Value is true)
            {
                context.ReportDiagnostic(Diagnostic.Create(ShouldBeTrueRule, op.Syntax.GetLocation()));
            }
        }
        else if (op.TargetMethod.Name is "NotBe" && op.TargetMethod.ContainingType.Equals(booleanAssertionsFullSymbol, SymbolEqualityComparer.Default) && op.Arguments.Length >= 1)
        {
            var constantValue = op.Arguments[0].Value.RemoveImplicitConversion().ConstantValue;
            if (!constantValue.HasValue)
                return;

            if (constantValue.Value is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(ShouldNotBeNullRule, op.Syntax.GetLocation()));
            }
            else if (constantValue.Value is false)
            {
                context.ReportDiagnostic(Diagnostic.Create(ShouldNotBeFalseRule, op.Syntax.GetLocation()));
            }
            else if (constantValue.Value is true)
            {
                context.ReportDiagnostic(Diagnostic.Create(ShouldNotBeTrueRule, op.Syntax.GetLocation()));
            }
        }
    }
}

