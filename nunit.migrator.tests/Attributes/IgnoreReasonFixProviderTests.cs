using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    public class IgnoreReasonFixProviderTests : CodeFixVerifier
    {
        protected override CodeFixProvider GetCSharpCodeFixProvider() => new IgnoreReasonFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new IgnoreReasonAnalyzer();

        [TestCase("Ignore", "Ignore(\"TODO: provide a reason\")")]
        [TestCase("Ignore()", "Ignore(\"TODO: provide a reason\")")]
        [TestCase("NUnit.Framework.Ignore", "NUnit.Framework.Ignore(\"TODO: provide a reason\")")]
        [TestCase("NUnit.Framework.Ignore()", "NUnit.Framework.Ignore(\"TODO: provide a reason\")")]
        public void GivenIgnoreAttributeLackingReasonLiteral_Fixes(string attribute, string expectedFix)
        {
            string CreateSource(string attr) => @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, " + attr + @"]
    void TestMethod()
    {
    }
}
";
            VerifyCSharpFix(CreateSource(attribute), CreateSource(expectedFix));
        }

        [TestCaseSource(nameof(TestCaseOrFixtureArgumentsCases))]
        public void GivenTestCaseWithIgnoringIndicationAndNoReasonSpecified_Fixes((string args, string fix) caseData)
        {
            string CreateSource(string tcArgs) => @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(" + tcArgs + @")]
    void TestMethod()
    {
    }
}
";
            VerifyCSharpFix(CreateSource(caseData.args), CreateSource(caseData.fix), allowNewCompilerDiagnostics: true);
        }

        [TestCaseSource(nameof(TestCaseOrFixtureArgumentsCases))]
        public void GivenTestFixtureWithIgnoringIndicationAndNoReasonSpecified_Fixes((string args, string fix) caseData)
        {
            string CreateSource(string tfArgs) => @"
using NUnit.Framework;

[TestFixture(" + tfArgs + @")]
public class TestClass
{
    [TestCase]
    void TestMethod()
    {
    }
}
";
            VerifyCSharpFix(CreateSource(caseData.args), CreateSource(caseData.fix), allowNewCompilerDiagnostics: true);
        }

        [Test]
        public void GivenIgnoreArgumentAlreadyAssignedReason_LeavesCodeAsIs()
        {
            const string source = @"
using NUnit.Framework;

[TestFixture(Category = ""cat"", Ignore = ""net failed"")]
public class TestClass
{
    [TestCase(Category = ""tcs"", Ignore = ""net failed"")]
    public TestMethod1() {}
}
";

            VerifyCSharpFix(source, source);
        }

        [Test]
        public void GivenMultipleIgnoreThatRequireReasonAssignment_Fixes()
        {
            const string source = @"
using NUnit.Framework;

[TestFixture(Category = ""cat"", Ignore = true, IgnoreReason = ""net failed"")]
public class TestClass
{
    [Ignore(""my reason""]
    [Test]
    public TestMethod1() {}
    
    [Ignore]
    [TestCase(Ignore = true)]
    public TestMethod2() {}
    
    [TestCase(Ignored = true, Description = ""not ok"", IgnoreReason = ""some reason"")]
    [TestCase(Description = ""this is ok"")]
    public TestMethod3() {}
}
";

            const string expected = @"
using NUnit.Framework;

[TestFixture(Category = ""cat"", Ignore = ""net failed"")]
public class TestClass
{
    [Ignore(""my reason""]
    [Test]
    public TestMethod1() {}
    
    [Ignore(""TODO: provide a reason"")]
    [TestCase(Ignore = ""TODO: provide a reason"")]
    public TestMethod2() {}
    
    [TestCase(Description = ""not ok"", Ignore = ""some reason"")]
    [TestCase(Description = ""this is ok"")]
    public TestMethod3() {}
}
";

            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }

        private static readonly (string args, string fix)[] TestCaseOrFixtureArgumentsCases =
        {
            ("Ignore = true", "Ignore = \"TODO: provide a reason\""),
            ("Ignored = true", "Ignore = \"TODO: provide a reason\""),
            ("Ignore = false", "Ignore = \"TODO: provide a reason\""),
            ("Ignored = false", "Ignore = \"TODO: provide a reason\""),
            ("Ignore = ShouldIgnore", "Ignore = \"TODO: provide a reason\""),
            ("Ignored = ShouldIgnore", "Ignore = \"TODO: provide a reason\""),
            ("Ignore = true, Ignored = true", "Ignore = \"TODO: provide a reason\""),
            ("Ignore = true, Ignored = false", "Ignore = \"TODO: provide a reason\""),
            ("Ignore = false, Ignored = true", "Ignore = \"TODO: provide a reason\""),
            ("Ignore = false, Ignored = false", "Ignore = \"TODO: provide a reason\""),
            ("Ignored = true, Ignore = true", "Ignore = \"TODO: provide a reason\""),
            ("Ignored = false, Ignore = true", "Ignore = \"TODO: provide a reason\""),
            ("Ignore = true, IgnoreReason = \"some reason\"", "Ignore = \"some reason\""),
            ("Ignored = true, IgnoreReason = \"some reason\"", "Ignore = \"some reason\""),
            ("Ignore = true, IgnoreReason = \"some reason\", Ignored = true", "Ignore = \"some reason\""),
            ("IgnoreReason = \"some reason\"", "Ignore = \"some reason\""),
            ("1, Ignore = true", "1, Ignore = \"TODO: provide a reason\""),
            ("1, Ignore = true, Description = \"desc\", IgnoreReason = \"reason\"", "1, Description = \"desc\", Ignore = \"reason\""),
        };
    }
}