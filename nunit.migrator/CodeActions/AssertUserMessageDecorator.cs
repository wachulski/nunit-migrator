using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.Model;

namespace NUnit.Migrator.CodeActions
{
    internal class AssertUserMessageDecorator : AssertExceptionBlockDecorator
    {
        private readonly ExceptionExpectancyAtAttributeLevel _attribute;

        public AssertUserMessageDecorator(IAssertExceptionBlockCreator blockCreator,
            ExceptionExpectancyAtAttributeLevel attribute) : base(blockCreator)
        {
            _attribute = attribute;
        }

        public override BlockSyntax Create(MethodDeclarationSyntax method, TypeSyntax assertedType)
        {
            var body = base.Create(method, assertedType);

            if (!TryFindAssertThrowsInvocation(body, out InvocationExpressionSyntax invocation))
                return body;

            var userMessageArgument = SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.ParseToken($"\"{_attribute.UserMessage}\""))); // TODO: no need for full attribute, msg only

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