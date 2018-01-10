using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Attributes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ChangedSemanticAnalyzer : AttributeAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.AttributeChangedSemantic);

        internal override INamedTypeSymbol[] GetAnalyzedAttributeSymbols(NUnitFramework.Symbols nunit,
            Compilation compilation)
        {
            var mtaThreadAttribute = compilation.GetTypeByMetadataName("System.MTAThreadAttribute");
            var staThreadAttribute = compilation.GetTypeByMetadataName("System.STAThreadAttribute");

            return mtaThreadAttribute == null || staThreadAttribute == null
                ? new INamedTypeSymbol[] { }
                : new[] { mtaThreadAttribute, staThreadAttribute };
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax)
        {
            var attributeLocation = attributeSyntax.GetLocation();

            (var attrName, var nunitTreatedAsAttrName) = ResolveNames(attributeSyntax);

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.AttributeChangedSemantic, attributeLocation, attrName, nunitTreatedAsAttrName));
        }

        (string attrName, string nunitTreatedAsAttrName) ResolveNames(AttributeSyntax attribute)
        {
            var attrName = attribute.Name.ToString();

            if (attrName.Contains("MTAThread"))
                return (attrName, "NUnit.Framework.RequiresMTAAttribute");

            if (attrName.Contains("STAThread"))
                return (attrName, "NUnit.Framework.RequiresSTAAttribute");

            return (attrName, string.Empty);
        }
    }
}