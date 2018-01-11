using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Attributes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TrivialAttributeArgumentReplaceAnalyzer : AttributeAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.DeprecatedReplaceableAttributeArgument);

        internal override INamedTypeSymbol[] GetAnalyzedAttributeSymbols(NUnitFramework.Symbols nunit,
            Compilation compilation)
        {
            return new[]
            {
                nunit.TestCase
            };
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax)
        {
            SyntaxHelper.ParseAttributeArguments(attributeSyntax, (argName, expression) =>
            {
                if (argName == null || !ReplacementTable.ContainsKey(argName))
                    return;

                var argLocation = expression.Parent.GetLocation();
                var argNameFix = ReplacementTable[argName];

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.DeprecatedReplaceableAttributeArgument, argLocation, argName, argNameFix));
            });
        }

        internal static readonly IReadOnlyDictionary<string, string> ReplacementTable = new Dictionary<string, string>
        {
            ["Result"] = "ExpectedResult"
        };
    }
}