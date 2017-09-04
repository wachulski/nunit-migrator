using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.CodeActions;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Model
{
    /// <summary>
    /// <c>NUnit.Framework.ExpectedExceptionAttribute</c>. It was removed at all in v3 of the framework
    /// (https://github.com/nunit/docs/wiki/Breaking-Changes).
    /// See http://nunit.org/docs/2.6.4/exception.html for the complete reference.
    /// </summary>
    internal class ExpectedExceptionAttribute : ExceptionExpectancyAtAttributeLevel
    {
        public ExpectedExceptionAttribute(AttributeSyntax attribute) : base(attribute)
        {
            ParseAttributeArguments(attribute, ParseAttributeArgumentSyntax);
        }

        public string UserMessage { get; private set; }

        public override IAssertExceptionBlockCreator GetAssertExceptionBlockCreator()
        {
            var creator = base.GetAssertExceptionBlockCreator();

            if (UserMessage != null)
                creator = new AssertUserMessageDecorator(creator, this);

            return creator;
        }

        private void ParseAttributeArgumentSyntax(string nameEquals, ExpressionSyntax expression)
        {
            if (nameEquals != null
                && nameEquals != NUnitFramework.ExpectedExceptionArgument.UserMessage)
                return;

            switch (expression)
            {
                case LiteralExpressionSyntax literal when nameEquals == null && !IsLiteralNullOrEmpty(literal):
                    AssertedExceptionTypeName = literal.Token.ValueText;
                    break;
                case TypeOfExpressionSyntax typeOf:
                    AssertedExceptionTypeName = typeOf.Type.ToString();
                    break;
                case LiteralExpressionSyntax literal when nameEquals ==
                                                          NUnitFramework.ExpectedExceptionArgument.UserMessage:
                    UserMessage = literal.Token.ValueText;
                    break;
            }
        }
    }
}