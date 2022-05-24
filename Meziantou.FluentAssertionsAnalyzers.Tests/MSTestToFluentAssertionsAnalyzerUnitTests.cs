using Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;
using TestHelper;
using Xunit;

namespace Meziantou.FluentAssertionsAnalyzers.Tests;

public sealed class MSTestToFluentAssertionsAnalyzerUnitTests
{
    private static ProjectBuilder CreateProjectBuilder()
    {
        return new ProjectBuilder()
            .WithTargetFramework(TargetFramework.Net6_0)
            .WithAnalyzer<AssertAnalyzer>(id: "MFA002")
            .AddMSTestApi()
            .AddFluentAssertionsApi();
    }

    private static Task Assert(string sourceCode)
    {
        return CreateProjectBuilder()
                  .WithSourceCode(sourceCode)
                  .ValidateAsync();
    }

    [Fact]
    public Task Assert_IsTrue_Boolean()
    {
        return Assert("""
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using FluentAssertions;

            class Test
            {
                public void MyTest()
                {
                    var value = false;
                    [|Assert.IsTrue(value)|];
                }
            }
            """);
    }

    [Fact]
    public Task Throw_AssertFailedException()
    {
        return Assert("""
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using FluentAssertions;

            class Test
            {
                public void MyTest()
                {
                    [|throw new AssertFailedException("test");|]
                }
            }
            """);
    }
}
