using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Attributes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NoLongerSupportedAttributesAnalyzer : AttributeAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.NoLongerSupportedAttribute);

        internal override INamedTypeSymbol[] GetAnalyzedAttributeSymbols(NUnitFramework.Symbols nunit,
            Compilation compilation)
        {
            return new[]
            {
                nunit.Suite, nunit.RequiredAddin
            };
        }
    }
}