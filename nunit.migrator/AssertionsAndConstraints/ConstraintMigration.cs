using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.AssertionsAndConstraints
{
    internal class ConstraintMigration : MemberAccessBasedMigration<MemberAccessExpressionSyntax>
    {
        public override DiagnosticDescriptor DiagnosticDescriptor => Descriptors.Constraint;

        protected override SyntaxKind ContainerSyntaxKind => SyntaxKind.SimpleMemberAccessExpression;

        protected override bool TryGetMemberAccess(MemberAccessExpressionSyntax container,
            out MemberAccessExpressionSyntax memberAccess)
        {
            memberAccess = container;
            return true;
        }

        public override string CreateReplaceWithTargetString(MemberAccessExpressionSyntax fixedContainer)
        {
            return fixedContainer.ToString();
        }

        public override MemberAccessExpressionSyntax CreateFixedContainer(MemberAccessExpressionSyntax container)
        {
            if (!TryGetMemberAccess(container, out MemberAccessExpressionSyntax memberAccess))
                return container;

            if (!MemberAccessMigrationTable.TryGetFixExpression(memberAccess, out ExpressionSyntax fixExpression))
                return container;

            if (memberAccess.Name is GenericNameSyntax genericName)
                fixExpression = SyntaxFactory.ParseExpression($"{fixExpression}{genericName.TypeArgumentList}");

            return fixExpression as MemberAccessExpressionSyntax ?? container;
        }

        protected override INamedTypeSymbol[] GetMemberAccessContainingClassSymbolsEligibleForFix(
            NUnitFramework.Symbols nunit)
        {
            return new[]
            {
                nunit.Text, nunit.Is
            };
        }
    }
}