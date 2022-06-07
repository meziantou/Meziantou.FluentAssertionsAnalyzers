using Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;
using TestHelper;
using Xunit;

namespace Meziantou.FluentAssertionsAnalyzers.Tests;

public sealed class BooleanShouldBeAnalyzerUnitTests
{
    private static ProjectBuilder CreateProjectBuilder()
    {
        return new ProjectBuilder()
            .WithTargetFramework(TargetFramework.Net6_0)
            .WithAnalyzer<BooleanShouldBeAnalyzer>()
            .AddFluentAssertionsApi();
    }

    [Theory]
    [InlineData(@"true.Should().Be(true)", @"true.Should().BeTrue()", typeof(BooleanShouldBeTrueCodeFixProvider))]
    [InlineData(@"true.Should().Be(false)", @"true.Should().BeFalse()", typeof(BooleanShouldBeFalseCodeFixProvider))]
    public Task Assert_Tests(string code, string fix, Type type)
    {
        return CreateProjectBuilder()
                  .WithCodeFixProvider(type)
                  .WithSourceCode($$"""
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = new int[1];
        [|{{code}}|];
    }
}
""")
                  .ShouldFixCodeWith($$"""
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = new int[1];
        {{fix}};
    }
}
""")
                  .ValidateAsync();
    }
}
