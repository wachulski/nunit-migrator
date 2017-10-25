using NUnit.Framework;

namespace NUnit.Migrator.Tests.ExceptionExpectancy
{
    [TestFixture]
    public class MixedExpectedExceptionAndTestCasesFixture : ExceptionExpectancyFixProviderTests
    {
        [Test]
        public void For2TCs_When1TCExpectsSpecificException_FixesInto2TestMethodsAndProperExceptionAsserts()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [ExpectedException]
    [TestCase(1)]
    [TestCase(2, ExpectedException = typeof(System.ArgumentException))]
    public void TestMethod(int x)
    {
        throw new System.Exception();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    public void TestMethod_ShouldThrowException(int x)
    {
        Assert.Throws<System.Exception>(() =>
        {
            throw new System.Exception();
        });
    }

    [TestCase(2)]
    public void TestMethod_ShouldThrowArgumentException(int x)
    {
        Assert.Throws<System.ArgumentException>(() =>
        {
            throw new System.Exception();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void For2TCs_WhenSecondOverridesPropertiesOfFirst_FixesInto2TestMethodsAndProperExceptionAsserts()
        {
            var source = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [ExpectedException(typeof(InvalidCastException), ExpectedMessage = ""Msg!"", MatchType=MessageMatch.Regex)]
    [TestCase(1)]
    [TestCase(2, ExpectedException = typeof(System.ArgumentException), ExpectedMessage = ""Overwrite!"", MatchType=MessageMatch.Contains)]
    public void TestMethod(int x)
    {
        throw new Exception();
    }
}";
            var expected = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    public void TestMethod_ShouldThrowInvalidCastException(int x)
    {
        var ex = Assert.Throws<InvalidCastException>(() =>
        {
            throw new Exception();
        });
        Assert.That(ex.Message, Does.Match(""Msg!""));
    }

    [TestCase(2)]
    public void TestMethod_ShouldThrowArgumentException(int x)
    {
        var ex = Assert.Throws<System.ArgumentException>(() =>
        {
            throw new Exception();
        });
        Assert.That(ex.Message, Does.Contain(""Overwrite!""));
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }

        [Test]
        public void For2TCs_WhenBothTestCasesDoNotOverwriteExceptionProperties_CreatesASingleClusterOfTestCases()
        {
            var source = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [ExpectedException(typeof(OverflowException))]
    [TestCase(1)]
    [TestCase(2)]
    public void TestMethod(int x)
    {
        throw new OverflowException();
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
        Assert.Throws<OverflowException>(() =>
        {
            throw new OverflowException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void For7TCs_WhenEvenNumberedTCsOverwriteExceptionProperties_CreatesProperClustersOfTestCases()
        {
            var source = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [ExpectedException(typeof(OverflowException), ExpectedMessage=""Msg!"")]
    [TestCase(1)]
    [TestCase(2, ExpectedExceptionName=""System.InvalidCastException"", MatchType=MessageMatch.Contains)]
    [TestCase(3)]
    [TestCase(4, ExpectedExceptionName=""System.InvalidCastException"", MatchType=MessageMatch.Contains)]
    [TestCase(5)]
    [TestCase(6, ExpectedException=typeof(ArgumentException))]
    [TestCase(7)]
    [TestCase(8, ExpectedMessage=""Msg!+"")]
    public void TestMethod(int x)
    {
        throw new Exception();
    }
}";
            var expected = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    [TestCase(3)]
    [TestCase(5)]
    [TestCase(7)]
    public void TestMethod_ShouldThrowOverflowException(int x)
    {
        var ex = Assert.Throws<OverflowException>(() =>
        {
            throw new Exception();
        });
        Assert.That(ex.Message, Is.EqualTo(""Msg!""));
    }

    [TestCase(2)]
    [TestCase(4)]
    public void TestMethod_ShouldThrowInvalidCastException(int x)
    {
        var ex = Assert.Throws<System.InvalidCastException>(() =>
        {
            throw new Exception();
        });
        Assert.That(ex.Message, Does.Contain(""Msg!""));
    }

    [TestCase(6)]
    public void TestMethod_ShouldThrowArgumentException(int x)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            throw new Exception();
        });
        Assert.That(ex.Message, Is.EqualTo(""Msg!""));
    }

    [TestCase(8)]
    public void TestMethod_ShouldThrowOverflowException2(int x)
    {
        var ex = Assert.Throws<OverflowException>(() =>
        {
            throw new Exception();
        });
        Assert.That(ex.Message, Is.EqualTo(""Msg!+""));
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }
    }
}