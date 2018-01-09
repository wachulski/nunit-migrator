using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.ExceptionExpectancy.Model
{
    internal class ExceptionExpectancyMethodModel
    {
        public AttributeSyntax[] ExceptionFreeTestCaseAttributeNodes { get; }

        public ExceptionExpectancyAtAttributeLevel[] ExceptionRelatedAttributes { get; }

        public ExceptionExpectancyMethodModel(MethodDeclarationSyntax method, SemanticModel semanticModel, 
            NUnitFramework.Symbols nunit)
        {
            var attributesWithSymbols = GetAttributesWithSymbols(method, semanticModel);
            var attributes = new List<ExceptionExpectancyAtAttributeLevel>();

            var isExpectedException = TryGetFirstExpectedExceptionAttribute(nunit, attributesWithSymbols,
                out ExpectedExceptionAttribute expectedException);
            if (isExpectedException)
            {
                attributes.Add(expectedException);
            }

            var exceptionRelatedTestCases = GetExceptionRelatedTestCases(nunit, attributesWithSymbols, 
                isExpectedException, expectedException);
            attributes.AddRange(exceptionRelatedTestCases);
            ExceptionRelatedAttributes = attributes.ToArray();
            ExceptionFreeTestCaseAttributeNodes = GetExceptionFreeTestCaseAttributeNodes(nunit, attributesWithSymbols, 
                isExpectedException);
        }

        public static bool TryFindDiagnostic(MethodDeclarationSyntax methodSyntax, SemanticModel semanticModel, 
            NUnitFramework.Symbols nunit, out Diagnostic diagnostic)
        {
            var eligibleAttributes = GetEligibleAttributes(methodSyntax, semanticModel, nunit);
            if (eligibleAttributes.Length <= 0)
            {
                diagnostic = null;
                return false;
            }

            var diagnosticLocation = eligibleAttributes.Length == 1 
                ? eligibleAttributes.First().GetLocation()
                : methodSyntax.Identifier.GetLocation();
            var methodName = methodSyntax.Identifier.Text;
            diagnostic = Diagnostic.Create(Descriptors.ExceptionExpectancy, diagnosticLocation, methodName);

            return true;
        }

        private static AttributeSyntax[] GetExceptionFreeTestCaseAttributeNodes(NUnitFramework.Symbols nunit, 
            AttributeWithSymbol[] attributesWithSymbols, bool isExpectedException)
        {
            return attributesWithSymbols
                .Where(x => IsTestCaseAttributeNotExpectingException(x.Attribute, x.Symbol, nunit, isExpectedException))
                .Select(x => x.Attribute)
                .ToArray();
        }

        private static TestCaseExpectingExceptionAttribute[] GetExceptionRelatedTestCases(NUnitFramework.Symbols nunit, 
            AttributeWithSymbol[] attributesWithSymbols, bool isExpectedException, 
            ExpectedExceptionAttribute expectedException)
        {
            return attributesWithSymbols
                .Where(x => IsTestCaseAttributeExpectingException(x.Attribute, x.Symbol, nunit, isExpectedException))
                .Select(x => new TestCaseExpectingExceptionAttribute(x.Attribute, expectedException))
                .ToArray();
        }

        private static bool TryGetFirstExpectedExceptionAttribute(NUnitFramework.Symbols nunit, 
            AttributeWithSymbol[] attributesWithSymbols, out ExpectedExceptionAttribute expectedException)
        {
            var expectedExceptionNode = GetExpectedExceptionAttributes(nunit, attributesWithSymbols)?.FirstOrDefault();

            if (expectedExceptionNode != null)
            {
                expectedException = new ExpectedExceptionAttribute(expectedExceptionNode);
                return true;
            }

            expectedException = null;
            return false;
        }

        private static AttributeWithSymbol[] GetAttributesWithSymbols(BaseMethodDeclarationSyntax method, 
            SemanticModel semanticModel)
        {
            return method.AttributeLists
                .SelectMany(al => al.Attributes)
                .Select(at => new AttributeWithSymbol
                {
                    Attribute = at,
                    Symbol = semanticModel.GetSymbolInfo(at).Symbol?.ContainingSymbol
                }).ToArray();
        }

        private static AttributeSyntax[] GetEligibleAttributes(BaseMethodDeclarationSyntax method, 
            SemanticModel semanticModel, NUnitFramework.Symbols nunit)
        {
            var attributesWithSymbols = GetAttributesWithSymbols(method, semanticModel);
            var expectedExceptionAttributes = GetExpectedExceptionAttributes(nunit, attributesWithSymbols);
            var doesExpectedExceptionAttributeAlsoExist = expectedExceptionAttributes.Any();
            var testCaseAttributes = attributesWithSymbols.Where(x => IsTestCaseAttributeExpectingException(x.Attribute, x.Symbol, nunit,
                        doesExpectedExceptionAttributeAlsoExist))
                .Select(x => x.Attribute);

            return expectedExceptionAttributes.Union(testCaseAttributes).ToArray();
        }

        private static AttributeSyntax[] GetExpectedExceptionAttributes(NUnitFramework.Symbols nunit, 
            AttributeWithSymbol[] attributesWithSymbols)
        {
            return attributesWithSymbols.Where(x => nunit.ExpectedException.Equals(x.Symbol))
                .Select(x => x.Attribute)
                .ToArray();
        }

        private static bool IsTestCaseAttributeExpectingException(AttributeSyntax attribute, ISymbol symbol, 
            NUnitFramework.Symbols nunit, bool doesExpectedExceptionAttributeAlsoExist)
        {
            if (!nunit.TestCase.Equals(symbol))
                return false;

            if (doesExpectedExceptionAttributeAlsoExist)
                return true;

            return attribute.ArgumentList != null 
                && attribute.ArgumentList.Arguments.Any(DefinesExpectedException);
        }

        private static bool IsTestCaseAttributeNotExpectingException(AttributeSyntax attribute, ISymbol symbol, 
            NUnitFramework.Symbols nunit, bool doesExpectedExceptionAttributeAlsoExist)
        {
            if (!nunit.TestCase.Equals(symbol) || doesExpectedExceptionAttributeAlsoExist)
                return false;

            return attribute.ArgumentList == null
                   || attribute.ArgumentList.Arguments.All(arg => !DefinesExpectedException(arg));
        }

        private static bool DefinesExpectedException(AttributeArgumentSyntax attributeArg)
        {
            if (attributeArg.NameEquals == null)
                return false;

            var argName = attributeArg.NameEquals.Name.Identifier.ValueText;

            return argName == NUnitFramework.ExpectedExceptionArgument.ExpectedExceptionName
                   || argName == NUnitFramework.ExpectedExceptionArgument.ExpectedException;
        }

        private struct AttributeWithSymbol
        {
            public AttributeSyntax Attribute;
            public ISymbol Symbol;
        }
    }
}