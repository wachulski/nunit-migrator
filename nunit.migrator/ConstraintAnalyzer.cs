using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstraintAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.Constraint);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(ctx =>
            {
                if (!NUnitFramework.Symbols.TryGetNUnitSymbols(ctx.Compilation, out NUnitFramework.Symbols nunit))
                    return;

                ctx.RegisterSyntaxNodeAction(syntaxNodeContext =>
                    AnalyzeMemberAccess(syntaxNodeContext, nunit), SyntaxKind.SimpleMemberAccessExpression);
            });
        }

        private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context, NUnitFramework.Symbols nunit)
        {
            var memberAccess = (MemberAccessExpressionSyntax) context.Node;

            bool found = FindOldContraintApiWithProposedFix(context, nunit, memberAccess, out var fixedConstraint);

            if (!found)
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.Constraint,
                memberAccess.GetLocation(),
                $"{memberAccess}",
                $"{fixedConstraint}"));
        }

        private static bool FindOldContraintApiWithProposedFix(SyntaxNodeAnalysisContext context, 
            NUnitFramework.Symbols nunit, MemberAccessExpressionSyntax memberAccess, 
            out SyntaxNode v3ApiFixMemberAccessNode)
        {
            v3ApiFixMemberAccessNode = null;

            if (!IsV2ApiConstraintToBeMigrated(context, nunit, memberAccess))
                return false;

            v3ApiFixMemberAccessNode = ConstraintCodeAction.CreateV3ConstraintToFixWith(memberAccess);

            return true;
        }

        private static bool IsV2ApiConstraintToBeMigrated(SyntaxNodeAnalysisContext context, 
            NUnitFramework.Symbols nunit, MemberAccessExpressionSyntax memberAccess)
        {
            if (!ConstraintCodeAction.IsMemberAccessMigratableConstraint(memberAccess))
                return false;

            var containingClass = context.SemanticModel.GetSymbolInfo(memberAccess.Name).Symbol?.ContainingSymbol;
            if (containingClass == null || !(nunit.Text.Equals(containingClass) || nunit.Is.Equals(containingClass)))
                return false;

            return true;
        }
    }
}