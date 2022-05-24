using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;

internal sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private readonly IDictionary<string, string> _values;

    public TestAnalyzerConfigOptionsProvider(IDictionary<string, string> values)
    {
        _values = values ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    public override AnalyzerConfigOptions GlobalOptions => new TestAnalyzerConfigOptions(_values);
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new TestAnalyzerConfigOptions(_values);
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new TestAnalyzerConfigOptions(_values);

    private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly IDictionary<string, string> _values;

        public TestAnalyzerConfigOptions(IDictionary<string, string> values)
        {
            _values = values;
        }

        public override bool TryGetValue(string key, out string value)
        {
            return _values.TryGetValue(key, out value);
        }
    }
}
