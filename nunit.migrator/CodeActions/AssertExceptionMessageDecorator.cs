using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.Helpers;
using NUnit.Migrator.Model;

namespace NUnit.Migrator.CodeActions
{
    internal class AssertExceptionMessageDecorator : AssertExceptionBlockDecorator
    {
        private readonly ExceptionExpectancyAtAttributeLevel _attribute;

        public AssertExceptionMessageDecorator(IAssertExceptionBlockCreator blockCreator,
            ExceptionExpectancyAtAttributeLevel attribute) : base(blockCreator)
        {
            _attribute = attribute;
        }

        public override BlockSyntax Create(MethodDeclarationSyntax method, TypeSyntax assertedType)
        {
            var body = base.Create(method, assertedType);

            return body.AddStatements(CreateAssertExceptionMessageStatement());
        }

        private IEnumerable<ArgumentSyntax> CreateExceptionMessageAssertThatArguments()
        {
            return new[]
            {
                SyntaxFactory.Argument(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(AssignAssertThrowsToLocalVariableDecorator
                            .LocalExceptionVariableName),
                        SyntaxFactory.IdentifierName(nameof(Exception.Message)))),

                SyntaxFactory.Argument(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NUnitFramework.IsIdentifier),
                            SyntaxFactory.IdentifierName(
                                CreateExpectedExceptionMessageAssertionMethod())),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.ParseToken($"\"{_attribute.ExpectedMessage}\""
                                        )))))))
            };
        }

        private string CreateExpectedExceptionMessageAssertionMethod()
        {
            var matchType = _attribute.MatchType;

            switch (matchType)
            {
                case NUnitFramework.MessageMatch.Contains: return NUnitFramework.Is.StringContaining;
                case NUnitFramework.MessageMatch.Regex: return NUnitFramework.Is.StringMatching;
                default: return NUnitFramework.Is.EqualTo;
            }
        }

        private ExpressionStatementSyntax CreateAssertExceptionMessageStatement()
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(NUnitFramework.AssertIdentifier),
                        SyntaxFactory.IdentifierName(NUnitFramework.Assert.ThatIdentifier)),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            CreateExceptionMessageAssertThatArguments()))));
        }
    }
}