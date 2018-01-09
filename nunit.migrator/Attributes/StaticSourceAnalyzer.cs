using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Attributes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticSourceAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.StaticSource);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(ctx =>
            {
                if (!NUnitFramework.Symbols.TryGetNUnitSymbols(ctx.Compilation, out NUnitFramework.Symbols nunit))
                    return; 

                ctx.RegisterSyntaxNodeAction(syntaxNodeContext =>
                    AnalyzeAttribute(syntaxNodeContext, nunit), SyntaxKind.Attribute);
            });
        }

        private static bool IsMemberStatic(INamespaceOrTypeSymbol containerTypeSymbol, string memberName, 
            Location scope, SemanticModel semanticModel)
        {
            return semanticModel
                .LookupStaticMembers(scope.SourceSpan.Start, containerTypeSymbol, memberName)
                .Any();
        }

        private void AnalyzeAttribute(SyntaxNodeAnalysisContext context, NUnitFramework.Symbols nunit)
        {
            var attributeSyntax = (AttributeSyntax) context.Node;
            var semanticModel = context.SemanticModel;

            if (!IsNUnitAttributeSymbol(attributeSyntax, nunit, semanticModel))
                return;

            var model = new StaticSourceAttribute(attributeSyntax);

            var containerTypeSymbol = model.GetContainerTypeSymbol(semanticModel);
            if (containerTypeSymbol == null)
                return;

            var attributeLocation = attributeSyntax.GetLocation();

            if (!IsMemberStatic(containerTypeSymbol, model.SourceName, attributeLocation, semanticModel))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.StaticSource, attributeLocation, model.AttributeName, model.MemberFullPath));
            }
        }

        private bool IsNUnitAttributeSymbol(AttributeSyntax attributeSyntax, 
            NUnitFramework.Symbols nunit, SemanticModel semanticModel)
        {
            var attributeSymbol = semanticModel.GetSymbolInfo(attributeSyntax).Symbol?.ContainingSymbol;
            var analyzedAttributeSymbols = GetAnalyzedAttributeSymbols(nunit);

            return analyzedAttributeSymbols.Any(analyzedAttr => analyzedAttr.Equals(attributeSymbol));
        }

        private INamedTypeSymbol[] GetAnalyzedAttributeSymbols(NUnitFramework.Symbols nunit)
        {
            return new[]
            {
                nunit.TestCaseSource, nunit.ValueSource
            };
        }
    }
}