﻿using Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;
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
            .AddAllCodeFixers()
            .AddFluentAssertionsApi();
    }

    [Theory]
    [InlineData(@""""".Should().Be("""")", @""""".Should().BeEmpty()")]
    [InlineData(@""""".Should().Be(string.Empty)", @""""".Should().BeEmpty()")]

    [InlineData(@""""".Should().NotBe("""")", @""""".Should().NotBeEmpty()")]
    [InlineData(@""""".Should().NotBe(string.Empty)", @""""".Should().NotBeEmpty()")]
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
