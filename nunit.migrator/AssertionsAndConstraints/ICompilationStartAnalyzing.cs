using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.AssertionsAndConstraints
{
    internal interface ICompilationStartAnalyzing
    {
        DiagnosticDescriptor SupportedDiagnostic { get; }

        void RegisterAnalysis(CompilationStartAnalysisContext context, NUnitFramework.Symbols nunit);
    }
}