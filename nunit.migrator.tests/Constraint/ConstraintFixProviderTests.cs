using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Constraint
{
    [TestFixture]
    public class ConstraintFixProviderTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ConstraintAnalyzer();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ConstraintFixProvider();

        private static ConstraintApiFixCase[] _textConstraintCases =
        {
            new ConstraintApiFixCase
            {
                OriginalExpression = "Text.All.TypeOf<char>()",
                FixedExpression = "Is.All.TypeOf<char>()"
            },
            new ConstraintApiFixCase
            {
                OriginalExpression = "Text.Contains(\"dummy\")",
                FixedExpression = "Does.Contain(\"dummy\")"
            },
            new ConstraintApiFixCase
            {
                OriginalExpression = "Text.DoesNotContain(\"dummy\")",
                FixedExpression = "Does.Not.Contain(\"dummy\")"
            },
            new ConstraintApiFixCase
            {
                OriginalExpression = "Text.StartsWith(\"dummy\")",
                FixedExpression = "Does.StartWith(\"dummy\")"
            },
            new ConstraintApiFixCase
            {
                OriginalExpression = "Text.DoesNotStartWith(\"dummy\")",
                FixedExpression = "Does.Not.StartWith(\"dummy\")"
            },
            new ConstraintApiFixCase
            {
                OriginalExpression = "Text.EndsWith(\"dummy\")",
                FixedExpression = "Does.EndWith(\"dummy\")"
            },
            new ConstraintApiFixCase
            {
                OriginalExpression = "Text.DoesNotEndWith(\"dummy\")",
                FixedExpression = "Does.Not.EndWith(\"dummy\")"
            },
            new ConstraintApiFixCase
            {
                OriginalExpression = "Text.Matches(\"dummy\")",
                FixedExpression = "Does.Match(\"dummy\")"
            },
            new ConstraintApiFixCase
            {
                OriginalExpression = "Text.DoesNotMatch(\"dummy\")",
                FixedExpression = "Does.Not.Match(\"dummy\")"
            },
        };

        [TestCaseSource(nameof(_textConstraintCases))]
        public void ForGivenTextMemberAccess_TransformsToV3Api(ConstraintApiFixCase constraintCase)
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.That(""actual"", " + constraintCase.OriginalExpression + @");
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
        Assert.That(""actual"", " + constraintCase.FixedExpression + @");
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }

        public struct ConstraintApiFixCase
        {
            public string OriginalExpression { get; set; }

            public string FixedExpression { get; set; }

            public override string ToString() => $"{OriginalExpression} => {FixedExpression}";
        }
    }
}