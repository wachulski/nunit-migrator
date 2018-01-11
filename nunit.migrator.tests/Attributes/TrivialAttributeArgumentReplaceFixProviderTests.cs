using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    [TestFixture]
    public class TrivialAttributeArgumentReplaceFixProviderTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new TrivialAttributeArgumentReplaceAnalyzer();

        protected override CodeFixProvider GetCSharpCodeFixProvider() =>
            new TrivialAttributeArgumentReplaceFixProvider();

        [TestCase("Result = 1", "ExpectedResult = 1")]
        public void ForDeprecatedReplaceableAttributeArguments_Fixes(string argument, string expectedFix)
        {
            string source = @"
using NUnit.Framework;
using NU = NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(" + argument + @")]
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
    [TestCase(" + expectedFix + @")]
    void TestMethod()
    {
    }
}
";

            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }
    }
}