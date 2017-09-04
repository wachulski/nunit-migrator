using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.Model
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
        /// Represents <c>NUnit.Framework.TestCaseAttribute</c> attributes that do not contain exception properties, 
        /// but belong to a method decorated with <c>NUnit.Framework.ExpectedExceptionAttribute</c>.
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
        }
    }
}