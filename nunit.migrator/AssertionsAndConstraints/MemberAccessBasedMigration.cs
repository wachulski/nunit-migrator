using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.AssertionsAndConstraints
{
    internal abstract class MemberAccessBasedMigration<TMemberAccessContainerNode>
        : ICompilationStartAnalyzing, IFixer
        where TMemberAccessContainerNode : SyntaxNode
    {
        public abstract DiagnosticDescriptor DiagnosticDescriptor { get; }

        public abstract string CreateReplaceWithTargetString(TMemberAccessContainerNode fixedContainer);

        public abstract TMemberAccessContainerNode CreateFixedContainer(TMemberAccessContainerNode container);

        DiagnosticDescriptor ICompilationStartAnalyzing.SupportedDiagnostic => DiagnosticDescriptor;

        public void RegisterAnalysis(CompilationStartAnalysisContext context, NUnitFramework.Symbols nunit)
        {
            context.RegisterSyntaxNodeAction(syntaxNodeAnalysisContext =>
                Analyze(syntaxNodeAnalysisContext, nunit), ContainerSyntaxKind);
        }

        DiagnosticDescriptor IFixer.FixableDiagnostic => DiagnosticDescriptor;

        public void RegisterFixing(CodeFixContext context, Document document, SyntaxNode documentRoot)
        {
            var diagnostics = context.Diagnostics.Where(d => d.Id == DiagnosticDescriptor.Id).ToArray();
            if (!diagnostics.Any())
                return;

            var container = documentRoot.FindNode(context.Span).FirstAncestorOrSelf<TMemberAccessContainerNode>();
            var codeAction = new MemberAccessCodeAction<TMemberAccessContainerNode>(document, container, this);

            context.RegisterCodeFix(codeAction, diagnostics);
        }

        protected abstract SyntaxKind ContainerSyntaxKind { get; }

        protected abstract bool TryGetMemberAccess(TMemberAccessContainerNode container,
            out MemberAccessExpressionSyntax memberAccess);

        protected abstract INamedTypeSymbol[] GetMemberAccessContainingClassSymbolsEligibleForFix(
            NUnitFramework.Symbols nunit);

        private void Analyze(SyntaxNodeAnalysisContext context, NUnitFramework.Symbols nunit)
        {
            var containerNode = (TMemberAccessContainerNode)context.Node;

            if (!TryGetMemberAccess(containerNode, out MemberAccessExpressionSyntax memberAccess))
                return;

            if (!FindOldApiAndProposedFix(context, nunit, memberAccess))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptor,
                memberAccess.GetLocation(),
                $"{memberAccess}",
                $"{CreateReplaceWithTargetString(CreateFixedContainer(containerNode))}"));
        }

        private bool FindOldApiAndProposedFix(SyntaxNodeAnalysisContext context,
            NUnitFramework.Symbols nunit, MemberAccessExpressionSyntax memberAccess)
        {
            return MemberAccessMigrationTable.TryGetFixExpression(memberAccess, out ExpressionSyntax _) 
                && DoesMemberAccessSymbolMatchNUnit(context, nunit, memberAccess);
        }
        
        private bool DoesMemberAccessSymbolMatchNUnit(
            SyntaxNodeAnalysisContext context, NUnitFramework.Symbols nunit,
            MemberAccessExpressionSyntax memberAccess)
        {
            var containingClassSymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol?.ContainingSymbol;
            var allowedContainingClassSymbols = GetMemberAccessContainingClassSymbolsEligibleForFix(nunit);

            // since member access may be defined in client code too, we need to distinguish as we migrate nunit only
            if (containingClassSymbol == null
                || !allowedContainingClassSymbols.Any(classSymbol => classSymbol.Equals(containingClassSymbol)))
            {
                return false;
            }

            return true;
        }
    }
}