using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Tests.Helpers;
using DiagnosticVerifier = NUnit.Migrator.Tests.Helpers.DiagnosticVerifier;

namespace NUnit.Migrator.Tests.ExceptionExpectancy
{
    [TestFixture]
    public class ExceptionExpectancyAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExceptionExpectancyAnalyzer();
        }

        [TestCase("")]
        [TestCase("(typeof(System.InvalidOperationException))")]
        [TestCase("(ExpectedException = typeof(System.InvalidOperationException))")]
        [TestCase("(\"System.InvalidOperationException\")")]
        [TestCase("(ExpectedExceptionName = \"System.InvalidOperationException\")")]
        public void ForExpectedExceptionSpecified_FindsMigrationPossible(string attrArguments)
        {
            var source = @"
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
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NUnit2Migra001",
                Locations = new[] {new DiagnosticResultLocation("Test0.cs", 7, 12)},
                Message =
                    "Method 'TestMethod' contains 'ExpectedException' attribute and/or " +
                    "'TestCase' exception related arguments which should be replaced with Assert.Throws<T>.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [TestCase("ExpectedException = typeof(System.InvalidOperationException)")]
        [TestCase("ExpectedExceptionName = \"System.InvalidOperationException\"")]
        public void ForTestCaseContainingExceptionProperties_FindsMigrationPossible(string attrArguments)
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(" + attrArguments + @")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NUnit2Migra001",
                Locations = new[] {new DiagnosticResultLocation("Test0.cs", 7, 6)},
                Message =
                    "Method 'TestMethod' contains 'ExpectedException' attribute and/or " +
                    "'TestCase' exception related arguments which should be replaced with Assert.Throws<T>.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [Test]
        public void ForExpectedExceptionAttributeNotFromNUnitFramework_FindsNoMigration()
        {
            const string source = @"
[NUnit.Framework.TestFixture]
public class TestClass
{
    [NUnit.Framework.Test, ExpectedException]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }

    private class ExpectedExceptionAttribute : System.Attribute { }
}";
            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void ForExceptionExpectancyNotSpecifiedAtAttributeLevel_FindsNoMigration()
        {
            const string source = @"
using NUnit.Framework;

public class TestClass
{
    [Test]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void ForTestCaseLackingExceptionProperties_FindsNoMigration()
        {
            const string source = @"
using NUnit.Framework;

public class TestClass
{
    [TestCase]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void ForTestCaseAttributeNotFromNUnitFramework_FindsNoMigration()
        {
            const string source = @"
[NUnit.Framework.TestFixture]
public class TestClass
{
    [TestCase(ExpectedExceptionName = ""SomeException""]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }

    private class TestCaseAttribute : System.Attribute { public string ExpectedExceptionName { get; set; } }
}";
            VerifyNoDiagnosticReported(source);
        }
    }
}