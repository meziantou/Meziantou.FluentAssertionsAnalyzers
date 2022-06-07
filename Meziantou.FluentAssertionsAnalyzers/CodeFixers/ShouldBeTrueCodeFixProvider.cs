using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Meziantou.FluentAssertionsAnalyzers.CodeFixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class ShouldBeTrueCodeFixProvider : SimpleCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MFA004", "MFA008");

    public override string NewMethodName => "BeTrue";
}
