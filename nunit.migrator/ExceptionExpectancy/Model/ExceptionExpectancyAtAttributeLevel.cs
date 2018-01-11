using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.ExceptionExpectancy.CodeActions;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.ExceptionExpectancy.Model
{
    /// <summary>
    /// Expectancy of an exception being thrown in a test method expressed at attribute level. Either
    /// <c>NUnit.Framework.ExpectedException</c> or <c>NUnit.Framework.TestCase</c> containing expected exception
    /// properties.
    /// </summary>
    internal abstract class ExceptionExpectancyAtAttributeLevel
    {
        private const string ImplicitAssertedExceptionTypeAssumedByDefault = "System.Exception";
        private const string ExpectExceptionHandlerMethodName = "HandleException";

        protected internal string AssertedExceptionTypeName;

        protected ExceptionExpectancyAtAttributeLevel(AttributeSyntax attribute)
        {
            AttributeNode = attribute ?? throw new ArgumentNullException(nameof(attribute));

            ParseAttributeArguments(attribute, ParseAttributeArgumentSyntax);
            ParseTestFixtureClass(attribute.FirstAncestorOrSelf<ClassDeclarationSyntax>());
        }

        public AttributeSyntax AttributeNode { get; }

        public string ExpectedMessage { get; protected set; }

        public string MatchType { get; protected set; }

        public TypeSyntax AssertedExceptionType => SyntaxFactory.ParseTypeName(
            AssertedExceptionTypeName ?? ImplicitAssertedExceptionTypeAssumedByDefault);

        public string HandlerName { get; protected set; }

        public string UserMessage { get; protected set; }

        public IAssertExceptionBlockCreator GetAssertExceptionBlockCreator()
        {
            IAssertExceptionBlockCreator creator = new AssertThrowsExceptionCreator();

            if (ExpectedMessage != null || HandlerName != null)
                creator = new AssignAssertThrowsToLocalVariableDecorator(creator);

            if (ExpectedMessage != null)
                creator = new AssertExceptionMessageDecorator(creator, this);

            if (HandlerName != null)
                creator = new AssertHandlerMethodDecorator(creator, HandlerName);

            if (UserMessage != null)
                creator = new AssertUserMessageDecorator(creator, this);

            return creator;
        }

        protected static void ParseAttributeArguments(AttributeSyntax attribute,
            ArgumentParseAction argumentParseAction)
        {
            if (attribute?.ArgumentList == null || !attribute.ArgumentList.Arguments.Any())
                return;

            foreach (var argument in attribute.ArgumentList.Arguments)
            {
                var nameEquals = argument.NameEquals?.Name?.Identifier.ValueText;

                argumentParseAction(nameEquals, argument.Expression);
            }
        }

        private void ParseTestFixtureClass(BaseTypeDeclarationSyntax classDeclaration)
        {
            if (SyntaxHelper.GetAllBaseTypes(classDeclaration).Any(t => t.ToString() == "IExpectException"))
            {
                HandlerName = ExpectExceptionHandlerMethodName;
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