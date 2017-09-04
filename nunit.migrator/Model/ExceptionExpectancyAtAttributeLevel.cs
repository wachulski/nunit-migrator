using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.CodeActions;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Model
{
    /// <summary>
    /// Expectancy of an exception being thrown in a test method expressed at attribute level. Either
    /// <c>NUnit.Framework.ExpectedException</c> or <c>NUnit.Framework.TestCase</c> containing expected exception
    /// properties.
    /// </summary>
    internal abstract class ExceptionExpectancyAtAttributeLevel
    {
        private const string ImplicitAssertedExceptionTypeAssumedByDefault = "System.Exception";

        protected internal string AssertedExceptionTypeName;

        protected ExceptionExpectancyAtAttributeLevel(AttributeSyntax attribute)
        {
            AttributeNode = attribute ?? throw new ArgumentNullException(nameof(attribute));

            ParseAttributeArguments(attribute, ParseAttributeArgumentSyntax);
        }

        public AttributeSyntax AttributeNode { get; }

        public string ExpectedMessage { get; protected set; }

        public string MatchType { get; protected set; }

        public TypeSyntax AssertedExceptionType => SyntaxFactory.ParseTypeName(
            AssertedExceptionTypeName ?? ImplicitAssertedExceptionTypeAssumedByDefault);

        public virtual IAssertExceptionBlockCreator GetAssertExceptionBlockCreator()
        {
            IAssertExceptionBlockCreator creator = new AssertThrowsExceptionCreator();

            if (ExpectedMessage != null)
            {
                creator = new AssertExceptionMessageDecorator(
                    new AssignAssertThrowsToLocalVariableDecorator(
                        creator),
                    this);
            }

            return creator;
        }

        protected static void ParseAttributeArguments(AttributeSyntax attribute,
            ArgumentParseAction argumentParseAction)
        {
            Debug.Assert(attribute != null, "attribute != null");

            if (attribute.ArgumentList == null || !attribute.ArgumentList.Arguments.Any())
                return;

            foreach (var argument in attribute.ArgumentList.Arguments)
            {
                var nameEquals = argument.NameEquals?.Name?.Identifier.ValueText;

                argumentParseAction(nameEquals, argument.Expression);
            }
        }

        protected static bool IsLiteralNullOrEmpty(LiteralExpressionSyntax literal)
        {
            return literal.Kind() == SyntaxKind.NullLiteralExpression || string.IsNullOrEmpty(literal.Token.ValueText);
        }

        private void ParseAttributeArgumentSyntax(string nameEquals, ExpressionSyntax expression)
        {
            if (nameEquals == null)
                return;

            switch (expression)
            {
                case LiteralExpressionSyntax literal when nameEquals ==
                                                          NUnitFramework.ExpectedExceptionArgument.ExpectedExceptionName
                                                          && !IsLiteralNullOrEmpty(literal):
                    AssertedExceptionTypeName = literal.Token.ValueText;
                    break;
                case TypeOfExpressionSyntax typeOf when nameEquals ==
                                                        NUnitFramework.ExpectedExceptionArgument.ExpectedException:
                    AssertedExceptionTypeName = typeOf.Type.ToString();
                    break;
                case LiteralExpressionSyntax literal when nameEquals ==
                                                          NUnitFramework.ExpectedExceptionArgument.ExpectedMessage:
                    ExpectedMessage = literal.Token.ValueText;
                    break;
                case MemberAccessExpressionSyntax memberAccess when nameEquals ==
                                                                    NUnitFramework.ExpectedExceptionArgument.MatchType:
                    MatchType = memberAccess.Name.ToString();
                    break;
            }
        }

        protected delegate void ArgumentParseAction(string nameEquals, ExpressionSyntax expression);
    }
}