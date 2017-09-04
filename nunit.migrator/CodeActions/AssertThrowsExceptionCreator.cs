using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.CodeActions
{
    internal class AssertThrowsExceptionCreator : IAssertExceptionBlockCreator
    {
        public BlockSyntax Create(MethodDeclarationSyntax method, TypeSyntax assertedType)
        {
            return CreateFixedTestMethodBodyBlock(method, assertedType);
        }

        private static BlockSyntax CreateFixedTestMethodBodyBlock(BaseMethodDeclarationSyntax methodDeclarationSyntax,
            TypeSyntax assertedType)
        {
            var assertThrowsInvocation =
                CreateAssertThrowsInvocation(methodDeclarationSyntax, assertedType);

            return SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(assertThrowsInvocation));
        }

        private static InvocationExpressionSyntax CreateAssertThrowsInvocation(
            BaseMethodDeclarationSyntax methodDeclarationSyntax,
            TypeSyntax assertedType)
        {
            var arguments = new List<ArgumentSyntax>
            {
                SyntaxFactory.Argument(
                    SyntaxFactory.ParenthesizedLambdaExpression(
                        methodDeclarationSyntax.Body.WithoutTrailingTrivia()))
            };
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(NUnitFramework.AssertIdentifier),
                    SyntaxFactory.GenericName(
                        SyntaxFactory.ParseToken(NUnitFramework.Assert.ThrowsIdentifier),
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList(assertedType)))),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(arguments)));
        }
    }
}