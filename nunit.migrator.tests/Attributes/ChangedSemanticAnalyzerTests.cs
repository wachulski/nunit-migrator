using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    public class ChangedSemanticAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ChangedSemanticAnalyzer();

        [Test]
        public void ForNotSystemAttributes_DoesNotReport()
        {
            string source = @"
using NUnit.Framework;

public class MTAThreadAttribute : Attribute {}

[TestFixture]
public class TestClass
{
    [Test]
    [MTAThreadAttribute]
    void TestMethod() {}
}
";
            VerifyNoDiagnosticReported(source);
        }

        [TestCase("System.MTAThread", "NUnit.Framework.RequiresMTAAttribute")]
        [TestCase("System.STAThread", "NUnit.Framework.RequiresSTAAttribute")]
        public void ForSystemAttributesThatChangedTheirMeaning_Reports(string attr, string expectedMeaningAttr)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    [" + attr + @"]
    void TestMethod() {}
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M06",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 6) },
                Message = $"'{attr}' attribute is no longer treated as '{expectedMeaningAttr}'.",
                Severity = DiagnosticSeverity.Error
            });
        }
    }
}