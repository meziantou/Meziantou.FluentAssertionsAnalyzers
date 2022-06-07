using Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;
using TestHelper;
using Xunit;

namespace Meziantou.FluentAssertionsAnalyzers.Tests;

public sealed class StringShouldBeAnalyzerUnitTests
{
    private static ProjectBuilder CreateProjectBuilder()
    {
        return new ProjectBuilder()
            .WithTargetFramework(TargetFramework.Net6_0)
            .WithAnalyzer<StringShouldBeAnalyzer>()
            .AddFluentAssertionsApi();
    }

    [Theory]
    [InlineData(@""""".Should().Be("""")", @""""".Should().BeEmpty()", typeof(StringShouldBeEmptyCodeFixProvider))]
    [InlineData(@""""".Should().Be(string.Empty)", @""""".Should().BeEmpty()", typeof(StringShouldBeEmptyCodeFixProvider))]

    [InlineData(@""""".Should().NotBe("""")", @""""".Should().NotBeEmpty()", typeof(StringShouldNotBeEmptyCodeFixProvider))]
    [InlineData(@""""".Should().NotBe(string.Empty)", @""""".Should().NotBeEmpty()", typeof(StringShouldNotBeEmptyCodeFixProvider))]
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
