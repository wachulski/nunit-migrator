using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    public class TrivialAttributeArgumentReplaceAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new TrivialAttributeArgumentReplaceAnalyzer();

        [TestCase("Result", "ExpectedResult")]
        public void ForDeprecatedReplaceableAttributeArguments_Reports(string argument, string expectedCorrection)
        {
            string source = @"
using NUnit.Framework;
using NU = NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(" + argument + @" = 1)]
    void TestMethod() {}
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M08",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 15) },
                Message = $"'{argument}' " +
                          "attribute argument is deprecated and should be replaced with " +
                          $"'{expectedCorrection}'.",
                Severity = DiagnosticSeverity.Error
            });
        }
    }
}