using System.Linq;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    [TestFixture]
    public class ChangedSemanticFixProviderTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ChangedSemanticAnalyzer();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ChangedSemanticFixProvider();

        private static readonly string[][] ThreadRelatedAttributesAndTheirFixes = 
        {
            new [] {"System.MTAThread", "Apartment(System.Threading.ApartmentState.MTA)"},
            new [] {"System.STAThread", "Apartment(System.Threading.ApartmentState.STA)"}
        };

        private static readonly string[] ThreadRelatedAttributes =
            ThreadRelatedAttributesAndTheirFixes.Select(x => x[0]).ToArray();

        [TestCaseSource(nameof(ThreadRelatedAttributesAndTheirFixes))]
        public void GivenThreadRelatedAttributeAlongWithTestFixtureAttribute_Fixes(
            string threadAttribute, 
            string expectedAttribute)
        {
            string CreateSource(string attr) => @"
using NUnit.Framework;

[TestFixture, " + attr + @"]
public class TestClass
{
    [Test]
    void TestMethod() {}
}
";
            VerifyCSharpFix(CreateSource(threadAttribute), CreateSource(expectedAttribute), 
                allowNewCompilerDiagnostics: true);
        }

        [TestCaseSource(nameof(ThreadRelatedAttributesAndTheirFixes))]
        public void GivenThreadRelatedAttributeAlongWithTestAttribute_Fixes(
            string threadAttribute,
            string expectedAttribute)
        {
            string CreateSource(string attr) => @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, " + attr + @"]
    void TestMethod() {}
}
";
            VerifyCSharpFix(CreateSource(threadAttribute), CreateSource(expectedAttribute),
                allowNewCompilerDiagnostics: true);
        }

        [TestCaseSource(nameof(ThreadRelatedAttributesAndTheirFixes))]
        public void GivenThreadRelatedAttributeAlongWithTestCaseAttributes_Fixes(
            string threadAttribute,
            string expectedAttribute)
        {
            string CreateSource(string attr) => @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [TestCase(0)]
    [TestCase(1), " + attr + @"]
    void TestMethod() {}
}
";
            VerifyCSharpFix(CreateSource(threadAttribute), CreateSource(expectedAttribute),
                allowNewCompilerDiagnostics: true);
        }

        [TestCaseSource(nameof(ThreadRelatedAttributes))]
        public void GivenThreadRelatedAttributeButWithoutTestRelatedAttribute_DoesNotFix(string threadAttribute)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [" + threadAttribute + @"]
    void AuxMethod() {}
}
";
            VerifyCSharpFix(source, source);
        }
    }
}