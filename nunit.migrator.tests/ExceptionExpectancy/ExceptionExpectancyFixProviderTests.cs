using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Tests.Helpers;

namespace NUnit.Migrator.Tests.ExceptionExpectancy
{
    public class ExceptionExpectancyFixProviderTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ExceptionExpectancyAnalyzer();
        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ExceptionExpectancyFixProvider();
    }
}