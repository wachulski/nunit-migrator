using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Constraint
{
    [TestFixture]
    public class ConstraintAnalyzerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ConstraintAnalyzer();

        private static ConstraintApiBreakingCase[] _textConstraintCases =
        {
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Text.All.TypeOf<char>()",
                ExpectedDiagnosticMessage = "'Text.All' constraint should be replaced with 'Is.All'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Text.Contains(\"act\")",
                ExpectedDiagnosticMessage = "'Text.Contains' constraint should be replaced with 'Does.Contain'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Text.DoesNotContain(\"sub\")",
                ExpectedDiagnosticMessage =
                    "'Text.DoesNotContain' constraint should be replaced with 'Does.Not.Contain'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Text.StartsWith(\"act\")",
                ExpectedDiagnosticMessage =
                    "'Text.StartsWith' constraint should be replaced with 'Does.StartWith'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Text.DoesNotStartWith(\"act\")",
                ExpectedDiagnosticMessage =
                    "'Text.DoesNotStartWith' constraint should be replaced with 'Does.Not.StartWith'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Text.EndsWith(\"al\")",
                ExpectedDiagnosticMessage =
                    "'Text.EndsWith' constraint should be replaced with 'Does.EndWith'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Text.DoesNotEndWith(\"sub\")",
                ExpectedDiagnosticMessage =
                    "'Text.DoesNotEndWith' constraint should be replaced with 'Does.Not.EndWith'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Text.Matches(\"act.*l\")",
                ExpectedDiagnosticMessage =
                    "'Text.Matches' constraint should be replaced with 'Does.Match'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Text.DoesNotMatch(\"a.*1.*\")",
                ExpectedDiagnosticMessage =
                    "'Text.DoesNotMatch' constraint should be replaced with 'Does.Not.Match'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Is.StringStarting(\"str\")",
                ExpectedDiagnosticMessage = "'Is.StringStarting' constraint should be replaced with 'Does.StartWith'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Is.StringEnding(\"str\")",
                ExpectedDiagnosticMessage = "'Is.StringEnding' constraint should be replaced with 'Does.EndWith'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Is.StringContaining(\"str\")",
                ExpectedDiagnosticMessage = "'Is.StringContaining' constraint should be replaced with 'Does.Contain'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Is.StringMatching(\"str\")",
                ExpectedDiagnosticMessage = "'Is.StringMatching' constraint should be replaced with 'Does.Match'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Is.InstanceOfType(typeof(string))",
                ExpectedDiagnosticMessage = "'Is.InstanceOfType' constraint should be replaced with 'Is.InstanceOf'."
            },
            new ConstraintApiBreakingCase
            {
                OriginalExpression = "Is.InstanceOfType<string>()",
                ExpectedDiagnosticMessage = 
                    "'Is.InstanceOfType<string>' constraint should be replaced with 'Is.InstanceOf<string>'."
            },
        };

        [TestCaseSource(nameof(_textConstraintCases))]
        public void ForGivenTextMemberAccess_Reports(ConstraintApiBreakingCase breakingCase)
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.That(""actual"", " + breakingCase.OriginalExpression + @");
    }
}";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NUnit2Migra002",
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 31) },
                Message = breakingCase.ExpectedDiagnosticMessage,
                Severity = DiagnosticSeverity.Error
            });
        }

        [Test]
        public void ForIsAllWhichHasSameMethodNameAsTextAllButNotMigratable_DoesNotReport()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.That(""actual"", Is.All.TypeOf<char>());
    }
}";
            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void ForMemberAccessNotFromNUnitTextClass_DoesNotReport()
        {
            var source = @"
using NUnit.Framework;

public static class Text { public bool Contains(string text) { return true; } }

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Text.Contains(""str"");
    }
}";
            VerifyNoDiagnosticReported(source);
        }

        public struct ConstraintApiBreakingCase
        {
            public string OriginalExpression { get; set; }

            public string ExpectedDiagnosticMessage { get; set; }

            public override string ToString() => ExpectedDiagnosticMessage;
        }
    }
}