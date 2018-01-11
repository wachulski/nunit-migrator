using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Attributes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TrivialAttributeReplaceAnalyzer : AttributeAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.DeprecatedReplaceableAttribute);

        internal override INamedTypeSymbol[] GetAnalyzedAttributeSymbols(NUnitFramework.Symbols nunit,
            Compilation compilation)
        {
            return new[]
            {
                nunit.RequiresMTA,
                nunit.RequiresSTA,
                nunit.TestFixtureSetUp,
                nunit.TestFixtureTearDown
            };
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax)
        {
            var (refinedAttr, targetString) = GetRefinedAttributeWithItsMigrationTarget(attributeSyntax);

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.DeprecatedReplaceableAttribute, attributeSyntax.GetLocation(), refinedAttr, targetString));
        }

        internal static (string lookupKey, string replaceWith) GetRefinedAttributeWithItsMigrationTarget(
            AttributeSyntax attributeSyntax)
        {
            var attrName = attributeSyntax.Name.ToString();
            var nsFreeName = attrName.Substring(attrName.LastIndexOf('.') + 1);
            var refinedName = nsFreeName.Replace("Attribute", string.Empty);
            var replaceWith = ReplacementTable[refinedName];

            return (refinedName, replaceWith);
        }

        private static readonly IReadOnlyDictionary<string, string> ReplacementTable = new Dictionary<string, string>
        {
            ["RequiresMTA"] = "Apartment(System.Threading.ApartmentState.MTA)",
            ["RequiresSTA"] = "Apartment(System.Threading.ApartmentState.STA)",
            ["TestFixtureSetUp"] = "OneTimeSetUp",
            ["TestFixtureTearDown"] = "OneTimeTearDown",
        };
    }
}