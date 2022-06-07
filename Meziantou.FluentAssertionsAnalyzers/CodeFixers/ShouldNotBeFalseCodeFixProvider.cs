using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Meziantou.FluentAssertionsAnalyzers.CodeFixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class ShouldNotBeFalseCodeFixProvider : SimpleCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MFA011");

    public override string NewMethodName => "NotBeFalse";
}
