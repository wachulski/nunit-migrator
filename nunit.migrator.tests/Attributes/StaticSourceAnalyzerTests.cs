using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    public class StaticSourceAnalyzerTests : DiagnosticVerifier
    {
        public enum SourceKind
        {
            TestCaseSource = 1,
            ValueSource = 2
        }

        private static readonly SourceKind[] SourceKinds = {SourceKind.TestCaseSource, SourceKind.ValueSource};

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new StaticSourceAnalyzer();

        [Test]
        public void ForTypeReferenced_DoesNotReport()
        {
            string source = @"
using NUnit.Framework;

class TcSource : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return new object[] { 1 }
        yield return new object[] { 2 }
    }
}

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(SourceKind.TestCaseSource, "typeof(TcSource)") + @"
}
";
            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void ForStaticMemberReferencedInSameClass_DoesNotReport(
            [ValueSource(nameof(SourceKinds))] SourceKind sourceKind)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(sourceKind, "\"Cases\"") + @"

    static object[] Cases = { new object[] {1}, new object[] {2} }
}
";
            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void ForStaticMemberReferencedInOtherClass_DoesNotReport(
            [ValueSource(nameof(SourceKinds))] SourceKind sourceKind)
        {
            string source = @"
using NUnit.Framework;

class OuterSource
{
    public static object[] Cases = { new object[] {1}, new object[] {2} }
}

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(sourceKind, "typeof(OuterSource), \"Cases\"") + @"
}
";
            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void ForStaticMemberReferencedInOtherNamespaceClass_DoesNotReport(
            [ValueSource(nameof(SourceKinds))] SourceKind sourceKind)
        {
            string source = @"
using NUnit.Framework;

namespace Outer {
    class OuterSource
    {
        public static object[] Cases = { new object[] {1}, new object[] {2} }
    }
}

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(sourceKind, "typeof(Outer.OuterSource), \"Cases\"") + @"
}
";
            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void ForStaticMemberReferencedInInnerClass_DoesNotReport(
            [ValueSource(nameof(SourceKinds))] SourceKind sourceKind)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(sourceKind, "typeof(TestClass.InnerSource), \"Cases\"") + @"

    class InnerSource
    {
        internal static object[] Cases = { new object[] {1}, new object[] {2} }
    }
}
";
            VerifyNoDiagnosticReported(source);
        }

        [Test]
        public void ForNonStaticMemberReferencedInSameClass_Reports(
            [ValueSource(nameof(SourceKinds))] SourceKind sourceKind)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(sourceKind, "\"Cases\"") + @"

    object[] Cases = { new object[] {1}, new object[] {2} }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M04",
                Locations = new[] { GetExpectedDiagnosticLocation(sourceKind) },
                Message = GetExpectedAttributeName(sourceKind) + 
                          " attribute refers to 'TestClass.Cases' which is not static.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [Test]
        public void ForNonStaticMemberReferencedInSameClassByNameOf_Reports(
            [ValueSource(nameof(SourceKinds))] SourceKind sourceKind)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(sourceKind, "nameof(Cases)") + @"

    object[] Cases = { new object[] {1}, new object[] {2} }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M04",
                Locations = new[] { GetExpectedDiagnosticLocation(sourceKind) },
                Message = GetExpectedAttributeName(sourceKind) +
                          " attribute refers to 'TestClass.Cases' which is not static.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [Test]
        public void ForNonStaticMemberReferencedInOtherClass_Reports(
            [ValueSource(nameof(SourceKinds))] SourceKind sourceKind)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(sourceKind, "typeof(OuterSource), \"Cases\"") + @"
}

class OuterSource
{
    object[] Cases = { new object[] {1}, new object[] {2} }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M04",
                Locations = new[] { GetExpectedDiagnosticLocation(sourceKind) },
                Message = GetExpectedAttributeName(sourceKind) + 
                          " attribute refers to 'OuterSource.Cases' which is not static.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [Test]
        public void ForNonStaticMemberReferencedInOtherClassByNameOf_Reports(
            [ValueSource(nameof(SourceKinds))] SourceKind sourceKind)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(sourceKind, "typeof(OuterSource), nameof(OuterScope.Cases)") + @"
}

class OuterSource
{
    object[] Cases = { new object[] {1}, new object[] {2} }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M04",
                Locations = new[] { GetExpectedDiagnosticLocation(sourceKind) },
                Message = GetExpectedAttributeName(sourceKind) +
                          " attribute refers to 'OuterSource.Cases' which is not static.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [Test]
        public void ForNonStaticMemberReferencedInOtherNamespaceClass_Reports(
            [ValueSource(nameof(SourceKinds))] SourceKind sourceKind)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(sourceKind, "typeof(Outer.OuterSource), \"Cases\"") + @"
}

namespace Outer {
    class OuterSource
    {
        object[] Cases = { new object[] {1}, new object[] {2} }
    }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M04",
                Locations = new[] { GetExpectedDiagnosticLocation(sourceKind) },
                Message = GetExpectedAttributeName(sourceKind) + 
                          " attribute refers to 'Outer.OuterSource.Cases' which is not static.",
                Severity = DiagnosticSeverity.Error
            });
        }

        [Test]
        public void ForNonStaticMemberReferencedInInnerClass_Reports(
            [ValueSource(nameof(SourceKinds))] SourceKind sourceKind)
        {
            string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    " + CreateTestMethodString(sourceKind, "typeof(TestClass.InnerSource), \"Cases\"") + @"

    class InnerSource
    {
        object[] Cases = { new object[] {1}, new object[] {2} }
    }
}
";
            VerifyCSharpDiagnostic(source, new DiagnosticResult
            {
                Id = "NU2M04",
                Locations = new[] { GetExpectedDiagnosticLocation(sourceKind) },
                Message = GetExpectedAttributeName(sourceKind) + 
                          " attribute refers to 'TestClass.InnerSource.Cases' which is not static.",
                Severity = DiagnosticSeverity.Error
            });
        }

        private static string CreateTestMethodString(SourceKind sourceKind, string attributeParams)
        {
            return sourceKind == SourceKind.TestCaseSource
                ? "[TestCaseSource(" + attributeParams + ")] public void TestMethod(int x) {}"
                : "public void TestMethod([ValueSource(" + attributeParams + ")]int x) {}";
        }

        private static string GetExpectedAttributeName(SourceKind sourceKind) => sourceKind.ToString();

        private static DiagnosticResultLocation GetExpectedDiagnosticLocation(SourceKind sourceKind) =>
            sourceKind == SourceKind.TestCaseSource
                ? new DiagnosticResultLocation("Test0.cs", 7, 6)
                : new DiagnosticResultLocation("Test0.cs", 7, 29);
    }
}