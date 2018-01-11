using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    public class TrivialAttributeReplaceAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new TrivialAttributeReplaceAnalyzer();

        [Test]
        public void ForNotNUnitAttributes_DoesNotReport()
        {
            string source = @"
using NUnit.Framework;

public class RequiresMTAAttribute : Attribute {}

[TestFixture]
public class TestClass
{
    [Test]
    [RequiresMTA]
    void TestMethod() {}
}
";
            VerifyNoDiagnosticReported(source);
        }

        [TestCase("RequiresMTA", "Apartment(System.Threading.ApartmentState.MTA)", "RequiresMTA")]
        [TestCase("RequiresSTA", "Apartment(System.Threading.ApartmentState.STA)", "RequiresSTA")]
        [TestCase("TestFixtureSetUpAttribute", "OneTimeSetUp", "TestFixtureSetUp")]
        [TestCase("TestFixtureTearDown", "OneTimeTearDown", "TestFixtureTearDown")]
        [TestCase("NUnit.Framework.RequiresMTA", "Apartment(System.Threading.ApartmentState.MTA)", "RequiresMTA")]
        [TestCase("NUnit.Framework.RequiresSTAAttribute", "Apartment(System.Threading.ApartmentState.STA)", "RequiresSTA")]
        [TestCase("NU.TestFixtureSetUp", "OneTimeSetUp", "TestFixtureSetUp")]
        [TestCase("TestFixtureTearDownAttribute", "OneTimeTearDown", "TestFixtureTearDown")]
        public void ForDeprecatedReplaceableAttributes_Reports(string attr, 
            string expectedCorrectionToBeProposed, string expectedAttributeNameInMessageToBeRectified)
        {
            string source = @"
using NUnit.Framework;
using NU = NUnit.Framework;

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
                Id = "NU2M07",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 6) },
                Message = $"'{expectedAttributeNameInMessageToBeRectified}' " +
                          "attribute is deprecated and should be replaced with " +
                          $"'{expectedCorrectionToBeProposed}'.",
                Severity = DiagnosticSeverity.Error
            });
        }
    }
}