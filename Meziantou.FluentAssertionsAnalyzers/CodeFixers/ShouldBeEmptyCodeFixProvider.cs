using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Meziantou.FluentAssertionsAnalyzers.CodeFixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class ShouldBeEmptyCodeFixProvider : SimpleCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MFA006");

    public override string NewMethodName => "BeEmpty";

}
