using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using TestHelper;

namespace NUnit.Migrator.Tests
{
    [TestFixture]
    public class ExpectedExceptionAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ExpectedExceptionAnalyzer();

        [TestCase("")]
        [TestCase("(typeof(System.InvalidOperationException))")]
        [TestCase("(ExpectedException = typeof(System.InvalidOperationException))")]
        [TestCase("(\"System.InvalidOperationException\")")]
        [TestCase("(ExpectedExceptionName = \"System.InvalidOperationException\")")]
        public void ForExpectedExceptionAttributeSpecified_FindsMigrationPossible(string attrArguments)
        {
            string source = @"
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
                Id = Descriptors.ExpectedException.Id,
                Locations = new[] {new DiagnosticResultLocation("Test0.cs", 7, 12)},
                Message =
                    "'ExpectedException' attribute is not supported in NUnit v3. Consider replacing with Assert.Throws<T>.",
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
            VerifyCSharpDiagnostic(source);
        }
    }
}