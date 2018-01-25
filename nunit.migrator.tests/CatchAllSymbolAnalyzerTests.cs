using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests
{
    public class CatchAllSymbolAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new CatchAllSymbolAnalyzer();
        
        [Test]
        public void GivenTearDownAttribute_ReportsWarning()
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TearDown]
    public void TearDown() {}
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M11",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 6) },
                Message = "There is a change to the logic by which teardown methods are called. " +
                          "See more: https://github.com/nunit/docs/wiki/TearDown-Attribute",
                Severity = DiagnosticSeverity.Warning
            });
        }

        [Test]
        public void GivenSetUpFixtureAttribute_ReportsWarning()
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [SetUpFixture]
    public void SetUpFixture() {}
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M11",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 6) },
                Message = "Now uses OneTimeSetUpAttribute and OneTimeTearDownAttribute to designate higher-level " +
                          "setup and teardown methods. SetUpAttribute and TearDownAttribute are no longer allowed.",
                Severity = DiagnosticSeverity.Warning
            });
        }

        [Test]
        public void GivenTestContext_Reports()
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        var current = TestContext.CurrentContext;
    }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M11",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 23) },
                Message = "The fields available in the TestContext have changed, although the same information " +
                          "remains available as for NUnit V2. See more: https://github.com/nunit/docs/wiki/TestContext",
                Severity = DiagnosticSeverity.Warning
            });
        }

        [Test]
        public void GivenNullOrEmptyStringConstraint_ReportsError()
        {
            string source = @"
using NUnit.Framework;
using NUnit.Framework.Constraints;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.That(""my_str"", new NullOrEmptyStringConstraint());
    }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M10",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 35) },
                Message = "No longer supported. Use 'Assert.That(..., Is.Null.Or.Empty)'",
                Severity = DiagnosticSeverity.Error
            });
        }

        [Test]
        public void GivenEnvironmentCurrentDirectory_ReportsWarning()
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        var dir = System.Environment.CurrentDirectory;
    }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M11",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 19) },
                Message = "No longer set to the directory containing the test assembly. " +
                          "Use TestContext.CurrentContext.TestDirectory to locate that directory.",
                Severity = DiagnosticSeverity.Warning
            });
        }

        [Test]
        public void GivenTestCaseDataThrowsNamedProperty_ReportsError()
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        var data = new TestCaseData().Throws(""System.ArgumentException"");
    }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M10",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 20) },
                Message = "The Throws Named Property is no longer available. Use Assert.Throws or Assert.That " +
                          "in your test case.",
                Severity = DiagnosticSeverity.Error
            });

        }
    }
}