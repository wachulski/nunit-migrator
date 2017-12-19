using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.AssertionsAndConstraints
{
    internal class AssertionMigration : MemberAccessBasedMigration<InvocationExpressionSyntax>
    {
        public override DiagnosticDescriptor DiagnosticDescriptor => Descriptors.Assertion;

        protected override SyntaxKind ContainerSyntaxKind => SyntaxKind.InvocationExpression;

        protected override bool TryGetMemberAccess(InvocationExpressionSyntax container, 
            out MemberAccessExpressionSyntax memberAccess)
        {
            memberAccess = container.Expression as MemberAccessExpressionSyntax;

            return memberAccess != null;
        }

        public override string CreateReplaceWithTargetString(InvocationExpressionSyntax fixedContainer)
        {
            return $"Assert.That(..., {(SyntaxNode) fixedContainer.ArgumentList.Arguments.Last()})";
        }

        public override InvocationExpressionSyntax CreateFixedContainer(InvocationExpressionSyntax container)
        {
            if (!TryGetMemberAccess(container, out var memberAccess))
                return container;

            if (!MemberAccessMigrationTable.TryGetFixExpression(memberAccess, out ExpressionSyntax fixExpression))
                return container;

            var fixedInvocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.ParseExpression("Assert.That"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        container.ArgumentList.Arguments.Insert(1, SyntaxFactory.Argument(fixExpression)))));

            return fixedInvocation;

        }

        protected override INamedTypeSymbol[] GetMemberAccessContainingClassSymbolsEligibleForFix(
            NUnitFramework.Symbols nunit)
        {
            return new[]
            {
                nunit.Assert
            };
        }
    }
}