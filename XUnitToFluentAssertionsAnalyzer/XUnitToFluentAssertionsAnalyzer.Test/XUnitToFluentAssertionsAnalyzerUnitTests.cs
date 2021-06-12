using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace XUnitToFluentAssertionsAnalyzer.Test
{
    public class XUnitToFluentAssertionsAnalyzerUnitTest
    {
        private static ProjectBuilder CreateProjectBuilder()
        {
            return new ProjectBuilder()
                .WithAnalyzer<XUnitAssertAnalyzer>()
                .WithCodeFixProvider<XUnitAssertAnalyzerCodeFixProvider>()
                .AddXUnitApi()
                .WithTargetFramework(TargetFramework.NetStandard2_1)
                .AddFluentAssertionsApi();
        }

        [Fact]
        public async Task FixDocument()
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
using Xunit;
using FluentAssertions;

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
        public async Task Assert_True_Boolean()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_True_Boolean_String()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var value = false;
        [|Assert.True(value, ""test"")|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var value = false;
        value.Should().BeTrue(""test"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_False_Boolean()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_False_Boolean_String()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_False_NullableBoolean()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Collection()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Contains_T_IEnumerableT()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Contains_IEnumerableT_PredicateT()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_DoesNotContain_T_IEnumerableT()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_NotEmpty_IEnumerableT()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Empty_IEnumerableT()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_DoesNotContain_IEnumerableT_PredicateT()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Single_IEnumerableT()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Single_IEnumerableT_PredicateT()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Equal_T_T()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Equal_String_String()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Equal("""", """")|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        """".Should().Be("""");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Equal_Double_Double_Int32()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Equal_Double_Double_Double()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Equal_DateTime_DateTime_TimeSpan()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Equal(default(System.DateTime), default(System.DateTime), System.TimeSpan.Zero)|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        default(System.DateTime).Should().BeCloseTo(default(System.DateTime), (int)System.TimeSpan.Zero.TotalMilliseconds);
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Equal_IEnumerableT_IEnumerableT()
        {
            var test = @"
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
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        var collection = System.Array.Empty<object>();
        var expected = System.Array.Empty<object>();
        collection.Should().BeEquivalentTo(expected);
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Null_Object()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_NotNull_Object()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Same_Object_Object()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_NotSame_Object_Object()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_IsAssignableFromT_Object()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_IsAssignableFrom_Type_Object()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_IsNotTypeT_Object()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_IsNotType_Type_Object()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_IsTypeT_Object()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_IsType_Type_Object()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_StartsWith_String_String()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.StartsWith(""a"", ""b"")|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().StartWith(""a"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }
        
        [Fact]
        public async Task Assert_StartsWith_String_String_StringComparisonOrdinal()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.StartsWith(""a"", ""b"", System.StringComparison.Ordinal)|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().StartWith(""a"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_EndsWith_String_String()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.EndsWith(""a"", ""b"")|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().EndWith(""a"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }
        
        [Fact]
        public async Task Assert_EndsWith_String_String_StringComparisonOrdinal()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.EndsWith(""a"", ""b"", System.StringComparison.Ordinal)|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().EndWith(""a"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Contains_String_String()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Contains(""a"", ""b"")|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().Contain(""a"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }
        
        [Fact]
        public async Task Assert_Contains_String_String_StringComparisonOrdinal()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Contains(""a"", ""b"", System.StringComparison.Ordinal)|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().Contain(""a"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_DoesNotContain_String_String()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.DoesNotContain(""a"", ""b"")|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().NotContain(""a"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }
        
        [Fact]
        public async Task Assert_DoesNotContain_String_String_StringComparisonOrdinal()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.DoesNotContain(""a"", ""b"", System.StringComparison.Ordinal)|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().NotContain(""a"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Matches_String_String()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.Matches(""a"", ""b"")|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().MatchRegex(""a"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_DoesNotMatch_String_String()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.DoesNotMatch(""a"", ""b"")|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        ""b"".Should().NotMatchRegex(""a"");
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_NotInRange_Int32_Int32()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.NotInRange(0, 1, 2)|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        0.Should().NotBeInRange(1, 2);
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }
        
        [Fact]
        public async Task Assert_InRange_Int32_Int32()
        {
            var test = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        [|Assert.InRange(0, 1, 2)|];
    }
}
";
            var fix = @"
using Xunit;
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        0.Should().BeInRange(1, 2);
    }
}
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Subset_ISet_ISet()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_Superset_ISet_ISet()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_ThrowT_Action()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }
        
        [Fact]
        public async Task Assert_ThrowT_Func()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_ThrowAnyT_Action()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_ThrowAnyT_ActionWithoutCast()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }

        [Fact]
        public async Task Assert_ThrowAsyncT_Func()
        {
            var test = @"
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
";
            var fix = @"
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
";

            await CreateProjectBuilder()
                  .WithSourceCode(test)
                  .ShouldFixCodeWith(fix)
                  .ValidateAsync();
        }
    }
}
