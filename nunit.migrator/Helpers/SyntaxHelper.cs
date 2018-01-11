using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.Helpers
{
    internal static class SyntaxHelper
    {
        internal delegate void ArgumentParseAction(string nameEquals, ExpressionSyntax expression);

        public static MethodDeclarationSyntax WithoutExceptionExpectancyInAttributes(
            this MethodDeclarationSyntax method, AttributeSyntax[] testCasesToRemain)
        {
            var resultMethod = method.RemoveNodes(GetExpectedExceptionsToRemove(method)
                    .Union(GetTestCasesToRemove(method, testCasesToRemain))
                    .Union(GetTestCaseArgsToRemove(method)),
                SyntaxRemoveOptions.KeepNoTrivia);

            return resultMethod.RemoveNodes(GetEmptyAttributeLists(resultMethod)
                    .Union(GetEmptyAttributeArgumentLists(resultMethod)),
                SyntaxRemoveOptions.KeepNoTrivia);
        }

        internal static TypeSyntax[] GetAllBaseTypes(BaseTypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.BaseList?.Types.Select(t => t.Type).ToArray() ?? new TypeSyntax[] {};
        }

        internal static void ParseAttributeArguments(AttributeSyntax attribute,
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

        private static IEnumerable<SyntaxNode> GetEmptyAttributeArgumentLists(BaseMethodDeclarationSyntax resultMethod)
        {
            return GetMethodAttributes(resultMethod, NUnitFramework.TestCaseAttributeSimpleName)
                .Select(at => at.ArgumentList)
                .Where(al => !al.Arguments.Any());
        }

        private static IEnumerable<SyntaxNode> GetEmptyAttributeLists(BaseMethodDeclarationSyntax resultMethod)
        {
            return resultMethod.AttributeLists.Where(al => !al.Attributes.Any());
        }

        private static IEnumerable<SyntaxNode> GetTestCaseArgsToRemove(BaseMethodDeclarationSyntax method)
        {
            return GetMethodAttributes(method, NUnitFramework.TestCaseAttributeSimpleName)
                .SelectMany(at => at.ArgumentList.Arguments)
                .Where(IsArgumentExpectingException);
        }

        private static IEnumerable<SyntaxNode> GetTestCasesToRemove(BaseMethodDeclarationSyntax method, 
            AttributeSyntax[] testCasesToRemain)
        {
            return GetMethodAttributes(method, NUnitFramework.TestCaseAttributeSimpleName,
                at => !testCasesToRemain.Contains(at));
        }

        private static IEnumerable<SyntaxNode> GetExpectedExceptionsToRemove(BaseMethodDeclarationSyntax method)
        {
            return GetMethodAttributes(method, NUnitFramework.ExpectedExceptionSimpleName);
        }

        private static IEnumerable<AttributeSyntax> GetMethodAttributes(BaseMethodDeclarationSyntax method, 
            string simpleName, Predicate<AttributeSyntax> attributePredicate = null)
        {
            return method
                .AttributeLists
                .SelectMany(al => al.Attributes)
                .Where(at => at.Name.ToString() == simpleName 
                       && (attributePredicate?.Invoke(at) ?? true));
        }

        private static bool IsArgumentExpectingException(AttributeArgumentSyntax arg)
        {
            var nameEquals = arg.NameEquals?.Name?.Identifier.ToString();

            return nameEquals != null &&
                   (nameEquals == NUnitFramework.ExpectedExceptionArgument.ExpectedExceptionName
                    || nameEquals == NUnitFramework.ExpectedExceptionArgument.ExpectedException
                    || nameEquals == NUnitFramework.ExpectedExceptionArgument.ExpectedMessage
                    || nameEquals == NUnitFramework.ExpectedExceptionArgument.MatchType);
        }
    }
}