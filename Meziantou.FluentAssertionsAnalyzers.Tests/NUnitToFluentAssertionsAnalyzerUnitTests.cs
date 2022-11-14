using Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;
using TestHelper;
using Xunit;

namespace Meziantou.FluentAssertionsAnalyzers.Tests;

public sealed class NUnitToFluentAssertionsAnalyzerUnitTests
{
    private static ProjectBuilder CreateProjectBuilder()
    {
        return new ProjectBuilder()
            .WithTargetFramework(TargetFramework.Net6_0)
            .WithAnalyzer<AssertAnalyzer>(id: "MFA003")
            .AddAllCodeFixers()
            .AddNUnitApi()
            .AddFluentAssertionsApi();
    }

    private static Task Assert(string sourceCode, string fix)
    {
        return CreateProjectBuilder()
                  .WithSourceCode(sourceCode)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
    }

    [Fact]
    public Task Assert_Pass()
    {
        return Assert(
"""
using NUnit.Framework;

class Test
{
    public void MyTest()
    {
        [|Assert.Pass()|];
    }
}
""",
"""
using NUnit.Framework;

class Test
{
    public void MyTest()
    {
        return;
    }
}
""");
    }
    
    [Fact]
    public Task Rethrow()
    {
        return CreateProjectBuilder()
          .WithSourceCode("""
using NUnit.Framework;

class Test
{
    public void MyTest()
    {
        try
        {
        }
        catch
        {
	        throw;
        }
    }
}
""")
          .ValidateAsync();
    }

    [Theory]

    [InlineData(@"Assert.AreEqual(false, true)", @"true.Should().Be(false)")]
    [InlineData(@"Assert.AreEqual(false, true, ""because"")", @"true.Should().Be(false, ""because"")")]
    [InlineData(@"Assert.AreEqual(false, true, ""because"", 1, 2)", @"true.Should().Be(false, ""because"", 1, 2)")]

