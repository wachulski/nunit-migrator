using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.ExceptionExpectancy.CodeActions
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
            var lambda = SyntaxFactory.ParenthesizedLambdaExpression(methodDeclarationSyntax.Body.WithoutTrailingTrivia());

            return methodDeclarationSyntax.Modifiers.Any(SyntaxKind.AsyncKeyword)
                ? CreateInvocation(
                    NUnitFramework.Assert.ThrowsAsyncIdentifier,
                    lambda
                        .WithAsyncKeyword(SyntaxFactory.Token(SyntaxKind.AsyncKeyword)))
                : CreateInvocation(
                    NUnitFramework.Assert.ThrowsIdentifier,
                    lambda);

            InvocationExpressionSyntax CreateInvocation(string throwsIdentifier, ParenthesizedLambdaExpressionSyntax lambdaExpression)
            {
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(NUnitFramework.AssertIdentifier),
                        SyntaxFactory.GenericName(
                            SyntaxFactory.ParseToken(throwsIdentifier),
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(assertedType)))),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new List<ArgumentSyntax>
                        {
                            SyntaxFactory.Argument(lambdaExpression)
                        })));
            }
        }
    }
}