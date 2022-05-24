using Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;
using TestHelper;
using Xunit;

namespace Meziantou.FluentAssertionsAnalyzers.Tests;

public sealed class XUnitToFluentAssertionsAnalyzerUnitTests
{
    private static ProjectBuilder CreateProjectBuilder()
    {
        return new ProjectBuilder()
            .WithTargetFramework(TargetFramework.Net6_0)
            .WithAnalyzer<AssertAnalyzer>(id: "MFA001")
            .WithCodeFixProvider<XUnitAssertAnalyzerCodeFixProvider>()
            .AddXUnitApi()
            .AddFluentAssertionsApi();
    }

    private static Task Assert(string sourceCode, string expectedFix)
    {
        return CreateProjectBuilder()
                  .WithSourceCode(sourceCode)
                  .ShouldFixCodeWith(expectedFix)
                  .ValidateAsync();
    }

    [Fact]
    public async Task FixDocumentWithMultipleDiagnostics()
    {
        var test = @"
using Xunit;

class Test
{
    public void MyTest()
    {
        var value = false;
        [|Assert.True(value)|];
        [|Assert.True(value)|];
    }
}
";
        var fix = @"
using FluentAssertions;
using Xunit;

class Test
{
    public void MyTest()
    {
        var value = false;
        value.Should().BeTrue();
        value.Should().BeTrue();
    }
}
";

        await CreateProjectBuilder()
              .WithSourceCode(test)
              .ShouldBatchFixCodeWith(fix)
              .ValidateAsync();
    }

    [Fact]
    public Task Assert_True_Boolean()
    {
        return Assert("""
            using Xunit;
            using FluentAssertions;

            class Test
            {
                public void MyTest()
                {
                    var value = false;
                    [|Assert.True(value)|];
                }
            }
            """, """
            using Xunit;
            using FluentAssertions;

            class Test
            {
                public void MyTest()
                {
                    var value = false;
                    value.Should().BeTrue();
                }
            }
            """);
    }

    [Fact]
    public Task Assert_True_Boolean_String()
    {
        return Assert("""
            using Xunit;
            using FluentAssertions;

            class Test
            {
                public void MyTest()
                {
                    var value = false;
                    [|Assert.True(value, "test")|];
                }
            }
            """, """
            using Xunit;
            using FluentAssertions;

            class Test
            {
                public void MyTest()
                {
                    var value = false;
                    value.Should().BeTrue("test");
                }
            }
            """);
    }

    [Fact]
    public Task Assert_False_Boolean()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var value = false;
        [|Assert.False(value)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var value = false;
        value.Should().BeFalse();
    }
}
");
    }

    [Fact]
    public Task Assert_False_Boolean_String()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var value = false;
        [|Assert.False(value, ""test"")|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var value = false;
        value.Should().BeFalse(""test"");
    }
}
");
    }

    [Fact]
    public Task Assert_False_NullableBoolean()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        bool? value = false;
        [|Assert.False(value)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        bool? value = false;
        value.Should().BeFalse();
    }
}
");
    }

    [Fact]
    public Task Assert_Collection()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<string>();
        [|Assert.Collection(collection, item => { }, item => { })|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<string>();
        collection.Should().SatisfyRespectively(item => { }, item => { });
    }
}
");
    }

    [Fact]
    public Task Assert_Contains_T_IEnumerableT()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        [|Assert.Contains(0, collection)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        collection.Should().Contain(0);
    }
}
");
    }

    [Fact]
    public Task Assert_Contains_IEnumerableT_PredicateT()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        [|Assert.Contains(collection, item => true)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        collection.Should().Contain(item => true);
    }
}
");
    }

    [Fact]
    public Task Assert_DoesNotContain_T_IEnumerableT()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        [|Assert.DoesNotContain(0, collection)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        collection.Should().NotContain(0);
    }
}
");
    }

    [Fact]
    public Task Assert_NotEmpty_IEnumerableT()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        [|Assert.NotEmpty(collection)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        collection.Should().NotBeEmpty();
    }
}
");
    }

    [Fact]
    public Task Assert_Empty_IEnumerableT()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        [|Assert.Empty(collection)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        collection.Should().BeEmpty();
    }
}
");
    }

    [Fact]
    public Task Assert_DoesNotContain_IEnumerableT_PredicateT()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        [|Assert.DoesNotContain(collection, item => true)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        collection.Should().NotContain(item => true);
    }
}
");
    }

    [Fact]
    public Task Assert_Single_IEnumerableT()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        [|Assert.Single(collection)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        collection.Should().ContainSingle();
    }
}
");
    }

    [Fact]
    public Task Assert_Single_IEnumerableT_PredicateT()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        [|Assert.Single(collection, item => true)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        collection.Should().ContainSingle(item => true);
    }
}
");
    }

    [Fact]
    public Task Assert_Equal_T_T()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var obj = new object();
        var expected = new object();
        [|Assert.Equal(expected, obj)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var obj = new object();
        var expected = new object();
        obj.Should().Be(expected);
    }
}
");
    }

    [Fact]
    public Task Assert_Equal_String_String()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Equal("""", """")|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        """".Should().Be("""");
    }
}
");
    }

    [Fact]
    public Task Assert_Equal_Double_Double_Int32()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var precision = 2;
        [|Assert.Equal(1d, 0d, precision)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var precision = 2;
        System.Math.Round(0d, precision).Should().Be(System.Math.Round(1d, precision));
    }
}
");
    }

    [Fact]
    public Task Assert_Equal_Double_Double_Double()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Equal(1d, 0d, 0.1d)|];
    }
}

