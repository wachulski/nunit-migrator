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
            var model = new StaticSourceAttribute(attributeSyntax);

            var containerTypeSymbol = model.GetContainerTypeSymbol(context.SemanticModel);
            if (containerTypeSymbol == null)
                return;

            var attributeLocation = attributeSyntax.GetLocation();

            if (!IsMemberStatic(containerTypeSymbol, model.SourceName, attributeLocation, context.SemanticModel))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.StaticSource, attributeLocation, model.AttributeName, model.MemberFullPath));
            }
        }

        internal override INamedTypeSymbol[] GetAnalyzedAttributeSymbols(NUnitFramework.Symbols nunit)
        {
            return new[]
            {
                nunit.TestCaseSource, nunit.ValueSource
            };
        }

        private static bool IsMemberStatic(INamespaceOrTypeSymbol containerTypeSymbol, string memberName, 
            Location scope, SemanticModel semanticModel)
        {
            return semanticModel
                .LookupStaticMembers(scope.SourceSpan.Start, containerTypeSymbol, memberName)
                .Any();
        }
    }
}