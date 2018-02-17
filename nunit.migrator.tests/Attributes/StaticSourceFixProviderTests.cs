using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using NUnit.Migrator.Attributes;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.Attributes
{
    [TestFixture]
    public class StaticSourceFixProviderTests : CodeFixVerifier
    {
        protected override CodeFixProvider GetCSharpCodeFixProvider() => new StaticSourceFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new StaticSourceAnalyzer();

        [Test]
        public void ForNonStaticMemberReferencedInSameClass_Fixes()
        {
            const string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    private object[] Data { get; } = { 1, 2, 3 };

    [Test]
    [TestCaseSource(nameof(Data))]
    public void TestMethod(int x) {}
}";
            const string expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    private static object[] Data { get; } = { 1, 2, 3 };

    [Test]
    [TestCaseSource(nameof(Data))]
    public void TestMethod(int x) {}
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void ForNonStaticMemberReferencedInInnerClass_Fixes()
        {
            const string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    [TestCaseSource(typeof(InnerClass), nameof(data))]
    public void TestMethod(int x) {}

    internal class InnerClass
    {
        public readonly static object[] data = { 1, 2, 3 };
    }
}";
            const string expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    [TestCaseSource(typeof(InnerClass), nameof(data))]
    public void TestMethod(int x) {}

    internal class InnerClass
    {
        public readonly static object[] data = { 1, 2, 3 };
    }
}";
            VerifyCSharpFix(source, expected);

        }

        [Test]
        public void ForNonStaticMemberReferencedInOtherNamespaceClass_Fixes()
        {
            const string source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    [TestCaseSource(typeof(Outer.OuterClass), ""GetData"")]
    public void TestMethod(int x) {}
}

namespace Outer
{
    public class OuterClass
    {
        public object[] GetData() => { 1, 2, 3 };
    }
}";
            const string expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    [TestCaseSource(typeof(Outer.OuterClass), ""GetData"")]
    public void TestMethod(int x) {}
}

namespace Outer
{
    public class OuterClass
    {
        public static object[] GetData() => { 1, 2, 3 };
    }
}";
            VerifyCSharpFix(source, expected);
        }
    }
}
