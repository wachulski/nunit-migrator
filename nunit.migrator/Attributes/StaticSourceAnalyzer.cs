using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Attributes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticSourceAnalyzer : AttributeAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.StaticSource);

        protected override void Analyze(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax)
        {
            var attribute = new StaticSourceAttribute(attributeSyntax, context.SemanticModel);
            if (string.IsNullOrEmpty(attribute.MemberName))
                return;
            var attributeLocation = attributeSyntax.GetLocation();

            if (!IsMemberStatic(attribute.ContainingSymbol, attribute.MemberName, attributeLocation, 
                context.SemanticModel))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.StaticSource, 
                        attributeLocation,
                        attribute.GetFixerParams(),
                        attribute.AttributeName, 
                        attribute.MemberFullPath));
            }
        }

        internal override INamedTypeSymbol[] GetAnalyzedAttributeSymbols(NUnitFramework.Symbols nunit,
            Compilation compilation)
        {
            return new[]
            {
                nunit.TestCaseSource, nunit.ValueSource
            };
        }

        private static bool IsMemberStatic(INamespaceOrTypeSymbol containingTypeSymbol, string memberName, 
            Location scope, SemanticModel semanticModel)
        {
            // for safety, code might not compile as the author is being typing, the analyzer shouldn't throw then
            if (containingTypeSymbol == null || string.IsNullOrWhiteSpace(memberName))
                return false;

            return semanticModel
                .LookupStaticMembers(scope.SourceSpan.Start, containingTypeSymbol, memberName)
                .Any();
        }
    }
}