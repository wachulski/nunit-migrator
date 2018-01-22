using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.Attributes
{
    /// <summary>
    /// Representation of test (test fixture) ignoring that may come from <c>NUnit.Framework.IgnoreAttribute</c>
    /// or <c>NUnit.Framework.TestCase</c> or <c>NUnit.Framework.TestFixture</c>.
    /// </summary>
    internal class TestIgnoringModel
    {
        private const string DefaultReasonMessageFix = "\"TODO: provide a reason\"";

        public readonly AttributeSyntax Attribute;

        public bool DoesIgnoringNeedAdjustment => Kind == IgnoreKind.IgnoreAttribute
            ? ReasonExpressionSyntax == null
            : !IsIgnoreArgumentSetToStringValue
              && (IsIgnoreArgumentSetToBoolValue
                  || IsIgnoredArgumentSetToBoolValue
                  || ReasonExpressionSyntax != null);

        public string ReportMessage
        {
            get
            {
                var attributeName = Attribute.Name;

                return Kind == IgnoreKind.IgnoreAttribute
                    ? $"'{attributeName}' attribute should provide ignoring reason."
                    : $"'{attributeName}' if being ignored should provide reason in its 'Ignore' argument.";
            }
        }

        private IgnoreKind Kind { get; }

        /// <summary>
        /// <c>Ignore</c> attribute first and only positional argument containing reason message or 
        /// <c>IgnoreReason</c> attribute argument expression syntax in TestCase or TestFixture or
        /// already fixed to v3 <c>Ignore</c> attribute argument expression syntax containing reason fix text.
        /// </summary>
        private ExpressionSyntax ReasonExpressionSyntax { get; set; }

        /// <summary>
        /// <c>Ignore</c> attribute argument presence (either true/false or const) in TestCase or TestFixture.
        /// </summary>
        private bool IsIgnoreArgumentSetToBoolValue { get; set; }

        /// <summary>
        /// <c>Ignore</c> attribute argument has already been fixed to v3 version when it contains a string reason
        /// </summary>
        private bool IsIgnoreArgumentSetToStringValue { get; set; }

        /// <summary>
        /// <c>Ignore</c> attribute argument presence (either true/false or const) in TestCase or TestFixture.
        /// </summary>
        private bool IsIgnoredArgumentSetToBoolValue { get; set; }

        public TestIgnoringModel(AttributeSyntax attribute)
        {
            Attribute = attribute;

            Kind = attribute.Name.ToString().Contains("Ignore")
                ? IgnoreKind.IgnoreAttribute
                : IgnoreKind.IgnoreArgumentInTestCaseOrTestFixture;

            if (Kind == IgnoreKind.IgnoreAttribute)
                SyntaxHelper.ParseAttributeArguments(attribute, ParseIgnoreAttributeArgs);
            else
                SyntaxHelper.ParseAttributeArguments(attribute, ParseTestCaseOrFixtureArgs);
        }

        public AttributeSyntax GetFixedAttribute()
        {
            var fixedAttribute = Kind == IgnoreKind.IgnoreAttribute 
                ? FixIgnoreAttribute() 
                : FixTestCaseOrTestFixtureIgnoreArgs();

            return fixedAttribute.WithAdditionalAnnotations(Formatter.Annotation);
        }

        private AttributeSyntax FixTestCaseOrTestFixtureIgnoreArgs()
        {
            var newArgs = Attribute.ArgumentList.Arguments
                .Where(arg => !arg.NameEquals?.Name?.ToString().Contains("Ignore") ?? true)
                .ToList();
            newArgs.Add(
                SyntaxFactory.AttributeArgument(
                    nameEquals: SyntaxFactory.NameEquals("Ignore"),
                    nameColon: null,
                    expression: ReasonExpressionSyntax ??
                                SyntaxFactory.ParseExpression(DefaultReasonMessageFix)));

            var newAttributeWithFixedIgnoreArgs = Attribute.WithArgumentList(
                SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(newArgs)));

            return newAttributeWithFixedIgnoreArgs;
        }

        private AttributeSyntax FixIgnoreAttribute()
        {
            return SyntaxFactory.Attribute(Attribute.Name,
                SyntaxFactory.ParseAttributeArgumentList($"({DefaultReasonMessageFix})"));
        }

        private void ParseIgnoreAttributeArgs(string name, ExpressionSyntax expression)
        {
            ReasonExpressionSyntax = expression;
        }

        private void ParseTestCaseOrFixtureArgs(string name, ExpressionSyntax expression)
        {
            switch (name)
            {
                // the following one is a case when already Ignore argument has already been fixed, we don't want twice
                case "Ignore" when expression is LiteralExpressionSyntax stringExpression
                                   && stringExpression.Kind() == SyntaxKind.StringLiteralExpression:
                    IsIgnoreArgumentSetToStringValue = true;
                    ReasonExpressionSyntax = expression;
                    break;
                case "Ignore":
                    IsIgnoreArgumentSetToBoolValue = true;
                    break;
                case "Ignored":
                    IsIgnoredArgumentSetToBoolValue = true;
                    break;
                case "IgnoreReason":
                    ReasonExpressionSyntax = expression;
                    break;
            }
        }

        private enum IgnoreKind
        {
            IgnoreAttribute,
            IgnoreArgumentInTestCaseOrTestFixture
        }
    }
}