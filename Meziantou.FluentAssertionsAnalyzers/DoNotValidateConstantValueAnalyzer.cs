using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.FluentAssertionsAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotValidateConstantValueAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ConstantValueRule = new(
      "MFA014",
      title: "Do not assert constant value",
      messageFormat: "Do not assert constant value",
      description: "",
      category: "Design",
      defaultSeverity: DiagnosticSeverity.Warning,
      isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(ConstantValueRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(context =>
        {
            var assertionExtensionsSymbol = context.Compilation.GetTypeByMetadataName("FluentAssertions.AssertionExtensions");
            if (assertionExtensionsSymbol is null)
                return;

            context.RegisterOperationAction(context => AnalyzeInvocation(context, assertionExtensionsSymbol), OperationKind.Invocation);
        });
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol assertionExtensionsSymbol)
    {
        var op = (IInvocationOperation)context.Operation;
        if (op.TargetMethod.Name is "Should" && op.Arguments.Length >= 1 && op.TargetMethod.ContainingType.Equals(assertionExtensionsSymbol, SymbolEqualityComparer.Default))
        {
            var constantValue = op.Arguments[0].Value.RemoveImplicitConversion().ConstantValue;
            if (!constantValue.HasValue)
                return;

            context.ReportDiagnostic(Diagnostic.Create(ConstantValueRule, op.Arguments[0].Syntax.GetLocation()));
        }
    }
}

