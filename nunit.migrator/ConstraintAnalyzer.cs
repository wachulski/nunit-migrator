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

            var found = FindOldContraintApiWithProposedFix(context, nunit, memberAccess, out var v2ApiTextMember, 
                out SyntaxNode v3FixedMemberAccess);

            if (!found)
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.Constraint,
                memberAccess.GetLocation(),
                $"Text.{v2ApiTextMember}",
                $"{v3FixedMemberAccess}"));
        }

        private static bool FindOldContraintApiWithProposedFix(SyntaxNodeAnalysisContext context, 
            NUnitFramework.Symbols nunit, MemberAccessExpressionSyntax memberAccess, out string v2ApiTextMember,
            out SyntaxNode v3ApiFixMemberAccess)
        {
            v2ApiTextMember = null;
            v3ApiFixMemberAccess = null;

            if (!MemberAccessFixingMap.ContainsKey(memberAccess.Name.Identifier.Text))
                return false;

            var textMethodSymbol = context.SemanticModel.GetSymbolInfo(memberAccess.Name).Symbol;
            if (textMethodSymbol == null || !nunit.Text.Equals(textMethodSymbol.ContainingSymbol))
                return false;

            v2ApiTextMember = textMethodSymbol.Name;

            return MemberAccessFixingMap.TryGetValue(v2ApiTextMember, out v3ApiFixMemberAccess);
        }

        internal static readonly IImmutableDictionary<string, SyntaxNode> MemberAccessFixingMap =
            new Dictionary<string, SyntaxNode>
            {
                ["All"] = SyntaxFactory.ParseExpression("Is.All"),
                ["Contains"] = SyntaxFactory.ParseExpression("Does.Contain"),
                ["DoesNotContain"] = SyntaxFactory.ParseExpression("Does.Not.Contain"),
                ["StartsWith"] = SyntaxFactory.ParseExpression("Does.StartWith"),
                ["DoesNotStartWith"] = SyntaxFactory.ParseExpression("Does.Not.StartWith"),
                ["EndsWith"] = SyntaxFactory.ParseExpression("Does.EndWith"),
                ["DoesNotEndWith"] = SyntaxFactory.ParseExpression("Does.Not.EndWith"),
                ["Matches"] = SyntaxFactory.ParseExpression("Does.Match"),
                ["DoesNotMatch"] = SyntaxFactory.ParseExpression("Does.Not.Match"),
            }.ToImmutableDictionary();
    }
}