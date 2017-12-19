using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.AssertionsAndConstraints;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Assertion
{
    [TestFixture]
    public class AssertionAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new MemberAccessBasedAnalyzer();

        [TestCase("IsNullOrEmpty", "Assert.That(..., Is.Null.Or.Empty)")]
        [TestCase("IsNotNullOrEmpty", "Assert.That(..., Is.Not.Null.And.Not.Empty)")]
        public void ForGivenMemberAccess_Reports(string v2Method, string expectedPredicate)
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert." + v2Method + @"(""actual"");
    }
}";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M03",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9) },
                Message = $"'Assert.{v2Method}' assertion should be replaced with '{expectedPredicate}'.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [Test]
        public void ForMemberAccessNotFromNUnitTextClass_DoesNotReport()
        {
            var source = @"
using NUnit.Framework;

public static class Assert { public bool IsNullOrEmpty(string text) { return true; } }

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.IsNullOrEmpty(""str"");
    }
}";
            VerifyNoDiagnosticReported(source);
        }
    }
}