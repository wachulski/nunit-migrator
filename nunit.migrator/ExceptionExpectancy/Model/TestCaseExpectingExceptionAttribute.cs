using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.ExceptionExpectancy.Model
{
    /// <summary>
    /// <c>NUnit.Framework.TestCaseAttribute</c> describing expectancy of an exception being thrown.
    /// The attribute arguments related: <c>ExpectedException</c>, <c>ExpectedExceptionName</c>, <c>ExpectedMessage</c>
    /// and <c>MatchType</c> were discontinued in v3 of the framework.
    /// See: http://nunit.org/docs/2.6.4/testCase.html
    /// </summary>
    internal class TestCaseExpectingExceptionAttribute : ExceptionExpectancyAtAttributeLevel
    {
        /// <summary>
        /// Constructs <c>NUnit.Framework.TestCaseAttribute</c> model that allows for sourcing exception related 
        /// properties from <c>NUnit.Framework.ExpectedExceptionAttribute</c> in case of not defining them by itself.
        /// </summary>
        public TestCaseExpectingExceptionAttribute(AttributeSyntax attribute, 
            ExpectedExceptionAttribute expectedException) : base(attribute)
        {
            if (expectedException == null)
                return;

            if (AssertedExceptionTypeName == null)
                AssertedExceptionTypeName = expectedException.AssertedExceptionTypeName;

            if (ExpectedMessage == null)
                ExpectedMessage = expectedException.ExpectedMessage;

            if (MatchType == null)
                MatchType = expectedException.MatchType;

            if (HandlerName == null)
                HandlerName = expectedException.HandlerName;

            if (UserMessage == null)
                UserMessage = expectedException.UserMessage;
        }
    }
}