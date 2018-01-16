using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    public class IgnoreReasonAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new IgnoreReasonAnalyzer();

        [TestCase("Ignore(\"ignoring reason\")")]
        [TestCase("Ignore(CommonIgnoringReasonConst)")]
        [TestCase("NUnit.Framework.Ignore(\"ignoring reason\")")]
        public void GivenIgnoreAttributeSpecifyingReasonLiteral_DoesNotReport(string attribute)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    const string CommonIgnoringReasonConst = ""ignoring reason"";

    [Test, " + attribute + @"]
    void TestMethod()
    {
    }
}
";

            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void GivenTestCaseOrTestFixtureWithNoIgnoringIndication_DoesNotReport()
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(1)]
    [TestCase(2)]
    void TestMethod(int x)
    {
    }
}
";

            VerifyNoDiagnosticReported(source);
        }

        [TestCase("Ignore", "Ignore")]
        [TestCase("Ignore()", "Ignore")]
        [TestCase("NUnit.Framework.Ignore", "NUnit.Framework.Ignore")]
        [TestCase("NUnit.Framework.Ignore()", "NUnit.Framework.Ignore")]
        public void GivenIgnoreAttributeLackingReasonLiteral_Reports(string attribute, string expectedNameToReport)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, " + attribute + @"]
    void TestMethod()
    {
    }
}
";

            VerifyCSharpDiagnostic(source, new DiagnosticResult()
            {
                Id = "NU2M09",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 12) },
                Message = $"'{expectedNameToReport}' attribute should provide ignoring reason.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [TestCaseSource(nameof(TestCaseOrFixtureArgumentsCases))]
        public void GivenTestCaseWithIgnoringIndicationAndNoReasonSpecified_Reports(string args)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    const bool ShouldIgnore = true;

    [TestCase(" + args + @")]
    void TestMethod()
    {
    }
}
";

            VerifyCSharpDiagnostic(source, new DiagnosticResult()
            {
                Id = "NU2M09",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 6) },
                Message = "'TestCase' if being ignored should provide reason in its 'Ignore' argument.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [TestCaseSource(nameof(TestCaseOrFixtureArgumentsCases))]
        public void GivenTestFixtureWithIgnoringIndicationAndNoReasonSpecified_Reports(string args)
        {
            string source = @"
using NUnit.Framework;

[TestFixture(" + args + @")]
public class TestClass
{
    [Test]
    void TestMethod()
    {
    }
}
";

            VerifyCSharpDiagnostic(source, new DiagnosticResult()
            {
                Id = "NU2M09",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 4, 2) },
                Message = "'TestFixture' if being ignored should provide reason in its 'Ignore' argument.",
                Severity = DiagnosticSeverity.Error
            });
        }

        private static readonly string[] TestCaseOrFixtureArgumentsCases = 
        {
            "Ignore = true",
            "Ignored = true",
            "Ignore = false",
            "Ignored = false",
            "Ignore = ShouldIgnore",
            "Ignored = ShouldIgnore",
            "Ignore = true, Ignored = true",
            "Ignore = true, Ignored = false",
            "Ignore = false, Ignored = true",
            "Ignore = false, Ignored = false",
            "Ignored = true, Ignore = true",
            "Ignored = false, Ignore = true",
            "Ignore = true, IgnoreReason = \"some reason\"",
            "Ignored = true, IgnoreReason = \"some reason\"",
            "Ignore = true, IgnoreReason = \"some reason\", Ignored = true",
            "IgnoreReason = \"some reason\"",
        };
    }
}