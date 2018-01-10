using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Attributes
{
    public abstract class AttributeAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(ctx =>
            {
                if (!NUnitFramework.Symbols.TryGetNUnitSymbols(ctx.Compilation, out NUnitFramework.Symbols nunit))
                    return; 

                ctx.RegisterSyntaxNodeAction(syntaxNodeContext =>
                    CheckAttributeSymbolsAndAnalyze(syntaxNodeContext, nunit, ctx.Compilation), SyntaxKind.Attribute);
            });
        }

        internal abstract INamedTypeSymbol[] GetAnalyzedAttributeSymbols(NUnitFramework.Symbols nunit,
            Compilation compilation);

        protected virtual void Analyze(SyntaxNodeAnalysisContext context, AttributeSyntax attributeSyntax)
        {
            var attributeLocation = attributeSyntax.GetLocation();

            context.ReportDiagnostic(Diagnostic.Create(
                SupportedDiagnostics.First(), attributeLocation, attributeSyntax.Name.ToString()));
        }

        private void CheckAttributeSymbolsAndAnalyze(SyntaxNodeAnalysisContext context, NUnitFramework.Symbols nunit,
            Compilation compilation)
        {
            var attributeSyntax = (AttributeSyntax) context.Node;
            var semanticModel = context.SemanticModel;

            if (!IsNUnitAttributeSymbol(attributeSyntax, nunit, semanticModel, compilation))
                return;

            Analyze(context, attributeSyntax);
        }

        private bool IsNUnitAttributeSymbol(AttributeSyntax attributeSyntax,
            NUnitFramework.Symbols nunit, SemanticModel semanticModel, Compilation compilation)
        {
            var attributeSymbol = semanticModel.GetSymbolInfo(attributeSyntax).Symbol?.ContainingSymbol;
            var analyzedAttributeSymbols = GetAnalyzedAttributeSymbols(nunit, compilation);

            return analyzedAttributeSymbols.Any(analyzedAttr => analyzedAttr.Equals(attributeSymbol));
        }
    }
}