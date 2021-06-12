using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace XUnitToFluentAssertionsAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class XUnitAssertAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor s_rule = new(
           "XFA001",
           title: "Use FluentAssertions equivalent",
           messageFormat: "Use FluentAssertions equivalent",
           description: "",
           category: "Design",
           defaultSeverity: DiagnosticSeverity.Info,
           isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        }

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var op = (IInvocationOperation)context.Operation;
            if (!op.TargetMethod.ContainingType.Equals(context.Compilation.GetTypeByMetadataName("Xunit.Assert"), SymbolEqualityComparer.Default))
                return;

            context.ReportDiagnostic(Diagnostic.Create(s_rule, op.Syntax.GetLocation()));
        }
    }
}
