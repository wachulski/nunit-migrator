using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.ExceptionExpectancy.Model;

namespace NUnit.Migrator.ExceptionExpectancy.CodeActions
{
    internal class AssertUserMessageDecorator : AssertExceptionBlockDecorator
    {
        private readonly string _userMessage;

        public AssertUserMessageDecorator(IAssertExceptionBlockCreator blockCreator,
            ExceptionExpectancyAtAttributeLevel attribute) : base(blockCreator)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            _userMessage = attribute.UserMessage;
        }

        public override BlockSyntax Create(MethodDeclarationSyntax method, TypeSyntax assertedType)
        {
            var body = base.Create(method, assertedType);

            if (!TryFindAssertThrowsInvocation(body, out InvocationExpressionSyntax invocation))
                return body;

            var userMessageArgument = SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                SyntaxFactory.ParseToken(_userMessage)));

            var decoratedInvocationArgumentList = invocation.ArgumentList.AddArguments(userMessageArgument);

            return body.ReplaceNode(invocation.ArgumentList, decoratedInvocationArgumentList);
        }

        private static bool TryFindAssertThrowsInvocation(BlockSyntax block, out InvocationExpressionSyntax invocation)
        {
            invocation = null;

            if (block.Statements.Count == 0)
                return false;

            invocation = (block.Statements.First() as ExpressionStatementSyntax)?
                .Expression as InvocationExpressionSyntax;
            return invocation != null;
        }
    }
}