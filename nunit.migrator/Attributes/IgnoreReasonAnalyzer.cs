using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Attributes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IgnoreReasonAnalyzer : AttributeAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            new[] {Descriptors.IgnoreReason});

        internal override INamedTypeSymbol[] GetAnalyzedAttributeSymbols(NUnitFramework.Symbols nunit, Compilation compilation)
        {
            return new[] {nunit.TestCase, nunit.Ignore, nunit.TestFixture};
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax)
        {
            var ignoringModel = new TestIgnoringModel(attributeSyntax);

            if (!ignoringModel.DoesIgnoringNeedAdjustment)
                return;

            var attributeLocation = attributeSyntax.GetLocation();

            var reportMessage = ignoringModel.ReportMessage;
            
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.IgnoreReason, attributeLocation, reportMessage));
        }
    }
}