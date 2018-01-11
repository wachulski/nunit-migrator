using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    [TestFixture]
    public class TrivialAttributeReplaceFixProviderTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new TrivialAttributeReplaceAnalyzer();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new TrivialAttributeReplaceFixProvider();

        [TestCase("RequiresMTA", "Apartment(System.Threading.ApartmentState.MTA)")]
        [TestCase("RequiresSTA", "Apartment(System.Threading.ApartmentState.STA)")]
        [TestCase("TestFixtureSetUpAttribute", "OneTimeSetUp")]
        [TestCase("TestFixtureTearDown", "OneTimeTearDown")]
        [TestCase("NUnit.Framework.RequiresMTA", "Apartment(System.Threading.ApartmentState.MTA)")]
        [TestCase("NUnit.Framework.RequiresSTAAttribute", "Apartment(System.Threading.ApartmentState.STA)")]
        [TestCase("NU.TestFixtureSetUp", "OneTimeSetUp")]
        [TestCase("TestFixtureTearDownAttribute", "OneTimeTearDown")]
        public void ForDeprecatedReplaceableAttributes_Fixes(string attr, string expectedFix)
        {
            string source = @"
using NUnit.Framework;
using NU = NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    [" + attr + @"]
    void TestMethod()
    {
    }
}
";

            string expected = @"
using NUnit.Framework;
using NU = NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    [" + expectedFix + @"]
    void TestMethod()
    {
    }
}
";

            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }
    }
}