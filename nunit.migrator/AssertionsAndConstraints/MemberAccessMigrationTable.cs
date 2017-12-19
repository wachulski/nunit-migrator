using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.AssertionsAndConstraints
{
    internal static class MemberAccessMigrationTable
    {
        internal static bool TryGetFixExpression(MemberAccessExpressionSyntax memberAccess, 
            out ExpressionSyntax fixExpression)
        {
            var memberAccessMethodName = memberAccess.Name.Identifier.Text;
            fixExpression = null;

            if (!MemberAccessMethodNames.Contains(memberAccessMethodName)) // to filter out early
                return false;

            var lookupName = $"{memberAccess.Expression}.{memberAccess.Name.Identifier.Text}";

            if (!MemberAccessFixingMap.TryGetValue(lookupName, out fixExpression))
                return false;

            return true;
        }

        private static readonly IImmutableDictionary<string, ExpressionSyntax> MemberAccessFixingMap =
            new Dictionary<string, ExpressionSyntax>
            {
                ["Assert.IsNullOrEmpty"]    = Parse("Is.Null.Or.Empty"),
                ["Assert.IsNotNullOrEmpty"] = Parse("Is.Not.Null.And.Not.Empty"),
                ["Text.All"]                = Parse("Is.All"),
                ["Text.Contains"]           = Parse("Does.Contain"),
                ["Text.DoesNotContain"]     = Parse("Does.Not.Contain"),
                ["Text.StartsWith"]         = Parse("Does.StartWith"),
                ["Text.DoesNotStartWith"]   = Parse("Does.Not.StartWith"),
                ["Text.EndsWith"]           = Parse("Does.EndWith"),
                ["Text.DoesNotEndWith"]     = Parse("Does.Not.EndWith"),
                ["Text.Matches"]            = Parse("Does.Match"),
                ["Text.DoesNotMatch"]       = Parse("Does.Not.Match"),
                ["Is.StringStarting"]       = Parse("Does.StartWith"),
                ["Is.StringEnding"]         = Parse("Does.EndWith"),
                ["Is.StringContaining"]     = Parse("Does.Contain"),
                ["Is.StringMatching"]       = Parse("Does.Match"),
                ["Is.InstanceOfType"]       = Parse("Is.InstanceOf"),
            }.ToImmutableDictionary();

        private static readonly IImmutableSet<string> MemberAccessMethodNames = 
            MemberAccessFixingMap
            .Keys
            .Select(k => k.Split('.')[1])
            .ToImmutableHashSet();

        private static ExpressionSyntax Parse(string strExpression) => SyntaxFactory.ParseExpression(strExpression);
    }
}