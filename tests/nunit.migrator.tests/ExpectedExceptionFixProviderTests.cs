using System.Security.Cryptography.X509Certificates;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using TestHelper;

namespace NUnit.Migrator.Tests
{
    [TestFixture]
    public class ExpectedExceptionFixProviderTests : CodeFixVerifier
    {
        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ExpectedExceptionFixProvider();
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ExpectedExceptionAnalyzer();

        public class BareExpectedExceptionWithNoArgumentsFixture : ExpectedExceptionFixProviderTests
        {
            [Test]
            public void FixesToAssertThrowsOfSystemException()
            {
                string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
                string expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }
}";
                VerifyCSharpFix(source, expected);
            }
        }

        public class AssertedExceptionTypeDefinedFixture : ExpectedExceptionFixProviderTests
        {
            [TestCase("(typeof(System.InvalidOperationException))")]
            [TestCase("(ExpectedException = typeof(System.InvalidOperationException))")]
            [TestCase("(\"System.InvalidOperationException\")")]
            [TestCase("(ExpectedExceptionName = \"System.InvalidOperationException\")")]
            public void ForTypeNameFullyQualified_FixesToAssertThrowsOfTypeFullyQualified(string attrArguments)
            {
                string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException" + attrArguments + @"]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
                string expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
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

            [TestCase("(typeof(ArgumentException))")]
            [TestCase("(ExpectedException = typeof(ArgumentException))")]
            [TestCase("(\"ArgumentException\")")]
            [TestCase("(ExpectedExceptionName = \"ArgumentException\")")]
            public void ForTypeNameSimpleWithoutNamespace_FixesToAssertThrowsOfSimpleTypeName(string attrArguments)
            {
                string source = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException" + attrArguments + @"]
    public void TestMethod()
    {
        throw new ArgumentException();
    }
}";
                string expected = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            throw new ArgumentException();
        });
    }
}";
                VerifyCSharpFix(source, expected);
            }

            [TestCase("(\"\")")]
            [TestCase("(ExpectedExceptionName = \"\")")]
            [TestCase("(ExpectedExceptionName = null)")]
            public void ForTypeDefinedExplicitlyAsNullOrEmpty_FixesNothing(string attrArguments)
            {
                string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException" + attrArguments + @"]
    public void TestMethod()
    {
        throw new ArgumentException();
    }
}";
                VerifyCSharpFix(source, source);
            }

            [TestCase("(typeof(FirstException), ExpectedExceptionName = \"LastException\")")]
            [TestCase("(ExpectedException = typeof(FirstException), ExpectedExceptionName = \"LastException\")")]
            [TestCase(
                "(typeof(FirstException), ExpectedException = typeof(MiddleException), ExpectedExceptionName = \"LastException\")")]
            [TestCase("(\"FirstException\", ExpectedException = typeof(LastException))")]
            [TestCase("(ExpectedExceptionName = \"FirstException\", ExpectedException = typeof(LastException))")]
            [TestCase(
                "(\"FirstException\", ExpectedExceptionName = \"MiddleException\", ExpectedException = typeof(LastException))")]
            public void ForTypeNamesDefinedMultipleTimes_FixesToAssertThrowsOfLastDefinedType(string attrArguments)
            {
                string source = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException" + attrArguments + @"]
    public void TestMethod()
    {
        throw new LastException();
    }
}

public class FirstExceptionAttribute : ExceptionAttribute {}
public class MiddleExceptionAttribute : ExceptionAttribute {}
public class LastExceptionAttribute : ExceptionAttribute {}";

                string expected = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<LastException>(() =>
        {
            throw new LastException();
        });
    }
}

public class FirstExceptionAttribute : ExceptionAttribute {}
public class MiddleExceptionAttribute : ExceptionAttribute {}
public class LastExceptionAttribute : ExceptionAttribute {}";

                VerifyCSharpFix(source, expected);
            }
        }

        public class ExceptionMessageFixture : ExpectedExceptionFixProviderTests
        {
            [Test]
            public void FixesToAssertThrowsWithVariableAssignmentAndMessageExactCheck()
            {
                string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException(ExpectedMessage = ""Invalid op message text."")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException(""Invalid op message text."");
    }
}";
                string expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        var ex = Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException(""Invalid op message text."");
        });
        Assert.That(ex.Message, Is.EqualTo(""Invalid op message text.""));
    }
}";
                VerifyCSharpFix(source, expected);
            }

            [TestCase("MessageMatch.Exact", "EqualTo")]
            [TestCase("MessageMatch.Contains", "StringContaining")]
            [TestCase("MessageMatch.Regex", "StringMatching")]
            public void ForMatchTypeSpecified_FixesToMessageStringSpecificCheck(string attributeArg,
                string expectedAssertionForm)
            {
                string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException(ExpectedMessage = ""Invalid op message text."", MatchType=" + attributeArg + @")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException(""Invalid op message text."");
    }
}";
                string expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        var ex = Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException(""Invalid op message text."");
        });
        Assert.That(ex.Message, Is." + expectedAssertionForm + @"(""Invalid op message text.""));
    }
}";
                VerifyCSharpFix(source, expected);
            }
        }

        public class UserMessageFixture : ExpectedExceptionFixProviderTests
        {
            [Test]
            public void FixesToAssertThrowsWithCustomFailureMessageProvidedAsInAttributeArg()
            {
                string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException(UserMessage = ""This test failed."")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
                string expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException();
        }, ""This test failed."");
    }
}";
                VerifyCSharpFix(source, expected);
            }
        }
    }
}