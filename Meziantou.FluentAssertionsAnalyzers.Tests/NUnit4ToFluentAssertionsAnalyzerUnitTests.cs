using Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;
using TestHelper;
using Xunit;

namespace Meziantou.FluentAssertionsAnalyzers.Tests;

public sealed class NUnit4ToFluentAssertionsAnalyzerUnitTests
{
    private static ProjectBuilder CreateProjectBuilder()
    {
        return new ProjectBuilder()
            .WithTargetFramework(TargetFramework.Net6_0)
            .WithAnalyzer<AssertAnalyzer>(id: "MFA003")
            .AddAllCodeFixers()
            .AddNUnit4Api()
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
using NUnit.Framework.Legacy;

class Test
{
    public void MyTest()
    {
        [|ClassicAssert.Pass()|];
    }
}
""",
"""
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
    public Task EnumerableTest()
    {
        return Assert(
            $$"""
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Legacy;

class CustomEnumerable : IEnumerable
{
    private readonly IEnumerable m_Collection;

    public CustomEnumerable(IEnumerable collection)
    {
        m_Collection = collection;
    }
    public IEnumerator GetEnumerator()
    {
        foreach (var item in m_Collection)
        {
            yield return item;
        }
    }
}

class Test
{
    public void MyTest()
    {
        var collection = new CustomEnumerable(new int[0]);
        [||]ClassicAssert.That(collection, Is.EqualTo(new int[0]));
    }
}
""",
            $$"""
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Legacy;

class CustomEnumerable : IEnumerable
{
    private readonly IEnumerable m_Collection;

    public CustomEnumerable(IEnumerable collection)
    {
        m_Collection = collection;
    }
    public IEnumerator GetEnumerator()
    {
        foreach (var item in m_Collection)
        {
            yield return item;
        }
    }
}

class Test
{
    public void MyTest()
    {
        var collection = new CustomEnumerable(new int[0]);
        ClassicAssert.That(collection, Is.EqualTo(new int[0]));
    }
}
""");
    }

    [Fact]
    public Task Assert_Dynamic()
    {
        return Assert(
"""
using NUnit.Framework;
using NUnit.Framework.Legacy;

class Test
{
    public void MyTest()
    {
        dynamic val = new object();
        [|ClassicAssert.That(val, Is.Not.Null)|];
    }
}
""",
"""
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

class Test
{
    public void MyTest()
    {
        dynamic val = new object();
        ((object)val).Should().NotBeNull();
    }
}
""");
    }

    [Fact]
    public Task Assert_DynamicProp()
    {
        return Assert(
            """
using NUnit.Framework;
using NUnit.Framework.Legacy;

class Test
{
    public void MyTest()
    {
        dynamic val = new object();
        [|ClassicAssert.That(val.Prop, Is.Not.Null)|];
    }
}
""",
            """
using FluentAssertions;
using NUnit.Framework;

class Test
{
    public void MyTest()
    {
        dynamic val = new object();
        ((object)val.Prop).Should().NotBeNull();
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
using NUnit.Framework.Legacy;

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
    [InlineData(@"ClassicAssert.AreEqual(false, true)", @"true.Should().Be(false)")]
    [InlineData(@"ClassicAssert.AreEqual(false, true, ""because"")", @"true.Should().Be(false, ""because"")")]
    [InlineData(@"ClassicAssert.AreEqual(false, true, ""because"", 1, 2)", @"true.Should().Be(false, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.AreEqual(""expected"", ""actual"")", @"""actual"".Should().Be(""expected"")")]
    [InlineData(@"ClassicAssert.AreEqual(""expected"", ""actual"", ""because"")", @"""actual"".Should().Be(""expected"", ""because"")")]
    [InlineData(@"ClassicAssert.AreEqual(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().Be(""expected"", ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.AreEqual(0d, 1d, delta: 2d)", @"1d.Should().BeApproximately(0d, 2d)")]
    [InlineData(@"ClassicAssert.AreEqual(0d, 1d, delta: 2d, ""because"")", @"1d.Should().BeApproximately(0d, 2d, ""because"")")]
    [InlineData(@"ClassicAssert.AreEqual(0d, 1d, delta: 2d, ""because"", 1, 2)", @"1d.Should().BeApproximately(0d, 2d, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.AreEqual(0d, (double?)null, delta: 2d)", @"((double?)null).Should().BeApproximately(0d, 2d)")]
    [InlineData(@"ClassicAssert.AreEqual(0d, (double?)null, delta: 2d, ""because"")", @"((double?)null).Should().BeApproximately(0d, 2d, ""because"")")]
    [InlineData(@"ClassicAssert.AreEqual(0d, (double?)null, delta: 2d, ""because"", 1, 2)", @"((double?)null).Should().BeApproximately(0d, 2d, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.AreNotEqual(""expected"", ""actual"")", @"""actual"".Should().NotBe(""expected"")")]
    [InlineData(@"ClassicAssert.AreNotEqual(""expected"", ""actual"", ""because"")", @"""actual"".Should().NotBe(""expected"", ""because"")")]
    [InlineData(@"ClassicAssert.AreNotEqual(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().NotBe(""expected"", ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.AreNotSame(""expected"", ""actual"")", @"""actual"".Should().NotBeSameAs(""expected"")")]
    [InlineData(@"ClassicAssert.AreNotSame(""expected"", ""actual"", ""because"")", @"""actual"".Should().NotBeSameAs(""expected"", ""because"")")]
    [InlineData(@"ClassicAssert.AreNotSame(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().NotBeSameAs(""expected"", ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.AreSame(""expected"", ""actual"")", @"""actual"".Should().BeSameAs(""expected"")")]
    [InlineData(@"ClassicAssert.AreSame(""expected"", ""actual"", ""because"")", @"""actual"".Should().BeSameAs(""expected"", ""because"")")]
    [InlineData(@"ClassicAssert.AreSame(""expected"", ""actual"", ""because"", 1, 2)", @"""actual"".Should().BeSameAs(""expected"", ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Catch(() => { })", @"FluentActions.Invoking(() => { }).Should().Throw<System.Exception>()")]
    [InlineData(@"ClassicAssert.Catch(() => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().Throw<System.Exception>(""because"")")]
    [InlineData(@"ClassicAssert.Catch(() => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().Throw<System.Exception>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Catch(typeof(System.ArgumentException), () => { })", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>()")]
    [InlineData(@"ClassicAssert.Catch(typeof(System.ArgumentException), () => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>(""because"")")]
    [InlineData(@"ClassicAssert.Catch(typeof(System.ArgumentException), () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Catch<System.ArgumentException>(() => { })", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>()")]
    [InlineData(@"ClassicAssert.Catch<System.ArgumentException>(() => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>(""because"")")]
    [InlineData(@"ClassicAssert.Catch<System.ArgumentException>(() => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().Throw<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.CatchAsync(async () => { })", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.Exception>()")]
    [InlineData(@"ClassicAssert.CatchAsync(async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.Exception>(""because"")")]
    [InlineData(@"ClassicAssert.CatchAsync(async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.Exception>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.CatchAsync(typeof(System.ArgumentException), async () => { })", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>()")]
    [InlineData(@"ClassicAssert.CatchAsync(typeof(System.ArgumentException), async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>(""because"")")]
    [InlineData(@"ClassicAssert.CatchAsync(typeof(System.ArgumentException), async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.CatchAsync<System.ArgumentException>(async () => { })", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>()")]
    [InlineData(@"ClassicAssert.CatchAsync<System.ArgumentException>(async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>(""because"")")]
    [InlineData(@"ClassicAssert.CatchAsync<System.ArgumentException>(async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().ThrowAsync<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.DoesNotThrow(() => { })", @"FluentActions.Invoking(() => { }).Should().NotThrow()")]
    [InlineData(@"ClassicAssert.DoesNotThrow(() => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().NotThrow(""because"")")]
    [InlineData(@"ClassicAssert.DoesNotThrow(() => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().NotThrow(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.DoesNotThrowAsync(async () => { })", @"FluentActions.Invoking(async () => { }).Should().NotThrowAsync()")]
    [InlineData(@"ClassicAssert.DoesNotThrowAsync(async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().NotThrowAsync(""because"")")]
    [InlineData(@"ClassicAssert.DoesNotThrowAsync(async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().NotThrowAsync(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Throws(typeof(System.ArgumentException), () => { })", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>()")]
    [InlineData(@"ClassicAssert.Throws(typeof(System.ArgumentException), () => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>(""because"")")]
    [InlineData(@"ClassicAssert.Throws(typeof(System.ArgumentException), () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Throws<System.ArgumentException>(() => { })", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>()")]
    [InlineData(@"ClassicAssert.Throws<System.ArgumentException>(() => { }, ""because"")", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>(""because"")")]
    [InlineData(@"ClassicAssert.Throws<System.ArgumentException>(() => { }, ""because"", 1, 2)", @"FluentActions.Invoking(() => { }).Should().ThrowExactly<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.ThrowsAsync(typeof(System.ArgumentException), async () => { })", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>()")]
    [InlineData(@"ClassicAssert.ThrowsAsync(typeof(System.ArgumentException), async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>(""because"")")]
    [InlineData(@"ClassicAssert.ThrowsAsync(typeof(System.ArgumentException), async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.ThrowsAsync<System.ArgumentException>(async () => { })", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>()")]
    [InlineData(@"ClassicAssert.ThrowsAsync<System.ArgumentException>(async () => { }, ""because"")", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>(""because"")")]
    [InlineData(@"ClassicAssert.ThrowsAsync<System.ArgumentException>(async () => { }, ""because"", 1, 2)", @"FluentActions.Invoking(async () => { }).Should().ThrowExactlyAsync<System.ArgumentException>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.False((bool?)false)", @"((bool?)false).Should().BeFalse()")]
    [InlineData(@"ClassicAssert.False((bool?)false, ""because"")", @"((bool?)false).Should().BeFalse(""because"")")]
    [InlineData(@"ClassicAssert.False((bool?)false, ""because"", 0, 1)", @"((bool?)false).Should().BeFalse(""because"", 0, 1)")]
    [InlineData(@"ClassicAssert.False(false)", @"false.Should().BeFalse()")]
    [InlineData(@"ClassicAssert.False(false, ""because"")", @"false.Should().BeFalse(""because"")")]
    [InlineData(@"ClassicAssert.False(false, ""because"", 1, 2)", @"false.Should().BeFalse(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Greater(1, 0)", @"1.Should().BeGreaterThan(0)")]
    [InlineData(@"ClassicAssert.Greater(1, 0, ""because"")", @"1.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"ClassicAssert.Greater(1, 0, ""because"", 2, 3)", @"1.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Greater(1u, 0)", @"1u.Should().BeGreaterThan(0)")]
    [InlineData(@"ClassicAssert.Greater(1u, 0, ""because"")", @"1u.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"ClassicAssert.Greater(1u, 0, ""because"", 2, 3)", @"1u.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Greater(1l, 0)", @"1l.Should().BeGreaterThan(0)")]
    [InlineData(@"ClassicAssert.Greater(1l, 0, ""because"")", @"1l.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"ClassicAssert.Greater(1l, 0, ""because"", 2, 3)", @"1l.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Greater(1ul, 0)", @"1ul.Should().BeGreaterThan(0)")]
    [InlineData(@"ClassicAssert.Greater(1ul, 0, ""because"")", @"1ul.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"ClassicAssert.Greater(1ul, 0, ""because"", 2, 3)", @"1ul.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Greater(1m, 0)", @"1m.Should().BeGreaterThan(0)")]
    [InlineData(@"ClassicAssert.Greater(1m, 0, ""because"")", @"1m.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"ClassicAssert.Greater(1m, 0, ""because"", 2, 3)", @"1m.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Greater(1f, 0)", @"1f.Should().BeGreaterThan(0)")]
    [InlineData(@"ClassicAssert.Greater(1f, 0, ""because"")", @"1f.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"ClassicAssert.Greater(1f, 0, ""because"", 2, 3)", @"1f.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Greater(1d, 0)", @"1d.Should().BeGreaterThan(0)")]
    [InlineData(@"ClassicAssert.Greater(1d, 0, ""because"")", @"1d.Should().BeGreaterThan(0, ""because"")")]
    [InlineData(@"ClassicAssert.Greater(1d, 0, ""because"", 2, 3)", @"1d.Should().BeGreaterThan(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.GreaterOrEqual(1, 0)", @"1.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1, 0, ""because"")", @"1.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1, 0, ""because"", 2, 3)", @"1.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.GreaterOrEqual(1u, 0)", @"1u.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1u, 0, ""because"")", @"1u.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1u, 0, ""because"", 2, 3)", @"1u.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.GreaterOrEqual(1l, 0)", @"1l.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1l, 0, ""because"")", @"1l.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1l, 0, ""because"", 2, 3)", @"1l.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.GreaterOrEqual(1ul, 0)", @"1ul.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1ul, 0, ""because"")", @"1ul.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1ul, 0, ""because"", 2, 3)", @"1ul.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.GreaterOrEqual(1m, 0)", @"1m.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1m, 0, ""because"")", @"1m.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1m, 0, ""because"", 2, 3)", @"1m.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.GreaterOrEqual(1f, 0)", @"1f.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1f, 0, ""because"")", @"1f.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1f, 0, ""because"", 2, 3)", @"1f.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.GreaterOrEqual(1d, 0)", @"1d.Should().BeGreaterThanOrEqualTo(0)")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1d, 0, ""because"")", @"1d.Should().BeGreaterThanOrEqualTo(0, ""because"")")]
    [InlineData(@"ClassicAssert.GreaterOrEqual(1d, 0, ""because"", 2, 3)", @"1d.Should().BeGreaterThanOrEqualTo(0, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.IsEmpty("""")", @""""".Should().BeEmpty()")]
    [InlineData(@"ClassicAssert.IsEmpty("""", ""because"")", @""""".Should().BeEmpty(""because"")")]
    [InlineData(@"ClassicAssert.IsEmpty("""", ""because"", 1, 2)", @""""".Should().BeEmpty(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsEmpty(new[] { 0 })", @"(new[] { 0 }).Should().BeEmpty()")]
    [InlineData(@"ClassicAssert.IsEmpty(new[] { 0 }, ""because"")", @"(new[] { 0 }).Should().BeEmpty(""because"")")]
    [InlineData(@"ClassicAssert.IsEmpty(new[] { 0 }, ""because"", 1, 2)", @"(new[] { 0 }).Should().BeEmpty(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsInstanceOf(typeof(string), """")", @""""".Should().BeOfType(typeof(string))")]
    [InlineData(@"ClassicAssert.IsInstanceOf(typeof(string), """", ""because"")", @""""".Should().BeOfType(typeof(string), ""because"")")]
    [InlineData(@"ClassicAssert.IsInstanceOf(typeof(string), """", ""because"", 1, 2)", @""""".Should().BeOfType(typeof(string), ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.That("""", Is.InstanceOf(typeof(string)))", @""""".Should().BeOfType(typeof(string))")]
    [InlineData(@"ClassicAssert.That("""", Is.InstanceOf(typeof(string)), ""because"")", @""""".Should().BeOfType(typeof(string), ""because"")")]
    [InlineData(@"ClassicAssert.That("""", Is.InstanceOf(typeof(string)), ""because"", 1, 2)", @""""".Should().BeOfType(typeof(string), ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsInstanceOf<string>("""")", @""""".Should().BeOfType<string>()")]
    [InlineData(@"ClassicAssert.IsInstanceOf<string>("""", ""because"")", @""""".Should().BeOfType<string>(""because"")")]
    [InlineData(@"ClassicAssert.IsInstanceOf<string>("""", ""because"", 1, 2)", @""""".Should().BeOfType<string>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.That("""", Is.InstanceOf<string>())", @""""".Should().BeOfType<string>()")]
    [InlineData(@"ClassicAssert.That("""", Is.InstanceOf<string>(), ""because"")", @""""".Should().BeOfType<string>(""because"")")]
    [InlineData(@"ClassicAssert.That("""", Is.InstanceOf<string>(), ""because"", 1, 2)", @""""".Should().BeOfType<string>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsNotInstanceOf(typeof(string), """")", @""""".Should().NotBeOfType(typeof(string))")]
    [InlineData(@"ClassicAssert.IsNotInstanceOf(typeof(string), """", ""because"")", @""""".Should().NotBeOfType(typeof(string), ""because"")")]
    [InlineData(@"ClassicAssert.IsNotInstanceOf(typeof(string), """", ""because"", 1, 2)", @""""".Should().NotBeOfType(typeof(string), ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsNotInstanceOf<string>("""")", @""""".Should().NotBeOfType<string>()")]
    [InlineData(@"ClassicAssert.IsNotInstanceOf<string>("""", ""because"")", @""""".Should().NotBeOfType<string>(""because"")")]
    [InlineData(@"ClassicAssert.IsNotInstanceOf<string>("""", ""because"", 1, 2)", @""""".Should().NotBeOfType<string>(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsNotEmpty("""")", @""""".Should().NotBeEmpty()")]
    [InlineData(@"ClassicAssert.IsNotEmpty("""", ""because"")", @""""".Should().NotBeEmpty(""because"")")]
    [InlineData(@"ClassicAssert.IsNotEmpty("""", ""because"", 1, 2)", @""""".Should().NotBeEmpty(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsNotEmpty(new[] { 0 })", @"(new[] { 0 }).Should().NotBeEmpty()")]
    [InlineData(@"ClassicAssert.IsNotEmpty(new[] { 0 }, ""because"")", @"(new[] { 0 }).Should().NotBeEmpty(""because"")")]
    [InlineData(@"ClassicAssert.IsNotEmpty(new[] { 0 }, ""because"", 1, 2)", @"(new[] { 0 }).Should().NotBeEmpty(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsFalse((bool?)false)", @"((bool?)false).Should().BeFalse()")]
    [InlineData(@"ClassicAssert.IsFalse((bool?)false, ""because"")", @"((bool?)false).Should().BeFalse(""because"")")]
    [InlineData(@"ClassicAssert.IsFalse((bool?)false, ""because"", 0, 1)", @"((bool?)false).Should().BeFalse(""because"", 0, 1)")]
    [InlineData(@"ClassicAssert.IsFalse(false)", @"false.Should().BeFalse()")]
    [InlineData(@"ClassicAssert.IsFalse(false, ""because"")", @"false.Should().BeFalse(""because"")")]
    [InlineData(@"ClassicAssert.IsFalse(false, ""because"", 1, 2)", @"false.Should().BeFalse(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsNaN(1d)", @"1d.Should().Be(double.NaN)")]
    [InlineData(@"ClassicAssert.IsNaN((double?)1d)", @"((double?)1d).Should().Be(double.NaN)")]

    [InlineData(@"ClassicAssert.IsNotNull("""")", @""""".Should().NotBeNull()")]
    [InlineData(@"ClassicAssert.IsNotNull("""", ""because"")", @""""".Should().NotBeNull(""because"")")]
    [InlineData(@"ClassicAssert.IsNotNull("""", ""because"", 1, 2)", @""""".Should().NotBeNull(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsNull("""")", @""""".Should().BeNull()")]
    [InlineData(@"ClassicAssert.IsNull("""", ""because"")", @""""".Should().BeNull(""because"")")]
    [InlineData(@"ClassicAssert.IsNull("""", ""because"", 1, 2)", @""""".Should().BeNull(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.IsTrue((bool?)false)", @"((bool?)false).Should().BeTrue()")]
    [InlineData(@"ClassicAssert.IsTrue((bool?)false, ""because"")", @"((bool?)false).Should().BeTrue(""because"")")]
    [InlineData(@"ClassicAssert.IsTrue((bool?)false, ""because"", 0, 1)", @"((bool?)false).Should().BeTrue(""because"", 0, 1)")]
    [InlineData(@"ClassicAssert.IsTrue(false)", @"false.Should().BeTrue()")]
    [InlineData(@"ClassicAssert.IsTrue(false, ""because"")", @"false.Should().BeTrue(""because"")")]
    [InlineData(@"ClassicAssert.IsTrue(false, ""because"", 1, 2)", @"false.Should().BeTrue(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Less(0, 1)", @"0.Should().BeLessThan(1)")]
    [InlineData(@"ClassicAssert.Less(0, 1, ""because"")", @"0.Should().BeLessThan(1, ""because"")")]
    [InlineData(@"ClassicAssert.Less(0, 1, ""because"", 2, 3)", @"0.Should().BeLessThan(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Less(0u, 1)", @"0u.Should().BeLessThan(1)")]
    [InlineData(@"ClassicAssert.Less(0u, 1, ""because"")", @"0u.Should().BeLessThan(1, ""because"")")]
    [InlineData(@"ClassicAssert.Less(0u, 1, ""because"", 2, 3)", @"0u.Should().BeLessThan(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Less(0l, 1)", @"0l.Should().BeLessThan(1)")]
    [InlineData(@"ClassicAssert.Less(0l, 1, ""because"")", @"0l.Should().BeLessThan(1, ""because"")")]
    [InlineData(@"ClassicAssert.Less(0l, 1, ""because"", 2, 3)", @"0l.Should().BeLessThan(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Less(0ul, 1)", @"0ul.Should().BeLessThan(1)")]
    [InlineData(@"ClassicAssert.Less(0ul, 1, ""because"")", @"0ul.Should().BeLessThan(1, ""because"")")]
    [InlineData(@"ClassicAssert.Less(0ul, 1, ""because"", 2, 3)", @"0ul.Should().BeLessThan(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Less(0m, 1)", @"0m.Should().BeLessThan(1)")]
    [InlineData(@"ClassicAssert.Less(0m, 1, ""because"")", @"0m.Should().BeLessThan(1, ""because"")")]
    [InlineData(@"ClassicAssert.Less(0m, 1, ""because"", 2, 3)", @"0m.Should().BeLessThan(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Less(0f, 1)", @"0f.Should().BeLessThan(1)")]
    [InlineData(@"ClassicAssert.Less(0f, 1, ""because"")", @"0f.Should().BeLessThan(1, ""because"")")]
    [InlineData(@"ClassicAssert.Less(0f, 1, ""because"", 2, 3)", @"0f.Should().BeLessThan(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.Less(0d, 1)", @"0d.Should().BeLessThan(1)")]
    [InlineData(@"ClassicAssert.Less(0d, 1, ""because"")", @"0d.Should().BeLessThan(1, ""because"")")]
    [InlineData(@"ClassicAssert.Less(0d, 1, ""because"", 2, 3)", @"0d.Should().BeLessThan(1, ""because"", 2, 3)")]


    [InlineData(@"ClassicAssert.LessOrEqual(0, 1)", @"0.Should().BeLessThanOrEqualTo(1)")]
    [InlineData(@"ClassicAssert.LessOrEqual(0, 1, ""because"")", @"0.Should().BeLessThanOrEqualTo(1, ""because"")")]
    [InlineData(@"ClassicAssert.LessOrEqual(0, 1, ""because"", 2, 3)", @"0.Should().BeLessThanOrEqualTo(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.LessOrEqual(0u, 1)", @"0u.Should().BeLessThanOrEqualTo(1)")]
    [InlineData(@"ClassicAssert.LessOrEqual(0u, 1, ""because"")", @"0u.Should().BeLessThanOrEqualTo(1, ""because"")")]
    [InlineData(@"ClassicAssert.LessOrEqual(0u, 1, ""because"", 2, 3)", @"0u.Should().BeLessThanOrEqualTo(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.LessOrEqual(0l, 1)", @"0l.Should().BeLessThanOrEqualTo(1)")]
    [InlineData(@"ClassicAssert.LessOrEqual(0l, 1, ""because"")", @"0l.Should().BeLessThanOrEqualTo(1, ""because"")")]
    [InlineData(@"ClassicAssert.LessOrEqual(0l, 1, ""because"", 2, 3)", @"0l.Should().BeLessThanOrEqualTo(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.LessOrEqual(0ul, 1)", @"0ul.Should().BeLessThanOrEqualTo(1)")]
    [InlineData(@"ClassicAssert.LessOrEqual(0ul, 1, ""because"")", @"0ul.Should().BeLessThanOrEqualTo(1, ""because"")")]
    [InlineData(@"ClassicAssert.LessOrEqual(0ul, 1, ""because"", 2, 3)", @"0ul.Should().BeLessThanOrEqualTo(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.LessOrEqual(0m, 1)", @"0m.Should().BeLessThanOrEqualTo(1)")]
    [InlineData(@"ClassicAssert.LessOrEqual(0m, 1, ""because"")", @"0m.Should().BeLessThanOrEqualTo(1, ""because"")")]
    [InlineData(@"ClassicAssert.LessOrEqual(0m, 1, ""because"", 2, 3)", @"0m.Should().BeLessThanOrEqualTo(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.LessOrEqual(0f, 1)", @"0f.Should().BeLessThanOrEqualTo(1)")]
    [InlineData(@"ClassicAssert.LessOrEqual(0f, 1, ""because"")", @"0f.Should().BeLessThanOrEqualTo(1, ""because"")")]
    [InlineData(@"ClassicAssert.LessOrEqual(0f, 1, ""because"", 2, 3)", @"0f.Should().BeLessThanOrEqualTo(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.LessOrEqual(0d, 1)", @"0d.Should().BeLessThanOrEqualTo(1)")]
    [InlineData(@"ClassicAssert.LessOrEqual(0d, 1, ""because"")", @"0d.Should().BeLessThanOrEqualTo(1, ""because"")")]
    [InlineData(@"ClassicAssert.LessOrEqual(0d, 1, ""because"", 2, 3)", @"0d.Should().BeLessThanOrEqualTo(1, ""because"", 2, 3)")]

    [InlineData(@"ClassicAssert.True((bool?)false)", @"((bool?)false).Should().BeTrue()")]
    [InlineData(@"ClassicAssert.True((bool?)false, ""because"")", @"((bool?)false).Should().BeTrue(""because"")")]
    [InlineData(@"ClassicAssert.True((bool?)false, ""because"", 0, 1)", @"((bool?)false).Should().BeTrue(""because"", 0, 1)")]
    [InlineData(@"ClassicAssert.True(false)", @"false.Should().BeTrue()")]
    [InlineData(@"ClassicAssert.True(false, ""because"")", @"false.Should().BeTrue(""because"")")]
    [InlineData(@"ClassicAssert.True(false, ""because"", 1, 2)", @"false.Should().BeTrue(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Negative(1)", @"1.Should().BeNegative()")]
    [InlineData(@"ClassicAssert.Negative(1, ""because"")", @"1.Should().BeNegative(""because"")")]
    [InlineData(@"ClassicAssert.Negative(1, ""because"", 1, 2)", @"1.Should().BeNegative(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.NotNull("""")", @""""".Should().NotBeNull()")]
    [InlineData(@"ClassicAssert.NotNull("""", ""because"")", @""""".Should().NotBeNull(""because"")")]
    [InlineData(@"ClassicAssert.NotNull("""", ""because"", 1, 2)", @""""".Should().NotBeNull(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.NotZero(1)", @"1.Should().NotBe(0)")]
    [InlineData(@"ClassicAssert.NotZero(1, ""because"")", @"1.Should().NotBe(0, ""because"")")]
    [InlineData(@"ClassicAssert.NotZero(1, ""because"", 1, 2)", @"1.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.NotZero(1u)", @"1u.Should().NotBe(0)")]
    [InlineData(@"ClassicAssert.NotZero(1u, ""because"")", @"1u.Should().NotBe(0, ""because"")")]
    [InlineData(@"ClassicAssert.NotZero(1u, ""because"", 1, 2)", @"1u.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.NotZero(1L)", @"1L.Should().NotBe(0)")]
    [InlineData(@"ClassicAssert.NotZero(1L, ""because"")", @"1L.Should().NotBe(0, ""because"")")]
    [InlineData(@"ClassicAssert.NotZero(1L, ""because"", 1, 2)", @"1L.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.NotZero(1uL)", @"1uL.Should().NotBe(0)")]
    [InlineData(@"ClassicAssert.NotZero(1uL, ""because"")", @"1uL.Should().NotBe(0, ""because"")")]
    [InlineData(@"ClassicAssert.NotZero(1uL, ""because"", 1, 2)", @"1uL.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.NotZero(1m)", @"1m.Should().NotBe(0)")]
    [InlineData(@"ClassicAssert.NotZero(1m, ""because"")", @"1m.Should().NotBe(0, ""because"")")]
    [InlineData(@"ClassicAssert.NotZero(1m, ""because"", 1, 2)", @"1m.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.NotZero(1.0)", @"1.0.Should().NotBe(0)")]
    [InlineData(@"ClassicAssert.NotZero(1.0, ""because"")", @"1.0.Should().NotBe(0, ""because"")")]
    [InlineData(@"ClassicAssert.NotZero(1.0, ""because"", 1, 2)", @"1.0.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.NotZero(1d)", @"1d.Should().NotBe(0)")]
    [InlineData(@"ClassicAssert.NotZero(1d, ""because"")", @"1d.Should().NotBe(0, ""because"")")]
    [InlineData(@"ClassicAssert.NotZero(1d, ""because"", 1, 2)", @"1d.Should().NotBe(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Null("""")", @""""".Should().BeNull()")]
    [InlineData(@"ClassicAssert.Null("""", ""because"")", @""""".Should().BeNull(""because"")")]
    [InlineData(@"ClassicAssert.Null("""", ""because"", 1, 2)", @""""".Should().BeNull(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Positive(1)", @"1.Should().BePositive()")]
    [InlineData(@"ClassicAssert.Positive(1, ""because"")", @"1.Should().BePositive(""because"")")]
    [InlineData(@"ClassicAssert.Positive(1, ""because"", 1, 2)", @"1.Should().BePositive(""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Zero(1)", @"1.Should().Be(0)")]
    [InlineData(@"ClassicAssert.Zero(1, ""because"")", @"1.Should().Be(0, ""because"")")]
    [InlineData(@"ClassicAssert.Zero(1, ""because"", 1, 2)", @"1.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Zero(1u)", @"1u.Should().Be(0)")]
    [InlineData(@"ClassicAssert.Zero(1u, ""because"")", @"1u.Should().Be(0, ""because"")")]
    [InlineData(@"ClassicAssert.Zero(1u, ""because"", 1, 2)", @"1u.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Zero(1L)", @"1L.Should().Be(0)")]
    [InlineData(@"ClassicAssert.Zero(1L, ""because"")", @"1L.Should().Be(0, ""because"")")]
    [InlineData(@"ClassicAssert.Zero(1L, ""because"", 1, 2)", @"1L.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Zero(1uL)", @"1uL.Should().Be(0)")]
    [InlineData(@"ClassicAssert.Zero(1uL, ""because"")", @"1uL.Should().Be(0, ""because"")")]
    [InlineData(@"ClassicAssert.Zero(1uL, ""because"", 1, 2)", @"1uL.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Zero(1m)", @"1m.Should().Be(0)")]
    [InlineData(@"ClassicAssert.Zero(1m, ""because"")", @"1m.Should().Be(0, ""because"")")]
    [InlineData(@"ClassicAssert.Zero(1m, ""because"", 1, 2)", @"1m.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Zero(1.0)", @"1.0.Should().Be(0)")]
    [InlineData(@"ClassicAssert.Zero(1.0, ""because"")", @"1.0.Should().Be(0, ""because"")")]
    [InlineData(@"ClassicAssert.Zero(1.0, ""because"", 1, 2)", @"1.0.Should().Be(0, ""because"", 1, 2)")]

    [InlineData(@"ClassicAssert.Zero(1d)", @"1d.Should().Be(0)")]
    [InlineData(@"ClassicAssert.Zero(1d, ""because"")", @"1d.Should().Be(0, ""because"")")]
    [InlineData(@"ClassicAssert.Zero(1d, ""because"", 1, 2)", @"1d.Should().Be(0, ""because"", 1, 2)")]
    public Task ClassicAssert_Tests(string code, string fix)
    {
        return Assert(
            $$"""
              using System.Collections.Generic;
              using NUnit.Framework.Legacy;
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
              using System.Collections.Generic;
              using FluentAssertions;
              using NUnit.Framework.Legacy;
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
