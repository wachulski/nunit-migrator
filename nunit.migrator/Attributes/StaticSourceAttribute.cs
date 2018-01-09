using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.Attributes
{
    /// <summary>
    /// <c>NUnit.Framework.TestCaseSource</c> or <c>NUnit.Framework.ValueSource</c> attribute model
    /// </summary>
    internal class StaticSourceAttribute
    {
        private readonly AttributeSyntax _attributeNode;
        private readonly TypeSyntax _sourceType;

        public string AttributeName => _attributeNode.Name.ToString();

        public string SourceName { get; }

        public string MemberFullPath => _sourceType != null
            ? $"{_sourceType}.{SourceName}"
            : SourceName;

        public StaticSourceAttribute(AttributeSyntax attribute)
        {
            _attributeNode = attribute;
            var arguments = attribute.ArgumentList.Arguments;

            foreach (var arg in arguments)
            {
                switch (arg.Expression)
                {
                    case LiteralExpressionSyntax sourceName:
                        SourceName = sourceName.Token.ValueText;
                        break;
                    case TypeOfExpressionSyntax sourceType:
                        _sourceType = sourceType.Type;
                        break;
                    case InvocationExpressionSyntax invocation when invocation.Expression.ToString() == "nameof":
                        SourceName = ExtractSourceNameFromNameOf(invocation);
                        break;
                }
            }
        }

        public ITypeSymbol GetContainerTypeSymbol(SemanticModel semanticModel)
        {
            if (_sourceType != null)
                return semanticModel.GetSymbolInfo(_sourceType).Symbol as ITypeSymbol;

            var containerType = _attributeNode.FirstAncestorOrSelf<TypeDeclarationSyntax>();

            return semanticModel.GetDeclaredSymbol(containerType) as ITypeSymbol;
        }

        private static string ExtractSourceNameFromNameOf(InvocationExpressionSyntax nameOf)
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