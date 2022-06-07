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
            .AddAllCodeFixers()
            .AddFluentAssertionsApi();
    }

    [Theory]
    [InlineData(@"true.Should().Be(true)", @"true.Should().BeTrue()")]
    [InlineData(@"true.Should().NotBe(false)", @"true.Should().BeTrue()")]

    [InlineData(@"true.Should().Be(false)", @"true.Should().BeFalse()")]
    [InlineData(@"true.Should().NotBe(true)", @"true.Should().BeFalse()")]
    public Task Assert_Tests(string code, string fix)
    {
        return CreateProjectBuilder()
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
