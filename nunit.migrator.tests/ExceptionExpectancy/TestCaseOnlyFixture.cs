using NUnit.Framework;

namespace NUnit.Migrator.Tests.ExceptionExpectancy
{
    [TestFixture]
    public class TestCaseOnlyFixture : ExceptionExpectancyFixProviderTests
    {
        [Test]
        public void For1TC_FixesToAssertThrowsProvidedExceptionType()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(ExpectedException = typeof(System.InvalidOperationException))]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase]
    public void TestMethod()
    {
        Assert.Throws<System.InvalidOperationException>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void For2TCs_When1ExpectsException_ExtractsNewTestMethodAndAssertsThereOnExceptionTypeProvided()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    [TestCase(2, ExpectedExceptionName = ""System.InvalidOperationException"", Ignore = true)]
    public void TestMethod(int x)
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    public void TestMethod(int x)
    {
        throw new System.InvalidOperationException();
    }

    [TestCase(2, Ignore = true)]
    public void TestMethod_ShouldThrowInvalidOperationException(int x)
    {
        Assert.Throws<System.InvalidOperationException>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }
        
        [Test]
        public void For3TCs_When1ExpectsException_ExtractsNewTestMethodAndAssertsThereOnExceptionTypeProvided()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Ignore]
    [TestCase(1)]
    [TestCase(2, ExpectedExceptionName = ""System.InvalidOperationException"")]
    [TestCase(3)]
    public void TestMethod(int x)
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Ignore]
    [TestCase(1)]
    [TestCase(3)]
    public void TestMethod(int x)
    {
        throw new System.InvalidOperationException();
    }

    [Ignore]
    [TestCase(2)]
    public void TestMethod_ShouldThrowInvalidOperationException(int x)
    {
        Assert.Throws<System.InvalidOperationException>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void For1TC_WhenExpectedMessageProvided_AssertsOnMessageWithStringExactCheck()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(ExpectedExceptionName =""System.ArgumentException"", ExpectedMessage = ""Invalid op message text."")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException(""Invalid op message text."");
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase]
    public void TestMethod()
    {
        var ex = Assert.Throws<System.ArgumentException>(() =>
        {
            throw new System.InvalidOperationException(""Invalid op message text."");
        });
        Assert.That(ex.Message, Is.EqualTo(""Invalid op message text.""));
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }

        [TestCase("MessageMatch.Exact", "Is.EqualTo")]
        [TestCase("MessageMatch.Contains", "Does.Contain")]
        [TestCase("MessageMatch.Regex", "Does.Match")]
        [TestCase("MessageMatch.StartsWith", "Does.StartWith")]
        public void For1TC_WhenExpectedMessageAndMatchTypeProvided_AssertsOnMessageWithProperStringCheck(
            string attributeArg, string expectedAssertionForm)
        {
            var source = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(ExpectedException=typeof(ArgumentException), ExpectedMessage = ""Msg."", MatchType=" + attributeArg +
                         @")]
    public void TestMethod()
    {
        throw new ArgumentException(""Msg."");
    }
}";
            var expected = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase]
    public void TestMethod()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            throw new ArgumentException(""Msg."");
        });
        Assert.That(ex.Message, " + expectedAssertionForm + @"(""Msg.""));
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }

        [Test]
        public void For2TCs_When2ExpectSameExceptionWithSameExceptionMessage_FixesToProperAssertThrows()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1, ExpectedException = typeof(System.InvalidOperationException), ExpectedMessage = ""msg"")]
    [TestCase(2, ExpectedExceptionName = ""System.InvalidOperationException"", ExpectedMessage = ""msg"", MatchType = MessageMatch.Exact)]
    public void TestMethod(int x)
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    [TestCase(2)]
    public void TestMethod(int x)
    {
        var ex = Assert.Throws<System.InvalidOperationException>(() =>
        {
            throw new System.InvalidOperationException();
        });
        Assert.That(ex.Message, Is.EqualTo(""msg""));
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }

        [Test]
        public void For2TCs_When2ExpectSameExceptionWithDifferentExceptionMessage_FixesTo2MethodsIncrementallyNamed()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1, ExpectedException = typeof(System.InvalidOperationException), ExpectedMessage = ""msg"")]
    [TestCase(2, ExpectedExceptionName = ""System.InvalidOperationException"", ExpectedMessage = ""msg!"")]
    public void TestMethod(int x)
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    public void TestMethod_ShouldThrowInvalidOperationException(int x)
    {
        var ex = Assert.Throws<System.InvalidOperationException>(() =>
        {
            throw new System.InvalidOperationException();
        });
        Assert.That(ex.Message, Is.EqualTo(""msg""));
    }

    [TestCase(2)]
    public void TestMethod_ShouldThrowInvalidOperationException2(int x)
    {
        var ex = Assert.Throws<System.InvalidOperationException>(() =>
        {
            throw new System.InvalidOperationException();
        });
        Assert.That(ex.Message, Is.EqualTo(""msg!""));
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }

        [Test]
        public void For2TCs_When2ExpectOtherExceptions_FixesTo2MethodsWithAdequateNames()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1, ExpectedException = typeof(System.InvalidOperationException))]
    [TestCase(2, ExpectedExceptionName = ""System.ArgumentException"")]
    public void TestMethod(int x)
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    public void TestMethod_ShouldThrowInvalidOperationException(int x)
    {
        Assert.Throws<System.InvalidOperationException>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }

    [TestCase(2)]
    public void TestMethod_ShouldThrowArgumentException(int x)
    {
        Assert.Throws<System.ArgumentException>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void For3TCs_When1ExceptionTypeWrappedByOther2_FixesTo2MethodsWithAdequateNames()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1, ExpectedException = typeof(System.InvalidOperationException))]
    [TestCase(2, ExpectedExceptionName = ""System.ArgumentException"")]
    [TestCase(3, ExpectedException = typeof(System.InvalidOperationException))]
    public void TestMethod(int x)
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    [TestCase(3)]
    public void TestMethod_ShouldThrowInvalidOperationException(int x)
    {
        Assert.Throws<System.InvalidOperationException>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }

    [TestCase(2)]
    public void TestMethod_ShouldThrowArgumentException(int x)
    {
        Assert.Throws<System.ArgumentException>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void For5TCs_When2ExpectExceptionAndOther3DoNot_FixesTo3MethodsWith2ConstitutingAnExceptionGroup()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1, ExpectedException = typeof(System.InvalidOperationException))]
    [TestCase(2, Explicit = true)]
    [TestCase(3)]
    [TestCase(4, ExpectedException = typeof(System.InvalidOperationException))]
    [TestCase(5), Ignore]
    public void TestMethod(int x)
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(2, Explicit = true)]
    [TestCase(3)]
    [TestCase(5), Ignore]
    public void TestMethod(int x)
    {
        throw new System.InvalidOperationException();
    }

    [TestCase(1)]
    [TestCase(4)]
    [Ignore]
    public void TestMethod_ShouldThrowInvalidOperationException(int x)
    {
        Assert.Throws<System.InvalidOperationException>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void For7TCs_When2DoNotExpectExceptionsAndOtherForm3ExceptionGroups_FixesTo4TestMethods()
        {
            var source = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3, ExpectedException = typeof(InvalidOperationException))]
    [TestCase(4, ExpectedException = typeof(InvalidOperationException))]
    [TestCase(5, ExpectedException = typeof(InvalidOperationException), ExpectedMessage=""Error:"", MatchType=MessageMatch.Contains)]
    [TestCase(6, ExpectedException = typeof(ArgumentException))]
    [TestCase(7, ExpectedException = typeof(ArgumentException))]
    public void TestMethod(int x)
    {
        throw new InvalidOperationException();
    }
}";
            var expected = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    [TestCase(2)]
    public void TestMethod(int x)
    {
        throw new InvalidOperationException();
    }

    [TestCase(3)]
    [TestCase(4)]
    public void TestMethod_ShouldThrowInvalidOperationException(int x)
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            throw new InvalidOperationException();
        });
    }

    [TestCase(5)]
    public void TestMethod_ShouldThrowInvalidOperationException2(int x)
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            throw new InvalidOperationException();
        });
        Assert.That(ex.Message, Does.Contain(""Error:""));
    }

    [TestCase(6)]
    [TestCase(7)]
    public void TestMethod_ShouldThrowArgumentException(int x)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            throw new InvalidOperationException();
        });
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }
    }
}