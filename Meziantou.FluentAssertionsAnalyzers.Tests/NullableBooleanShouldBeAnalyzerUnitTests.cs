using Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;
using TestHelper;
using Xunit;

namespace Meziantou.FluentAssertionsAnalyzers.Tests;

public sealed class NullableBooleanShouldBeAnalyzerUnitTests
{
    private static ProjectBuilder CreateProjectBuilder()
    {
        return new ProjectBuilder()
            .WithTargetFramework(TargetFramework.Net6_0)
            .WithAnalyzer<NullableBooleanShouldBeAnalyzer>()
            .AddAllCodeFixers()
            .AddFluentAssertionsApi();
    }

    [Theory]
    [InlineData(@"subject.Should().Be(null)", @"subject.Should().NotHaveValue()")]
    [InlineData(@"subject.Should().NotBe(null)", @"subject.Should().HaveValue()")]

    [InlineData(@"subject.Should().Be(false)", @"subject.Should().BeFalse()")]
    [InlineData(@"subject.Should().Be(true)", @"subject.Should().BeTrue()")]

    [InlineData(@"subject.Should().NotBe(false)", @"subject.Should().NotBeFalse()")]
    [InlineData(@"subject.Should().NotBe(true)", @"subject.Should().NotBeTrue()")]
    public Task Assert_Tests(string code, string fix)
    {
        return CreateProjectBuilder()
                  .WithSourceCode($$"""
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        bool? subject = false;
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
        bool? subject = false;
        {{fix}};
    }
}
""")
                  .ValidateAsync();
    }
}
