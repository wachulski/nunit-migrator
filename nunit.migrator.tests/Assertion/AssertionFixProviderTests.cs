using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.AssertionsAndConstraints;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Assertion
{
    [TestFixture]
    public class AssertionFixProviderTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new MemberAccessBasedAnalyzer();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new MemberAccessBasedFixProvider();

        [TestCase("IsNullOrEmpty", "Is.Null.Or.Empty")]
        [TestCase("IsNotNullOrEmpty", "Is.Not.Null.And.Not.Empty")]
        public void ForGivenAssertionMethod_MigratesToAssertThatWithPredicate(string oldMethod, string predicateFix)
        {
            Test(oldMethod, predicateFix, "");
        }

        [TestCase(", \"myMessage\"")]
        [TestCase(", \"msgFormat\", \"param1\"")]
        [TestCase(", \"msgFormat\", \"param1\", \"param2\"")]
        public void ForGivenMessageParameters_MigratesToAssertThatWithPredicatePreservingMessageParams(
            string restOfMessageParams)
        {
            Test("IsNullOrEmpty", "Is.Null.Or.Empty", restOfMessageParams);
        }

        private void Test(string oldMethod, string predicateFix, string restOfMessageParams)
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert." + oldMethod + @"(""actual""" + restOfMessageParams + @");
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.That(""actual"", " + predicateFix + restOfMessageParams + @");
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }
    }
}