    [InlineData(@"Assert.AreEqual(""expected"", ""actual"")", @"""actual"".Should().Be(""expected"")")]
    [InlineData(@"Assert.AreEqual(""expected"", ""actual"", ""because"")", @"""actual"".Should().Be(""expected"", ""because"")")]
    [InlineData(@"Assert.AreEqual(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().Be(""expected"", ""because"", 1, 2)")]

    [InlineData(@"Assert.AreEqual(0d, 1d, delta: 2d)", @"1d.Should().BeApproximately(0d, 2d)")]
    [InlineData(@"Assert.AreEqual(0d, 1d, delta: 2d, ""because"")", @"1d.Should().BeApproximately(0d, 2d, ""because"")")]
    [InlineData(@"Assert.AreEqual(0d, 1d, delta: 2d, ""because"", 1, 2)", @"1d.Should().BeApproximately(0d, 2d, ""because"", 1, 2)")]

    [InlineData(@"Assert.AreEqual(0d, (double?)null, delta: 2d)", @"((double?)null).Should().BeApproximately(0d, 2d)")]
    [InlineData(@"Assert.AreEqual(0d, (double?)null, delta: 2d, ""because"")", @"((double?)null).Should().BeApproximately(0d, 2d, ""because"")")]
    [InlineData(@"Assert.AreEqual(0d, (double?)null, delta: 2d, ""because"", 1, 2)", @"((double?)null).Should().BeApproximately(0d, 2d, ""because"", 1, 2)")]

    [InlineData(@"Assert.AreNotEqual(""expected"", ""actual"")", @"""actual"".Should().NotBe(""expected"")")]
    [InlineData(@"Assert.AreNotEqual(""expected"", ""actual"", ""because"")", @"""actual"".Should().NotBe(""expected"", ""because"")")]
    [InlineData(@"Assert.AreNotEqual(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().NotBe(""expected"", ""because"", 1, 2)")]

    [InlineData(@"Assert.AreNotSame(""expected"", ""actual"")", @"""actual"".Should().NotBeSameAs(""expected"")")]
    [InlineData(@"Assert.AreNotSame(""expected"", ""actual"", ""because"")", @"""actual"".Should().NotBeSameAs(""expected"", ""because"")")]
    [InlineData(@"Assert.AreNotSame(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().NotBeSameAs(""expected"", ""because"", 1, 2)")]

    [InlineData(@"Assert.AreSame(""expected"", ""actual"")", @"""actual"".Should().BeSameAs(""expected"")")]
    [InlineData(@"Assert.AreSame(""expected"", ""actual"", ""because"")", @"""actual"".Should().BeSameAs(""expected"", ""because"")")]
    [InlineData(@"Assert.AreSame(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().BeSameAs(""expected"", ""because"", 1, 2)")]

    [InlineData(@"Assert.Catch(() => { })", @"FluentActions.Invoking(() => { }).Should().Throw<System.Exception>()")]
    [InlineData(@"Assert.Catch(() => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().Throw<System.Exception>(""because"")")]
    [InlineData(@"Assert.Catch(() => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().Throw<System.Exception>(""because"", 1, 2)")]

    [InlineData(@"Assert.Catch(typeof(System.ArgumentException), () => { })", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>()")]
    [InlineData(@"Assert.Catch(typeof(System.ArgumentException), () => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>(""because"")")]
    [InlineData(@"Assert.Catch(typeof(System.ArgumentException), () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"Assert.Catch<System.ArgumentException>(() => { })", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>()")]
    [InlineData(@"Assert.Catch<System.ArgumentException>(() => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>(""because"")")]
    [InlineData(@"Assert.Catch<System.ArgumentException>(() => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"Assert.CatchAsync(async () => { })", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.Exception>()")]
    [InlineData(@"Assert.CatchAsync(async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.Exception>(""because"")")]
    [InlineData(@"Assert.CatchAsync(async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.Exception>(""because"", 1, 2)")]

    [InlineData(@"Assert.CatchAsync(typeof(System.ArgumentException), async () => { })", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>()")]
    [InlineData(@"Assert.CatchAsync(typeof(System.ArgumentException), async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>(""because"")")]
    [InlineData(@"Assert.CatchAsync(typeof(System.ArgumentException), async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"Assert.CatchAsync<System.ArgumentException>(async () => { })", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>()")]
    [InlineData(@"Assert.CatchAsync<System.ArgumentException>(async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>(""because"")")]
    [InlineData(@"Assert.CatchAsync<System.ArgumentException>(async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"Assert.DoesNotThrow(() => { })", @"FluentActions.Invoking(() => { }).Should().NotThrow()")]
    [InlineData(@"Assert.DoesNotThrow(() => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().NotThrow(""because"")")]
    [InlineData(@"Assert.DoesNotThrow(() => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().NotThrow(""because"", 1, 2)")]

    [InlineData(@"Assert.DoesNotThrowAsync(async () => { })", @"FluentActions.Invoking(async () => { }).Should().NotThrowAsync()")]
    [InlineData(@"Assert.DoesNotThrowAsync(async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().NotThrowAsync(""because"")")]
    [InlineData(@"Assert.DoesNotThrowAsync(async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().NotThrowAsync(""because"", 1, 2)")]

    [InlineData(@"Assert.Throws(typeof(System.ArgumentException), () => { })", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>()")]
    [InlineData(@"Assert.Throws(typeof(System.ArgumentException), () => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>(""because"")")]
    [InlineData(@"Assert.Throws(typeof(System.ArgumentException), () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"Assert.Throws<System.ArgumentException>(() => { })", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>()")]
    [InlineData(@"Assert.Throws<System.ArgumentException>(() => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>(""because"")")]
    [InlineData(@"Assert.Throws<System.ArgumentException>(() => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"Assert.ThrowsAsync(typeof(System.ArgumentException), async () => { })", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>()")]
    [InlineData(@"Assert.ThrowsAsync(typeof(System.ArgumentException), async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>(""because"")")]
    [InlineData(@"Assert.ThrowsAsync(typeof(System.ArgumentException), async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"Assert.ThrowsAsync<System.ArgumentException>(async () => { })", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>()")]
    [InlineData(@"Assert.ThrowsAsync<System.ArgumentException>(async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>(""because"")")]
    [InlineData(@"Assert.ThrowsAsync<System.ArgumentException>(async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"Assert.False((bool?)false)", @"((bool?)false).Should().BeFalse()")]
    [InlineData(@"Assert.False((bool?)false, ""because"")", @"((bool?)false).Should().BeFalse(""because"")")]
    [InlineData(@"Assert.False((bool?)false, ""because"", 0, 1)", @"((bool?)false).Should().BeFalse(""because"", 0, 1)")]
    [InlineData(@"Assert.False(false)", @"false.Should().BeFalse()")]
    [InlineData(@"Assert.False(false, ""because"")", @"false.Should().BeFalse(""because"")")]
    [InlineData(@"Assert.False(false, ""because"", 1, 2)", @"false.Should().BeFalse(""because"", 1, 2)")]

    [InlineData(@"Assert.Greater(0, 1)", @"1.Should().BeGreaterThan(0)")]
    [InlineData(@"Assert.Greater(0, 1, ""because"")", @"1.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"Assert.Greater(0, 1, ""because"", 2, 3)", @"1.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Greater(0, 1u)", @"1u.Should().BeGreaterThan(0)")]
    [InlineData(@"Assert.Greater(0, 1u, ""because"")", @"1u.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"Assert.Greater(0, 1u, ""because"", 2, 3)", @"1u.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Greater(0, 1l)", @"1l.Should().BeGreaterThan(0)")]
    [InlineData(@"Assert.Greater(0, 1l, ""because"")", @"1l.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"Assert.Greater(0, 1l, ""because"", 2, 3)", @"1l.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Greater(0, 1ul)", @"1ul.Should().BeGreaterThan(0)")]
    [InlineData(@"Assert.Greater(0, 1ul, ""because"")", @"1ul.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"Assert.Greater(0, 1ul, ""because"", 2, 3)", @"1ul.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Greater(0, 1m)", @"1m.Should().BeGreaterThan(0)")]
    [InlineData(@"Assert.Greater(0, 1m, ""because"")", @"1m.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"Assert.Greater(0, 1m, ""because"", 2, 3)", @"1m.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Greater(0, 1f)", @"1f.Should().BeGreaterThan(0)")]
    [InlineData(@"Assert.Greater(0, 1f, ""because"")", @"1f.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"Assert.Greater(0, 1f, ""because"", 2, 3)", @"1f.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Greater(0, 1d)", @"1d.Should().BeGreaterThan(0)")]
    [InlineData(@"Assert.Greater(0, 1d, ""because"")", @"1d.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"Assert.Greater(0, 1d, ""because"", 2, 3)", @"1d.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.GreaterOrEqual(0, 1)", @"1.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1, ""because"")", @"1.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1, ""because"", 2, 3)", @"1.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.GreaterOrEqual(0, 1u)", @"1u.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1u, ""because"")", @"1u.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1u, ""because"", 2, 3)", @"1u.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.GreaterOrEqual(0, 1l)", @"1l.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1l, ""because"")", @"1l.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1l, ""because"", 2, 3)", @"1l.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.GreaterOrEqual(0, 1ul)", @"1ul.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1ul, ""because"")", @"1ul.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1ul, ""because"", 2, 3)", @"1ul.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.GreaterOrEqual(0, 1m)", @"1m.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1m, ""because"")", @"1m.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1m, ""because"", 2, 3)", @"1m.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.GreaterOrEqual(0, 1f)", @"1f.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1f, ""because"")", @"1f.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1f, ""because"", 2, 3)", @"1f.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.GreaterOrEqual(0, 1d)", @"1d.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1d, ""because"")", @"1d.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.GreaterOrEqual(0, 1d, ""because"", 2, 3)", @"1d.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.IsEmpty("""")", @""""".Should().BeEmpty()")]
    [InlineData(@"Assert.IsEmpty("""", ""because"")", @""""".Should().BeEmpty(""because"")")]
    [InlineData(@"Assert.IsEmpty("""", ""because"", 1, 2)", @""""".Should().BeEmpty(""because"", 1, 2)")]

    [InlineData(@"Assert.IsEmpty(new[] { 0 })", @"(new[] { 0 }).Should().BeEmpty()")]
    [InlineData(@"Assert.IsEmpty(new[] { 0 }, ""because"")", @"(new[] { 0 }).Should().BeEmpty(""because"")")]
    [InlineData(@"Assert.IsEmpty(new[] { 0 }, ""because"", 1, 2)", @"(new[] { 0 }).Should().BeEmpty(""because"", 1, 2)")]

    [InlineData(@"Assert.IsInstanceOf(typeof(string), """")", @""""".Should().BeOfType(typeof(string))")]
    [InlineData(@"Assert.IsInstanceOf(typeof(string), """", ""because"")", @""""".Should().BeOfType(typeof(string), ""because"")")]
    [InlineData(@"Assert.IsInstanceOf(typeof(string), """", ""because"", 1, 2)", @""""".Should().BeOfType(typeof(string), ""because"", 1, 2)")]

    [InlineData(@"Assert.That("""", Is.InstanceOf(typeof(string)))", @""""".Should().BeOfType(typeof(string))")]
    [InlineData(@"Assert.That("""", Is.InstanceOf(typeof(string)), ""because"")", @""""".Should().BeOfType(typeof(string), ""because"")")]
    [InlineData(@"Assert.That("""", Is.InstanceOf(typeof(string)), ""because"", 1, 2)", @""""".Should().BeOfType(typeof(string), ""because"", 1, 2)")]

    [InlineData(@"Assert.IsInstanceOf<string>("""")", @""""".Should().BeOfType<string>()")]
    [InlineData(@"Assert.IsInstanceOf<string>("""", ""because"")", @""""".Should().BeOfType<string>(""because"")")]
    [InlineData(@"Assert.IsInstanceOf<string>("""", ""because"", 1, 2)", @""""".Should().BeOfType<string>(""because"", 1, 2)")]

    [InlineData(@"Assert.That("""", Is.InstanceOf<string>())", @""""".Should().BeOfType<string>()")]
    [InlineData(@"Assert.That("""", Is.InstanceOf<string>(), ""because"")", @""""".Should().BeOfType<string>(""because"")")]
    [InlineData(@"Assert.That("""", Is.InstanceOf<string>(), ""because"", 1, 2)", @""""".Should().BeOfType<string>(""because"", 1, 2)")]

    [InlineData(@"Assert.IsNotInstanceOf(typeof(string), """")", @""""".Should().NotBeOfType(typeof(string))")]
    [InlineData(@"Assert.IsNotInstanceOf(typeof(string), """", ""because"")", @""""".Should().NotBeOfType(typeof(string), ""because"")")]
    [InlineData(@"Assert.IsNotInstanceOf(typeof(string), """", ""because"", 1, 2)", @""""".Should().NotBeOfType(typeof(string), ""because"", 1, 2)")]

    [InlineData(@"Assert.IsNotInstanceOf<string>("""")", @""""".Should().NotBeOfType<string>()")]
    [InlineData(@"Assert.IsNotInstanceOf<string>("""", ""because"")", @""""".Should().NotBeOfType<string>(""because"")")]
    [InlineData(@"Assert.IsNotInstanceOf<string>("""", ""because"", 1, 2)", @""""".Should().NotBeOfType<string>(""because"", 1, 2)")]

    [InlineData(@"Assert.IsNotEmpty("""")", @""""".Should().NotBeEmpty()")]
    [InlineData(@"Assert.IsNotEmpty("""", ""because"")", @""""".Should().NotBeEmpty(""because"")")]
    [InlineData(@"Assert.IsNotEmpty("""", ""because"", 1, 2)", @""""".Should().NotBeEmpty(""because"", 1, 2)")]

    [InlineData(@"Assert.IsNotEmpty(new[] { 0 })", @"(new[] { 0 }).Should().NotBeEmpty()")]
    [InlineData(@"Assert.IsNotEmpty(new[] { 0 }, ""because"")", @"(new[] { 0 }).Should().NotBeEmpty(""because"")")]
    [InlineData(@"Assert.IsNotEmpty(new[] { 0 }, ""because"", 1, 2)", @"(new[] { 0 }).Should().NotBeEmpty(""because"", 1, 2)")]

    [InlineData(@"Assert.IsFalse((bool?)false)", @"((bool?)false).Should().BeFalse()")]
    [InlineData(@"Assert.IsFalse((bool?)false, ""because"")", @"((bool?)false).Should().BeFalse(""because"")")]
    [InlineData(@"Assert.IsFalse((bool?)false, ""because"", 0, 1)", @"((bool?)false).Should().BeFalse(""because"", 0, 1)")]
    [InlineData(@"Assert.IsFalse(false)", @"false.Should().BeFalse()")]
    [InlineData(@"Assert.IsFalse(false, ""because"")", @"false.Should().BeFalse(""because"")")]
    [InlineData(@"Assert.IsFalse(false, ""because"", 1, 2)", @"false.Should().BeFalse(""because"", 1, 2)")]

    [InlineData(@"Assert.IsNaN(1d)", @"1d.Should().Be(double.NaN)")]
    [InlineData(@"Assert.IsNaN((double?)1d)", @"((double?)1d).Should().Be(double.NaN)")]

    [InlineData(@"Assert.IsNotNull("""")", @""""".Should().NotBeNull()")]
    [InlineData(@"Assert.IsNotNull("""", ""because"")", @""""".Should().NotBeNull(""because"")")]
    [InlineData(@"Assert.IsNotNull("""", ""because"", 1, 2)", @""""".Should().NotBeNull(""because"", 1, 2)")]

    [InlineData(@"Assert.IsNull("""")", @""""".Should().BeNull()")]
    [InlineData(@"Assert.IsNull("""", ""because"")", @""""".Should().BeNull(""because"")")]
    [InlineData(@"Assert.IsNull("""", ""because"", 1, 2)", @""""".Should().BeNull(""because"", 1, 2)")]

    [InlineData(@"Assert.IsTrue((bool?)false)", @"((bool?)false).Should().BeTrue()")]
    [InlineData(@"Assert.IsTrue((bool?)false, ""because"")", @"((bool?)false).Should().BeTrue(""because"")")]
    [InlineData(@"Assert.IsTrue((bool?)false, ""because"", 0, 1)", @"((bool?)false).Should().BeTrue(""because"", 0, 1)")]
    [InlineData(@"Assert.IsTrue(false)", @"false.Should().BeTrue()")]
    [InlineData(@"Assert.IsTrue(false, ""because"")", @"false.Should().BeTrue(""because"")")]
    [InlineData(@"Assert.IsTrue(false, ""because"", 1, 2)", @"false.Should().BeTrue(""because"", 1, 2)")]

    [InlineData(@"Assert.Less(0, 1)", @"1.Should().BeLessThan(0)")]
    [InlineData(@"Assert.Less(0, 1, ""because"")", @"1.Should().BeLessThan(0, ""because"")")]
    [InlineData(@"Assert.Less(0, 1, ""because"", 2, 3)", @"1.Should().BeLessThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Less(0, 1u)", @"1u.Should().BeLessThan(0)")]
    [InlineData(@"Assert.Less(0, 1u, ""because"")", @"1u.Should().BeLessThan(0, ""because"")")]
    [InlineData(@"Assert.Less(0, 1u, ""because"", 2, 3)", @"1u.Should().BeLessThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Less(0, 1l)", @"1l.Should().BeLessThan(0)")]
    [InlineData(@"Assert.Less(0, 1l, ""because"")", @"1l.Should().BeLessThan(0, ""because"")")]
    [InlineData(@"Assert.Less(0, 1l, ""because"", 2, 3)", @"1l.Should().BeLessThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Less(0, 1ul)", @"1ul.Should().BeLessThan(0)")]
    [InlineData(@"Assert.Less(0, 1ul, ""because"")", @"1ul.Should().BeLessThan(0, ""because"")")]
    [InlineData(@"Assert.Less(0, 1ul, ""because"", 2, 3)", @"1ul.Should().BeLessThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Less(0, 1m)", @"1m.Should().BeLessThan(0)")]
    [InlineData(@"Assert.Less(0, 1m, ""because"")", @"1m.Should().BeLessThan(0, ""because"")")]
    [InlineData(@"Assert.Less(0, 1m, ""because"", 2, 3)", @"1m.Should().BeLessThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Less(0, 1f)", @"1f.Should().BeLessThan(0)")]
    [InlineData(@"Assert.Less(0, 1f, ""because"")", @"1f.Should().BeLessThan(0, ""because"")")]
    [InlineData(@"Assert.Less(0, 1f, ""because"", 2, 3)", @"1f.Should().BeLessThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.Less(0, 1d)", @"1d.Should().BeLessThan(0)")]
    [InlineData(@"Assert.Less(0, 1d, ""because"")", @"1d.Should().BeLessThan(0, ""because"")")]
    [InlineData(@"Assert.Less(0, 1d, ""because"", 2, 3)", @"1d.Should().BeLessThan(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.LessOrEqual(0, 1)", @"1.Should().BeLessThanOrEqualTo(0)")]
    [InlineData(@"Assert.LessOrEqual(0, 1, ""because"")", @"1.Should().BeLessThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.LessOrEqual(0, 1, ""because"", 2, 3)", @"1.Should().BeLessThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.LessOrEqual(0, 1u)", @"1u.Should().BeLessThanOrEqualTo(0)")]
    [InlineData(@"Assert.LessOrEqual(0, 1u, ""because"")", @"1u.Should().BeLessThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.LessOrEqual(0, 1u, ""because"", 2, 3)", @"1u.Should().BeLessThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.LessOrEqual(0, 1l)", @"1l.Should().BeLessThanOrEqualTo(0)")]
    [InlineData(@"Assert.LessOrEqual(0, 1l, ""because"")", @"1l.Should().BeLessThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.LessOrEqual(0, 1l, ""because"", 2, 3)", @"1l.Should().BeLessThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.LessOrEqual(0, 1ul)", @"1ul.Should().BeLessThanOrEqualTo(0)")]
    [InlineData(@"Assert.LessOrEqual(0, 1ul, ""because"")", @"1ul.Should().BeLessThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.LessOrEqual(0, 1ul, ""because"", 2, 3)", @"1ul.Should().BeLessThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.LessOrEqual(0, 1m)", @"1m.Should().BeLessThanOrEqualTo(0)")]
    [InlineData(@"Assert.LessOrEqual(0, 1m, ""because"")", @"1m.Should().BeLessThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.LessOrEqual(0, 1m, ""because"", 2, 3)", @"1m.Should().BeLessThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.LessOrEqual(0, 1f)", @"1f.Should().BeLessThanOrEqualTo(0)")]
    [InlineData(@"Assert.LessOrEqual(0, 1f, ""because"")", @"1f.Should().BeLessThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.LessOrEqual(0, 1f, ""because"", 2, 3)", @"1f.Should().BeLessThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.LessOrEqual(0, 1d)", @"1d.Should().BeLessThanOrEqualTo(0)")]
    [InlineData(@"Assert.LessOrEqual(0, 1d, ""because"")", @"1d.Should().BeLessThanOrEqualTo(0, ""because"")")]
    [InlineData(@"Assert.LessOrEqual(0, 1d, ""because"", 2, 3)", @"1d.Should().BeLessThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"Assert.True((bool?)false)", @"((bool?)false).Should().BeTrue()")]
    [InlineData(@"Assert.True((bool?)false, ""because"")", @"((bool?)false).Should().BeTrue(""because"")")]
    [InlineData(@"Assert.True((bool?)false, ""because"", 0, 1)", @"((bool?)false).Should().BeTrue(""because"", 0, 1)")]
    [InlineData(@"Assert.True(false)", @"false.Should().BeTrue()")]
    [InlineData(@"Assert.True(false, ""because"")", @"false.Should().BeTrue(""because"")")]
    [InlineData(@"Assert.True(false, ""because"", 1, 2)", @"false.Should().BeTrue(""because"", 1, 2)")]

    [InlineData(@"Assert.Negative(1)", @"1.Should().BeNegative()")]
    [InlineData(@"Assert.Negative(1, ""because"")", @"1.Should().BeNegative(""because"")")]
    [InlineData(@"Assert.Negative(1, ""because"", 1, 2)", @"1.Should().BeNegative(""because"", 1, 2)")]

    [InlineData(@"Assert.NotNull("""")", @""""".Should().NotBeNull()")]
    [InlineData(@"Assert.NotNull("""", ""because"")", @""""".Should().NotBeNull(""because"")")]
    [InlineData(@"Assert.NotNull("""", ""because"", 1, 2)", @""""".Should().NotBeNull(""because"", 1, 2)")]

    [InlineData(@"Assert.NotZero(1)", @"1.Should().NotBe(0)")]
    [InlineData(@"Assert.NotZero(1, ""because"")", @"1.Should().NotBe(0, ""because"")")]
    [InlineData(@"Assert.NotZero(1, ""because"", 1, 2)", @"1.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.NotZero(1u)", @"1u.Should().NotBe(0)")]
    [InlineData(@"Assert.NotZero(1u, ""because"")", @"1u.Should().NotBe(0, ""because"")")]
    [InlineData(@"Assert.NotZero(1u, ""because"", 1, 2)", @"1u.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.NotZero(1L)", @"1L.Should().NotBe(0)")]
    [InlineData(@"Assert.NotZero(1L, ""because"")", @"1L.Should().NotBe(0, ""because"")")]
    [InlineData(@"Assert.NotZero(1L, ""because"", 1, 2)", @"1L.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.NotZero(1uL)", @"1uL.Should().NotBe(0)")]
    [InlineData(@"Assert.NotZero(1uL, ""because"")", @"1uL.Should().NotBe(0, ""because"")")]
    [InlineData(@"Assert.NotZero(1uL, ""because"", 1, 2)", @"1uL.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.NotZero(1m)", @"1m.Should().NotBe(0)")]
    [InlineData(@"Assert.NotZero(1m, ""because"")", @"1m.Should().NotBe(0, ""because"")")]
    [InlineData(@"Assert.NotZero(1m, ""because"", 1, 2)", @"1m.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.NotZero(1.0)", @"1.0.Should().NotBe(0)")]
    [InlineData(@"Assert.NotZero(1.0, ""because"")", @"1.0.Should().NotBe(0, ""because"")")]
    [InlineData(@"Assert.NotZero(1.0, ""because"", 1, 2)", @"1.0.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.NotZero(1d)", @"1d.Should().NotBe(0)")]
    [InlineData(@"Assert.NotZero(1d, ""because"")", @"1d.Should().NotBe(0, ""because"")")]
    [InlineData(@"Assert.NotZero(1d, ""because"", 1, 2)", @"1d.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.Null("""")", @""""".Should().BeNull()")]
    [InlineData(@"Assert.Null("""", ""because"")", @""""".Should().BeNull(""because"")")]
    [InlineData(@"Assert.Null("""", ""because"", 1, 2)", @""""".Should().BeNull(""because"", 1, 2)")]

    [InlineData(@"Assert.Positive(1)", @"1.Should().BePositive()")]
    [InlineData(@"Assert.Positive(1, ""because"")", @"1.Should().BePositive(""because"")")]
    [InlineData(@"Assert.Positive(1, ""because"", 1, 2)", @"1.Should().BePositive(""because"", 1, 2)")]

    [InlineData(@"Assert.Zero(1)", @"1.Should().Be(0)")]
    [InlineData(@"Assert.Zero(1, ""because"")", @"1.Should().Be(0, ""because"")")]
    [InlineData(@"Assert.Zero(1, ""because"", 1, 2)", @"1.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.Zero(1u)", @"1u.Should().Be(0)")]
    [InlineData(@"Assert.Zero(1u, ""because"")", @"1u.Should().Be(0, ""because"")")]
    [InlineData(@"Assert.Zero(1u, ""because"", 1, 2)", @"1u.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.Zero(1L)", @"1L.Should().Be(0)")]
    [InlineData(@"Assert.Zero(1L, ""because"")", @"1L.Should().Be(0, ""because"")")]
    [InlineData(@"Assert.Zero(1L, ""because"", 1, 2)", @"1L.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.Zero(1uL)", @"1uL.Should().Be(0)")]
    [InlineData(@"Assert.Zero(1uL, ""because"")", @"1uL.Should().Be(0, ""because"")")]
    [InlineData(@"Assert.Zero(1uL, ""because"", 1, 2)", @"1uL.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.Zero(1m)", @"1m.Should().Be(0)")]
    [InlineData(@"Assert.Zero(1m, ""because"")", @"1m.Should().Be(0, ""because"")")]
    [InlineData(@"Assert.Zero(1m, ""because"", 1, 2)", @"1m.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.Zero(1.0)", @"1.0.Should().Be(0)")]
    [InlineData(@"Assert.Zero(1.0, ""because"")", @"1.0.Should().Be(0, ""because"")")]
    [InlineData(@"Assert.Zero(1.0, ""because"", 1, 2)", @"1.0.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"Assert.Zero(1d)", @"1d.Should().Be(0)")]
    [InlineData(@"Assert.Zero(1d, ""because"")", @"1d.Should().Be(0, ""because"")")]
    [InlineData(@"Assert.Zero(1d, ""because"", 1, 2)", @"1d.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.AllItemsAreInstancesOfType(collection, typeof(string))", @"collection.Should().AllBeOfType(typeof(string))")]
    [InlineData(@"CollectionAssert.AllItemsAreInstancesOfType(collection, typeof(string), ""because"")", @"collection.Should().AllBeOfType(typeof(string), ""because"")")]
    [InlineData(@"CollectionAssert.AllItemsAreInstancesOfType(collection, typeof(string), ""because"", 1, 2)", @"collection.Should().AllBeOfType(typeof(string), ""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.AllItemsAreNotNull(collection)", @"collection.Should().NotContainNulls()")]
    [InlineData(@"CollectionAssert.AllItemsAreNotNull(collection, ""because"")", @"collection.Should().NotContainNulls(""because"")")]
    [InlineData(@"CollectionAssert.AllItemsAreNotNull(collection, ""because"", 1, 2)", @"collection.Should().NotContainNulls(""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.AllItemsAreUnique(collection)", @"collection.Should().OnlyHaveUniqueItems()")]
    [InlineData(@"CollectionAssert.AllItemsAreUnique(collection, ""because"")", @"collection.Should().OnlyHaveUniqueItems(""because"")")]
    [InlineData(@"CollectionAssert.AllItemsAreUnique(collection, ""because"", 1, 2)", @"collection.Should().OnlyHaveUniqueItems(""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.AreEqual(new int[0], collection)", @"collection.Should().Equal(new int[0])")]
    [InlineData(@"CollectionAssert.AreEqual(new int[0], collection, ""because"")", @"collection.Should().Equal(new int[0], ""because"")")]
    [InlineData(@"CollectionAssert.AreEqual(new int[0], collection, ""because"", 1, 2)", @"collection.Should().Equal(new int[0], ""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.AreNotEqual(new int[0], collection)", @"collection.Should().NotEqual(new int[0])")]
    [InlineData(@"CollectionAssert.AreNotEqual(new int[0], collection, ""because"")", @"collection.Should().NotEqual(new int[0], ""because"")")]
    [InlineData(@"CollectionAssert.AreNotEqual(new int[0], collection, ""because"", 1, 2)", @"collection.Should().NotEqual(new int[0], ""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.AreEquivalent(new int[0], collection)", @"collection.Should().BeEquivalentTo(new int[0])")]
    [InlineData(@"CollectionAssert.AreEquivalent(new int[0], collection, ""because"")", @"collection.Should().BeEquivalentTo(new int[0], ""because"")")]
    [InlineData(@"CollectionAssert.AreEquivalent(new int[0], collection, ""because"", 1, 2)", @"collection.Should().BeEquivalentTo(new int[0], ""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.AreNotEquivalent(new int[0], collection)", @"collection.Should().NotBeEquivalentTo(new int[0])")]
    [InlineData(@"CollectionAssert.AreNotEquivalent(new int[0], collection, ""because"")", @"collection.Should().NotBeEquivalentTo(new int[0], ""because"")")]
    [InlineData(@"CollectionAssert.AreNotEquivalent(new int[0], collection, ""because"", 1, 2)", @"collection.Should().NotBeEquivalentTo(new int[0], ""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.Contains(collection, 1)", @"collection.Should().Contain(1)")]
    [InlineData(@"CollectionAssert.Contains(collection, 1, ""because"")", @"collection.Should().Contain(1, ""because"")")]
    [InlineData(@"CollectionAssert.Contains(collection, 1, ""because"", 1, 2)", @"collection.Should().Contain(1, ""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.DoesNotContain(collection, 1)", @"collection.Should().NotContain(1)")]
    [InlineData(@"CollectionAssert.DoesNotContain(collection, 1, ""because"")", @"collection.Should().NotContain(1, ""because"")")]
    [InlineData(@"CollectionAssert.DoesNotContain(collection, 1, ""because"", 1, 2)", @"collection.Should().NotContain(1, ""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.IsEmpty(collection)", @"collection.Should().BeEmpty()")]
    [InlineData(@"CollectionAssert.IsEmpty(collection, ""because"")", @"collection.Should().BeEmpty(""because"")")]
    [InlineData(@"CollectionAssert.IsEmpty(collection, ""because"", 1, 2)", @"collection.Should().BeEmpty(""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.IsNotEmpty(collection)", @"collection.Should().NotBeEmpty()")]
    [InlineData(@"CollectionAssert.IsNotEmpty(collection, ""because"")", @"collection.Should().NotBeEmpty(""because"")")]
    [InlineData(@"CollectionAssert.IsNotEmpty(collection, ""because"", 1, 2)", @"collection.Should().NotBeEmpty(""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.IsNotSubsetOf(new int[1], collection)", @"collection.Should().NotBeSubsetOf(new int[1])")]
    [InlineData(@"CollectionAssert.IsNotSubsetOf(new int[1], collection, ""because"")", @"collection.Should().NotBeSubsetOf(new int[1], ""because"")")]
    [InlineData(@"CollectionAssert.IsNotSubsetOf(new int[1], collection, ""because"", 1, 2)", @"collection.Should().NotBeSubsetOf(new int[1], ""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.IsSubsetOf(new int[1], collection)", @"collection.Should().BeSubsetOf(new int[1])")]
    [InlineData(@"CollectionAssert.IsSubsetOf(new int[1], collection, ""because"")", @"collection.Should().BeSubsetOf(new int[1], ""because"")")]
    [InlineData(@"CollectionAssert.IsSubsetOf(new int[1], collection, ""because"", 1, 2)", @"collection.Should().BeSubsetOf(new int[1], ""because"", 1, 2)")]

    [InlineData(@"CollectionAssert.IsOrdered(collection)", @"collection.Should().BeInAscendingOrder()")]
    [InlineData(@"CollectionAssert.IsOrdered(collection, ""because"")", @"collection.Should().BeInAscendingOrder(""because"")")]
    [InlineData(@"CollectionAssert.IsOrdered(collection, ""because"", 1, 2)", @"collection.Should().BeInAscendingOrder(""because"", 1, 2)")]

    [InlineData(@"StringAssert.AreEqualIgnoringCase(""expected"", ""actual"")", @"""actual"".Should().BeEquivalentTo(""expected"")")]
    [InlineData(@"StringAssert.AreEqualIgnoringCase(""expected"", ""actual"", ""because"")", @"""actual"".Should().BeEquivalentTo(""expected"", ""because"")")]
    [InlineData(@"StringAssert.AreEqualIgnoringCase(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().BeEquivalentTo(""expected"", ""because"", 1, 2)")]

    [InlineData(@"StringAssert.AreNotEqualIgnoringCase(""expected"", ""actual"")", @"""actual"".Should().NotBeEquivalentTo(""expected"")")]
    [InlineData(@"StringAssert.AreNotEqualIgnoringCase(""expected"", ""actual"", ""because"")", @"""actual"".Should().NotBeEquivalentTo(""expected"", ""because"")")]
    [InlineData(@"StringAssert.AreNotEqualIgnoringCase(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().NotBeEquivalentTo(""expected"", ""because"", 1, 2)")]

    [InlineData(@"StringAssert.Contains(""expected"", ""actual"")", @"""actual"".Should().Contain(""expected"")")]
    [InlineData(@"StringAssert.Contains(""expected"", ""actual"", ""because"")", @"""actual"".Should().Contain(""expected"", ""because"")")]
    [InlineData(@"StringAssert.Contains(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().Contain(""expected"", ""because"", 1, 2)")]

    [InlineData(@"StringAssert.DoesNotContain(""expected"", ""actual"")", @"""actual"".Should().NotContain(""expected"")")]
    [InlineData(@"StringAssert.DoesNotContain(""expected"", ""actual"", ""because"")", @"""actual"".Should().NotContain(""expected"", ""because"")")]
    [InlineData(@"StringAssert.DoesNotContain(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().NotContain(""expected"", ""because"", 1, 2)")]

    [InlineData(@"StringAssert.DoesNotEndWith(""expected"", ""actual"")", @"""actual"".Should().NotEndWith(""expected"")")]
    [InlineData(@"StringAssert.DoesNotEndWith(""expected"", ""actual"", ""because"")", @"""actual"".Should().NotEndWith(""expected"", ""because"")")]
    [InlineData(@"StringAssert.DoesNotEndWith(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().NotEndWith(""expected"", ""because"", 1, 2)")]

    [InlineData(@"StringAssert.DoesNotMatch(""expected"", ""actual"")", @"""actual"".Should().NotMatchRegex(""expected"")")]
    [InlineData(@"StringAssert.DoesNotMatch(""expected"", ""actual"", ""because"")", @"""actual"".Should().NotMatchRegex(""expected"", ""because"")")]
    [InlineData(@"StringAssert.DoesNotMatch(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().NotMatchRegex(""expected"", ""because"", 1, 2)")]

    [InlineData(@"StringAssert.DoesNotStartWith(""expected"", ""actual"")", @"""actual"".Should().NotStartWith(""expected"")")]
    [InlineData(@"StringAssert.DoesNotStartWith(""expected"", ""actual"", ""because"")", @"""actual"".Should().NotStartWith(""expected"", ""because"")")]
    [InlineData(@"StringAssert.DoesNotStartWith(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().NotStartWith(""expected"", ""because"", 1, 2)")]

    [InlineData(@"StringAssert.EndsWith(""expected"", ""actual"")", @"""actual"".Should().EndWith(""expected"")")]
    [InlineData(@"StringAssert.EndsWith(""expected"", ""actual"", ""because"")", @"""actual"".Should().EndWith(""expected"", ""because"")")]
    [InlineData(@"StringAssert.EndsWith(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().EndWith(""expected"", ""because"", 1, 2)")]

    [InlineData(@"StringAssert.IsMatch(""expected"", ""actual"")", @"""actual"".Should().MatchRegex(""expected"")")]
    [InlineData(@"StringAssert.IsMatch(""expected"", ""actual"", ""because"")", @"""actual"".Should().MatchRegex(""expected"", ""because"")")]
    [InlineData(@"StringAssert.IsMatch(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().MatchRegex(""expected"", ""because"", 1, 2)")]

    [InlineData(@"StringAssert.StartsWith(""expected"", ""actual"")", @"""actual"".Should().StartWith(""expected"")")]
    [InlineData(@"StringAssert.StartsWith(""expected"", ""actual"", ""because"")", @"""actual"".Should().StartWith(""expected"", ""because"")")]
    [InlineData(@"StringAssert.StartsWith(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().StartWith(""expected"", ""because"", 1, 2)")]

    [InlineData(@"Assert.That(false, Is.True)", @"false.Should().BeTrue()")]
    [InlineData(@"Assert.That(false, Is.False)", @"false.Should().BeFalse()")]

    [InlineData(@"Assert.That(true)", @"true.Should().BeTrue()")]
    [InlineData(@"Assert.That(true, ""because"")", @"true.Should().BeTrue(""because"")")]
    [InlineData(@"Assert.That(true, ""because"", 1, 2)", @"true.Should().BeTrue(""because"", 1, 2)")]

    [InlineData(@"Assert.That((bool?)false, Is.True)", @"((bool?)false).Should().BeTrue()")]
    [InlineData(@"Assert.That((bool?)false, Is.False)", @"((bool?)false).Should().BeFalse()")]

    [InlineData(@"Assert.That(false, Is.Not.True)", @"false.Should().BeFalse()")]
    [InlineData(@"Assert.That(false, Is.Not.False)", @"false.Should().BeTrue()")]

    [InlineData(@"Assert.That((bool?)false, Is.Not.True)", @"((bool?)false).Should().NotBeTrue()")]
    [InlineData(@"Assert.That((bool?)false, Is.Not.False)", @"((bool?)false).Should().NotBeFalse()")]

    [InlineData(@"Assert.That("""", Is.Null)", @""""".Should().BeNull()")]
    [InlineData(@"Assert.That("""", Is.Empty)", @""""".Should().BeEmpty()")]
    [InlineData(@"Assert.That(collection, Is.Empty)", @"collection.Should().BeEmpty()")]
    [InlineData(@"Assert.That(collection, Is.Empty, ""because"")", @"collection.Should().BeEmpty(""because"")")]

    [InlineData(@"Assert.That("""", Is.Not.Null)", @""""".Should().NotBeNull()")]
    [InlineData(@"Assert.That("""", Is.Not.Empty)", @""""".Should().NotBeEmpty()")]
    [InlineData(@"Assert.That(collection, Is.Not.Empty)", @"collection.Should().NotBeEmpty()")]
    [InlineData(@"Assert.That(collection, Is.Not.Empty, ""because"")", @"collection.Should().NotBeEmpty(""because"")")]

    [InlineData(@"Assert.That("""", Is.Null.Or.Empty)", @""""".Should().BeNullOrEmpty()")]
    [InlineData(@"Assert.That("""", Is.Not.Null.Or.Empty)", @""""".Should().NotBeNullOrEmpty()")]

    [InlineData(@"Assert.That(collection, Has.One.Items)", @"collection.Should().HaveCount(1)")]
    [InlineData(@"Assert.That(collection, Has.Count.EqualTo(2))", @"collection.Should().HaveCount(2)")]

    [InlineData(@"Assert.That("""", Is.EqualTo(""expected""))", @""""".Should().Be(""expected"")")]
    [InlineData(@"Assert.That("""", Is.Not.EqualTo(""expected""))", @""""".Should().NotBe(""expected"")")]

    [InlineData(@"Assert.That("""", Does.Contain(""expected""))", @""""".Should().Contain(""expected"")")]
    [InlineData(@"Assert.That("""", Does.Not.Contain(""expected""))", @""""".Should().NotContain(""expected"")")]
    [InlineData(@"Assert.That("""", Contains.Substring(""expected""))", @""""".Should().Contain(""expected"")")]

    [InlineData(@"Assert.That("""", Does.EndWith(""expected""))", @""""".Should().EndWith(""expected"")")]
    [InlineData(@"Assert.That("""", Does.Not.EndWith(""expected""))", @""""".Should().NotEndWith(""expected"")")]

    [InlineData(@"Assert.That("""", Does.Not.EndsWith(""expected""))", @""""".Should().NotEndWith(""expected"")")]

    [InlineData(@"Assert.That("""", Does.StartWith(""expected""))", @""""".Should().StartWith(""expected"")")]
    [InlineData(@"Assert.That("""", Does.Not.StartWith(""expected""))", @""""".Should().NotStartWith(""expected"")")]

    [InlineData(@"Assert.That("""", Does.Not.StartsWith(""expected""))", @""""".Should().NotStartWith(""expected"")")]

    [InlineData(@"Assert.That(() => { }, Throws.InstanceOf(typeof(System.ArgumentException)))", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>()")]
    [InlineData(@"Assert.That(() => { }, Throws.InstanceOf<System.ArgumentException>())", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>()")]

    public Task Assert_Tests(string code, string fix)
    {
        return Assert(
$$"""
using NUnit.Framework;

class Test
{
    public void MyTest()
    {
        var collection = new int[1];
        [|{{code}}|];
    }
}
""",
$$"""
using FluentAssertions;
using NUnit.Framework;

class Test
{
    public void MyTest()
    {
        var collection = new int[1];
        {{fix}};
    }
}
""");
    }
}