// From xunit3
namespace Xunit
{
    public static class Assert
    {
        public static void Equal(double expected, double actual, double tolerance) => throw null;
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        0d.Should().BeApproximately(1d, 0.1d);
    }
}

// From xunit3
namespace Xunit
{
    public static class Assert
    {
        public static void Equal(double expected, double actual, double tolerance) => throw null;
    }
}
");
    }

    [Fact]
    public Task Assert_Equal_DateTime_DateTime_TimeSpan()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Equal(default(System.DateTime), default(System.DateTime), System.TimeSpan.Zero)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        default(System.DateTime).Should().BeCloseTo(default(System.DateTime), System.TimeSpan.Zero);
    }
}
");
    }

    [Fact]
    public Task Assert_Equal_IEnumerableT_IEnumerableT()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        var expected = System.Array.Empty<object>();
        [|Assert.Equal(expected, collection)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        var expected = System.Array.Empty<object>();
        collection.Should().Equal(expected);
    }
}
");
    }

    [Fact]
    public Task Assert_Null_Object()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        [|Assert.Null(actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        actual.Should().BeNull();
    }
}
");
    }

    [Fact]
    public Task Assert_NotNull_Object()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        [|Assert.NotNull(actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        actual.Should().NotBeNull();
    }
}
");
    }

    [Fact]
    public Task Assert_Same_Object_Object()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        object expected = null;
        [|Assert.Same(expected, actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        object expected = null;
        actual.Should().BeSameAs(expected);
    }
}
");
    }

    [Fact]
    public Task Assert_NotSame_Object_Object()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        object expected = null;
        [|Assert.NotSame(expected, actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        object expected = null;
        actual.Should().NotBeSameAs(expected);
    }
}
");
    }

    [Fact]
    public Task Assert_IsAssignableFromT_Object()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        [|Assert.IsAssignableFrom<object>(actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        actual.Should().BeAssignableTo<object>();
    }
}
");
    }

    [Fact]
    public Task Assert_IsAssignableFrom_Type_Object()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        [|Assert.IsAssignableFrom(typeof(object), actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        actual.Should().BeAssignableTo(typeof(object));
    }
}
");
    }

    [Fact]
    public Task Assert_IsNotTypeT_Object()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        [|Assert.IsNotType<object>(actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        actual.Should().NotBeOfType<object>();
    }
}
");
    }

    [Fact]
    public Task Assert_IsNotType_Type_Object()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        [|Assert.IsNotType(typeof(object), actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        actual.Should().NotBeOfType(typeof(object));
    }
}
");
    }

    [Fact]
    public Task Assert_IsTypeT_Object()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        [|Assert.IsType<object>(actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        actual.Should().BeOfType<object>();
    }
}
");
    }

    [Fact]
    public Task Assert_IsType_Type_Object()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        [|Assert.IsType(typeof(object), actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        object actual = null;
        actual.Should().BeOfType(typeof(object));
    }
}
");
    }

    [Fact]
    public Task Assert_StartsWith_String_String()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.StartsWith(""a"", ""b"")|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().StartWith(""a"");
    }
}
");
    }

    [Fact]
    public Task Assert_StartsWith_String_String_StringComparisonOrdinal()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.StartsWith(""a"", ""b"", System.StringComparison.Ordinal)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().StartWith(""a"");
    }
}
");
    }

    [Fact]
    public Task Assert_EndsWith_String_String()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.EndsWith(""a"", ""b"")|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().EndWith(""a"");
    }
}
");
    }

    [Fact]
    public Task Assert_EndsWith_String_String_StringComparisonOrdinal()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.EndsWith(""a"", ""b"", System.StringComparison.Ordinal)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().EndWith(""a"");
    }
}
");
    }

    [Fact]
    public Task Assert_Contains_String_String()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Contains(""a"", ""b"")|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().Contain(""a"");
    }
}
");
    }

    [Fact]
    public Task Assert_Contains_String_String_StringComparisonOrdinal()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Contains(""a"", ""b"", System.StringComparison.Ordinal)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().Contain(""a"");
    }
}
");
    }

    [Fact]
    public Task Assert_DoesNotContain_String_String()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.DoesNotContain(""a"", ""b"")|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().NotContain(""a"");
    }
}
");
    }

    [Fact]
    public Task Assert_DoesNotContain_String_String_StringComparisonOrdinal()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.DoesNotContain(""a"", ""b"", System.StringComparison.Ordinal)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().NotContain(""a"");
    }
}
");
    }

    [Fact]
    public Task Assert_Matches_String_String()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Matches(""a"", ""b"")|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().MatchRegex(""a"");
    }
}
");
    }

    [Fact]
    public Task Assert_DoesNotMatch_String_String()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.DoesNotMatch(""a"", ""b"")|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().NotMatchRegex(""a"");
    }
}
");
    }

    [Fact]
    public Task Assert_NotInRange_Int32_Int32()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.NotInRange(0, 1, 2)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        0.Should().NotBeInRange(1, 2);
    }
}
");
    }

    [Fact]
    public Task Assert_InRange_Int32_Int32()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.InRange(0, 1, 2)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        0.Should().BeInRange(1, 2);
    }
}
");
    }

    [Fact]
    public Task Assert_Subset_ISet_ISet()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        System.Collections.Generic.HashSet<int> actual = null;
        System.Collections.Generic.HashSet<int> expected = null;
        [|Assert.Subset(expected, actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        System.Collections.Generic.HashSet<int> actual = null;
        System.Collections.Generic.HashSet<int> expected = null;
        actual.Should().BeSubsetOf(expected);
    }
}
");
    }

    [Fact]
    public Task Assert_Superset_ISet_ISet()
    {
        return Assert(@"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        System.Collections.Generic.HashSet<int> actual = null;
        System.Collections.Generic.HashSet<int> expected = null;
        [|Assert.Superset(expected, actual)|];
    }
}
", @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        System.Collections.Generic.HashSet<int> actual = null;
        System.Collections.Generic.HashSet<int> expected = null;
        expected.Should().BeSubsetOf(actual);
    }
}
");
    }

    [Fact]
    public Task Assert_ThrowT_Action()
    {
        return Assert(@"
using System;
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Throws<Exception>(() => { })|];
    }
}
", @"
using System;
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        new Action(() => { }).Should().ThrowExactly<Exception>();
    }
}
");
    }

    [Fact]
    public Task Assert_ThrowT_Func()
    {
        return Assert(@"
using System;
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Throws<Exception>(() => 0)|];
    }
}
", @"
using System;
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        new Func<object>(() => 0).Should().ThrowExactly<Exception>();
    }
}
");
    }

    [Fact]
    public Task Assert_ThrowAnyT_Action()
    {
        return Assert(@"
using System;
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.ThrowsAny<Exception>(() => { })|];
    }
}
", @"
using System;
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        new Action(() => { }).Should().Throw<Exception>();
    }
}
");
    }

    [Fact]
    public Task Assert_ThrowAnyT_ActionWithoutCast()
    {
        return Assert(
            """
            using System;
            using Xunit;
            using FluentAssertions;

            class Test
            {
                public void MyTest()
                {
                    Action action = () => { };
                    [|Assert.ThrowsAny<Exception>(action)|];
                }
            }
            """,
            """
            using System;
            using Xunit;
            using FluentAssertions;

            class Test
            {
                public void MyTest()
                {
                    Action action = () => { };
                    action.Should().Throw<Exception>();
                }
            }
            """);
    }

    [Fact]
    public Task Assert_ThrowAsyncT_Func()
    {
        return Assert(
            """
            using System;
            using Xunit;
            using FluentAssertions;

            class Test
            {
                public void MyTest()
                {
                    [|Assert.ThrowsAsync<Exception>(async () => { })|];
                }
            }
            """,
            """
            using System;
            using Xunit;
            using FluentAssertions;

            class Test
            {
                public void MyTest()
                {
                    new Func<System.Threading.Tasks.Task>(async () => { }).Should().ThrowExactlyAsync<Exception>();
                }
            }
            """);
    }
}
