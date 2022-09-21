using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.FluentAssertionsAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AssertAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor XunitRule = new(
       "MFA001",
       title: "Replace Xunit assertion with Fluent Assertions equivalent",
       messageFormat: "Use FluentAssertions equivalent",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MSTestsRule = new(
       "MFA002",
       title: "Replace MSTests assertion with Fluent Assertions equivalent",
       messageFormat: "Use FluentAssertions equivalent",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NUnitRule = new(
       "MFA003",
       title: "Replace NUnit assertion with Fluent Assertions equivalent",
       messageFormat: "Use FluentAssertions equivalent",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(XunitRule, MSTestsRule, NUnitRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(ctx =>
        {
            if (ctx.Compilation.GetTypeByMetadataName("FluentAssertions.ObjectAssertionsExtensions") is null)
                return;

            if (ctx.Compilation.GetTypeByMetadataName("Xunit.Assert") is not null)
            {
                ctx.RegisterOperationAction(AnalyzeXunitInvocation, OperationKind.Invocation);
            }

            if (ctx.Compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.Assert") is not null)
            {
                ctx.RegisterOperationAction(AnalyzeMsTestInvocation, OperationKind.Invocation);
                ctx.RegisterOperationAction(AnalyzeMsTestThrow, OperationKind.Throw);
            }

            if (ctx.Compilation.GetTypeByMetadataName("NUnit.Framework.AssertionException") is not null)
            {
                ctx.RegisterOperationAction(AnalyzeNunitInvocation, OperationKind.Invocation);
                ctx.RegisterOperationAction(AnalyzeNunitThrow, OperationKind.Throw);
            }
        });
    }

    private void AnalyzeXunitInvocation(OperationAnalysisContext context)
    {
        var op = (IInvocationOperation)context.Operation;
        if (op.TargetMethod.ContainingType.Equals(context.Compilation.GetTypeByMetadataName("Xunit.Assert"), SymbolEqualityComparer.Default))
        {
            context.ReportDiagnostic(Diagnostic.Create(XunitRule, op.Syntax.GetLocation()));
        }
    }

    private void AnalyzeMsTestInvocation(OperationAnalysisContext context)
    {
        var op = (IInvocationOperation)context.Operation;
        if (IsMsTestAssertClass(context.Compilation, op.TargetMethod.ContainingType))
        {
            context.ReportDiagnostic(Diagnostic.Create(MSTestsRule, op.Syntax.GetLocation()));
        }
    }

    private void AnalyzeMsTestThrow(OperationAnalysisContext context)
    {
        var op = (IThrowOperation)context.Operation;
        if (op.Exception != null && op.Exception.RemoveImplicitConversion().Type.IsOrInheritsFrom(context.Compilation, "Microsoft.VisualStudio.TestTools.UnitTesting.UnitTestAssertException"))
        {
            context.ReportDiagnostic(Diagnostic.Create(MSTestsRule, op.Syntax.GetLocation()));
        }
    }

    private void AnalyzeNunitInvocation(OperationAnalysisContext context)
    {
        var op = (IInvocationOperation)context.Operation;
        if (IsNunitAssertClass(context.Compilation, op.TargetMethod.ContainingType))
        {
            context.ReportDiagnostic(Diagnostic.Create(NUnitRule, op.Syntax.GetLocation()));
        }
    }

    private void AnalyzeNunitThrow(OperationAnalysisContext context)
    {
        var op = (IThrowOperation)context.Operation;
        if (op.Exception != null && op.Exception.RemoveImplicitConversion().Type.IsOrInheritsFrom(context.Compilation, "NUnit.Framework.ResultStateException"))
        {
            context.ReportDiagnostic(Diagnostic.Create(NUnitRule, op.Syntax.GetLocation()));
        }
    }

    private static bool IsMsTestAssertClass(Compilation compilation, ITypeSymbol typeSymbol)
    {
        if (typeSymbol == null)
            return false;

        return typeSymbol.Equals(compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.Assert"), SymbolEqualityComparer.Default)
            || typeSymbol.Equals(compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert"), SymbolEqualityComparer.Default)
            || typeSymbol.Equals(compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert"), SymbolEqualityComparer.Default);
    }

    private static bool IsNunitAssertClass(Compilation compilation, ITypeSymbol typeSymbol)
    {
        if (typeSymbol == null)
            return false;

        return typeSymbol.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.Assert"), SymbolEqualityComparer.Default)
            || typeSymbol.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.CollectionAssert"), SymbolEqualityComparer.Default)
            || typeSymbol.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.DirectoryAssert"), SymbolEqualityComparer.Default)
            || typeSymbol.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.FileAssert"), SymbolEqualityComparer.Default)
            || typeSymbol.Equals(compilation.GetTypeByMetadataName("NUnit.Framework.StringAssert"), SymbolEqualityComparer.Default);
    }
}
