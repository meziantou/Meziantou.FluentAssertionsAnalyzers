using Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;
using TestHelper;
using Xunit;

namespace Meziantou.FluentAssertionsAnalyzers.Tests;
public sealed class DoNotValidateConstantValueAnalyzerTests
{
    private static ProjectBuilder CreateProjectBuilder()
    {
        return new ProjectBuilder()
            .WithTargetFramework(TargetFramework.Net6_0)
            .WithAnalyzer<DoNotValidateConstantValueAnalyzer>()
            .AddAllCodeFixers()
            .AddFluentAssertionsApi();
    }

    [Theory]
    [InlineData(@"[|0|].Should().Be(1)")]
    [InlineData(@"[|true|].Should().Be(false)")]
    [InlineData(@"[|""abc""|].Should().Be("""")")]
    public Task Assert_Tests(string code)
    {
        return CreateProjectBuilder()
                 .WithSourceCode($$"""
                      using FluentAssertions;

                      class Test
                      {
                          public void MyTest()
                          {
                              {{code}};
                          }
                      }
                      """)
                 .ValidateAsync();
    }
}
