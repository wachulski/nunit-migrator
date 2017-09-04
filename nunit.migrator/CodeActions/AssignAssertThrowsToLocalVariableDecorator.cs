using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.CodeActions
{
    internal class AssignAssertThrowsToLocalVariableDecorator : AssertExceptionBlockDecorator
    {
        internal const string LocalExceptionVariableName = "ex";

        public AssignAssertThrowsToLocalVariableDecorator(IAssertExceptionBlockCreator blockCreator)
            : base(blockCreator)
        {
        }

        public override BlockSyntax Create(MethodDeclarationSyntax method, TypeSyntax assertedType)
        {
            var body = base.Create(method, assertedType);

            if (!TryFindExpressionToAssign(body, out ExpressionSyntax expressionToAssign))
                return body;

            var localVariableDeclarationStatement = CreateLocalVariableDeclarationWithAssignmentStatement(
                expressionToAssign);

            return body.ReplaceNode(expressionToAssign.Parent, localVariableDeclarationStatement);
        }

        private static bool TryFindExpressionToAssign(BlockSyntax block, out ExpressionSyntax expressionToAssign)
        {
            expressionToAssign = null;

            if (block.Statements.Count == 0)
                return false;

            expressionToAssign = (block.Statements.First() as ExpressionStatementSyntax)?.Expression;
            return expressionToAssign != null;
        }

        private static LocalDeclarationStatementSyntax CreateLocalVariableDeclarationWithAssignmentStatement(
            ExpressionSyntax expressionToAssign)
        {
            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("var"),
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.ParseToken(LocalExceptionVariableName),
                            null,
                            SyntaxFactory.EqualsValueClause(expressionToAssign))
                    })));
        }
    }
}