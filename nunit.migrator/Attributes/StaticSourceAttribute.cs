using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.Attributes
{
    /// <summary>
    /// <c>NUnit.Framework.TestCaseSource</c> or <c>NUnit.Framework.ValueSource</c> attribute model
    /// </summary>
    internal class StaticSourceAttribute
    {
        private const string ContainingTypeQualifiedNameParamName = "containingTypeQualifiedName";
        private const string MemberNameParamName = "memberName";

        private static readonly SymbolDisplayFormat ContainingTypeParamSerializationFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        private readonly AttributeSyntax _attributeNode;
        private readonly TypeSyntax _containingTypeSyntax;
        private readonly Lazy<ITypeSymbol> _containingTypeSymbolLazy;

        public StaticSourceAttribute(AttributeSyntax attribute, SemanticModel semanticModel)
        {
            _attributeNode = attribute;
            var arguments = attribute.ArgumentList.Arguments;

            foreach (var arg in arguments)
            {
                switch (arg.Expression)
                {
                    case LiteralExpressionSyntax sourceName:
                        MemberName = sourceName.Token.ValueText;
                        break;
                    case TypeOfExpressionSyntax sourceType:
                        _containingTypeSyntax = sourceType.Type;
                        break;
                    case InvocationExpressionSyntax invocation when invocation.Expression.ToString() == "nameof":
                        MemberName = ExtractMemberNameFromNameOf(invocation);
                        break;
                }
            }

            _containingTypeSymbolLazy = new Lazy<ITypeSymbol>(() =>
            {
                if (_containingTypeSyntax != null)
                    return semanticModel.GetSymbolInfo(_containingTypeSyntax).Symbol as ITypeSymbol;

                var containingTypeSyntax = _attributeNode.FirstAncestorOrSelf<TypeDeclarationSyntax>();

                return semanticModel.GetDeclaredSymbol(containingTypeSyntax) as ITypeSymbol;
            });
        }

        public string AttributeName => _attributeNode.Name.ToString();

        public string MemberName { get; }

        public ITypeSymbol ContainingSymbol => _containingTypeSymbolLazy.Value;

        public string MemberFullPath => ContainingSymbol != null
            ? $"{SerializeContainingSymbol(ContainingSymbol)}.{MemberName}" 
            : MemberName;

        public static string SerializeContainingSymbol(ISymbol containingSymbol)
        {
            return containingSymbol?.ToDisplayString(ContainingTypeParamSerializationFormat);
        }

        public ImmutableDictionary<string, string> GetFixerParams()
        {
            var containingTypeName = ContainingSymbol?.ToDisplayString(ContainingTypeParamSerializationFormat);

            return new Dictionary<string, string>
            {
                [ContainingTypeQualifiedNameParamName] = containingTypeName,
                [MemberNameParamName] = MemberName
            }.ToImmutableDictionary();
        }

        public static (string containingType, string memberName) GetContainingTypeAndMemberNames(
            IDictionary<string, string> properties)
        {
            return (properties[ContainingTypeQualifiedNameParamName], properties[MemberNameParamName]);
        }

        private static string ExtractMemberNameFromNameOf(InvocationExpressionSyntax nameOf)
        {
            var argument = nameOf.ArgumentList.Arguments.FirstOrDefault();
            if (argument == null)
                return string.Empty;

            switch (argument.Expression)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    return memberAccess.Name.Identifier.Text;
                case IdentifierNameSyntax identifierName:
                    return identifierName.Identifier.Text;
                default:
                    return string.Empty;
            }
        }
    }
}