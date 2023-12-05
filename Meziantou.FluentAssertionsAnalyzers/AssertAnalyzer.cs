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

        context.RegisterCompilationStartAction(context =>
        {
            if (context.Compilation.GetTypeByMetadataName("FluentAssertions.ObjectAssertionsExtensions") is null)
                return;

            var analyzerContext = new AnalyzerContext(context.Compilation);

            if (analyzerContext.IsXUnitAvailable)
            {
                context.RegisterOperationAction(analyzerContext.AnalyzeXunitInvocation, OperationKind.Invocation);
            }

            if (analyzerContext.IsMSTestsAvailable)
            {
                context.RegisterOperationAction(analyzerContext.AnalyzeMsTestInvocation, OperationKind.Invocation);
                context.RegisterOperationAction(analyzerContext.AnalyzeMsTestThrow, OperationKind.Throw);
            }

            if (analyzerContext.IsNUnitAvailable)
            {
                context.RegisterOperationAction(analyzerContext.AnalyzeNunitInvocation, OperationKind.Invocation);
                context.RegisterOperationAction(analyzerContext.AnalyzeNunitDynamicInvocation, OperationKind.DynamicInvocation);
                context.RegisterOperationAction(analyzerContext.AnalyzeNunitThrow, OperationKind.Throw);
            }
        });
    }

    private sealed class AnalyzerContext(Compilation compilation)
    {
        private readonly INamedTypeSymbol _xunitAssertSymbol = compilation.GetTypeByMetadataName("Xunit.Assert");

        private readonly INamedTypeSymbol _msTestsAssertSymbol = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.Assert");
        private readonly INamedTypeSymbol _msTestsStringAssertSymbol = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert");
        private readonly INamedTypeSymbol _msTestsCollectionAssertSymbol = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert");
        private readonly INamedTypeSymbol _msTestsUnitTestAssertExceptionSymbol = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.UnitTestAssertException");

        private readonly INamedTypeSymbol _nunitAssertionExceptionSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.AssertionException");
        private readonly INamedTypeSymbol _nunitAssertSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
        private readonly INamedTypeSymbol _nunitCollectionAssertSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.CollectionAssert");
        private readonly INamedTypeSymbol _nunitDirectoryAssertSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.DirectoryAssert");
        private readonly INamedTypeSymbol _nunitFileAssertSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.FileAssert");
        private readonly INamedTypeSymbol _nunitStringAssertSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.StringAssert");
        private readonly INamedTypeSymbol _nunitClassicAssertSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.Legacy.ClassicAssert");
        private readonly INamedTypeSymbol _nunitResultStateExceptionSymbol = compilation.GetTypeByMetadataName("NUnit.Framework.ResultStateException");

        public bool IsMSTestsAvailable => _msTestsAssertSymbol is not null;
        public bool IsNUnitAvailable => _nunitAssertionExceptionSymbol is not null;
        public bool IsXUnitAvailable => _xunitAssertSymbol is not null;

        private static readonly char[] SymbolsSeparators = [';'];

        private bool IsMethodExcluded(AnalyzerOptions options, IInvocationOperation operation)
        {
            var location = operation.Syntax.GetLocation().SourceTree;
            if (location is null)
                return false;

            var fileOptions = options.AnalyzerConfigOptionsProvider.GetOptions(location);
            if (fileOptions is null)
                return false;

            if (!fileOptions.TryGetValue("mfa_excluded_methods", out var symbolDocumentationIds))
                return false;

            var parts = symbolDocumentationIds.Split(SymbolsSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var symbols = DocumentationCommentId.GetSymbolsForDeclarationId(part, compilation);
                foreach (var symbol in symbols)
                {
                    if (SymbolEqualityComparer.Default.Equals(symbol, operation.TargetMethod))
                        return true;
                }
            }

            return false;
        }

        public void AnalyzeXunitInvocation(OperationAnalysisContext context)
        {
            var op = (IInvocationOperation)context.Operation;
            if (op.TargetMethod.ContainingType.Equals(_xunitAssertSymbol, SymbolEqualityComparer.Default) && !IsMethodExcluded(context.Options, op))
            {
                context.ReportDiagnostic(Diagnostic.Create(XunitRule, op.Syntax.GetLocation()));
            }
        }

        public void AnalyzeMsTestInvocation(OperationAnalysisContext context)
        {
            var op = (IInvocationOperation)context.Operation;
            if (IsMsTestAssertClass(op.TargetMethod.ContainingType) && !IsMethodExcluded(context.Options, op))
            {
                context.ReportDiagnostic(Diagnostic.Create(MSTestsRule, op.Syntax.GetLocation()));
            }
        }

        public void AnalyzeMsTestThrow(OperationAnalysisContext context)
        {
            var op = (IThrowOperation)context.Operation;
            if (op.Exception is not null && op.Exception.RemoveImplicitConversion().Type.IsOrInheritsFrom(_msTestsUnitTestAssertExceptionSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(MSTestsRule, op.Syntax.GetLocation()));
            }
        }

        public void AnalyzeNunitInvocation(OperationAnalysisContext context)
        {
            var op = (IInvocationOperation)context.Operation;
            if (IsNunitAssertClass(op.TargetMethod.ContainingType) && !IsMethodExcluded(context.Options, op))
            {
                if (op.TargetMethod.Name is "Inconclusive" or "Ignore" && op.TargetMethod.ContainingType.Equals(_nunitAssertSymbol, SymbolEqualityComparer.Default))
                    return;

                context.ReportDiagnostic(Diagnostic.Create(NUnitRule, op.Syntax.GetLocation()));
            }
        }

        public void AnalyzeNunitDynamicInvocation(OperationAnalysisContext context)
        {
            var op = (IDynamicInvocationOperation)context.Operation;

            if (op.Arguments.Length < 2)
                return;

            var containingType = ((op.Arguments[1]
                        .Parent as IDynamicInvocationOperation)?
                        .Operation as IDynamicMemberReferenceOperation)?
                        .ContainingType;
            if (IsNunitAssertClass(containingType))
            {
                context.ReportDiagnostic(Diagnostic.Create(NUnitRule, op.Syntax.GetLocation()));
            }
        }

        public void AnalyzeNunitThrow(OperationAnalysisContext context)
        {
            var op = (IThrowOperation)context.Operation;
            if (op.Exception is not null && op.Exception.RemoveImplicitConversion().Type.IsOrInheritsFrom(_nunitResultStateExceptionSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(NUnitRule, op.Syntax.GetLocation()));
            }
        }

        private bool IsMsTestAssertClass(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is null)
                return false;

            return typeSymbol.Equals(_msTestsAssertSymbol, SymbolEqualityComparer.Default)
                || typeSymbol.Equals(_msTestsStringAssertSymbol, SymbolEqualityComparer.Default)
                || typeSymbol.Equals(_msTestsCollectionAssertSymbol, SymbolEqualityComparer.Default);
        }

        private bool IsNunitAssertClass(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is null)
                return false;

            return typeSymbol.Equals(_nunitAssertSymbol, SymbolEqualityComparer.Default)
                || typeSymbol.Equals(_nunitCollectionAssertSymbol, SymbolEqualityComparer.Default)
                || typeSymbol.Equals(_nunitDirectoryAssertSymbol, SymbolEqualityComparer.Default)
                || typeSymbol.Equals(_nunitFileAssertSymbol, SymbolEqualityComparer.Default)
                || typeSymbol.Equals(_nunitStringAssertSymbol, SymbolEqualityComparer.Default)
                || typeSymbol.Equals(_nunitClassicAssertSymbol, SymbolEqualityComparer.Default);
        }
    }
}
