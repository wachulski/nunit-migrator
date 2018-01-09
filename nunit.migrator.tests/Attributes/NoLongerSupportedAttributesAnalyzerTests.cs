using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    public class NoLongerSupportedAttributesAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() 
            => new NoLongerSupportedAttributesAnalyzer();

        [Test]
        public void ForNotNUnitAttributes_DoesNotReport()
        {
            string source = @"
using NUnit.Framework;

internal class SuiteAttribute : Attribute {}

[TestFixture]
public class TestClass
{
    [Suite]
    public void Property { get; set; }
}
";
            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void ForSuiteAttribute_Reports()
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Suite]
    public void Property { get; set; }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M05",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 6) },
                Message = "'Suite' attribute is no longer supported.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [Test]
        public void ForRequiredAddinAttribute_Reports()
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [assembly:RequiredAddin(""addin-name"")]
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M05",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 15) },
                Message = "'RequiredAddin' attribute is no longer supported.",
                Severity = DiagnosticSeverity.Error
            });
        }
    }
}