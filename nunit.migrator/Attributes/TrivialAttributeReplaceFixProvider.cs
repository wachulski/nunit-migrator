using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace NUnit.Migrator.Attributes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class TrivialAttributeReplaceFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.DeprecatedReplaceableAttribute.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var attributeSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeSyntax>();

            context.RegisterCodeFix(new ReplaceDeprecatedAttribute(document, root, attributeSyntax), 
                context.Diagnostics);
        }

        private class ReplaceDeprecatedAttribute : CodeAction
        {
            private readonly Document _document;
            private readonly SyntaxNode _root;
            private readonly AttributeSyntax _attributeSyntax;
            private readonly string _targetString;
            private readonly string _targetFamily;

            public sealed override string EquivalenceKey => 
                $"DeprecatedAttributeReplaceableWith{_targetFamily}FamilyFixKey";

            public sealed override string Title => $"Replace with {_targetString}";

            public ReplaceDeprecatedAttribute(Document document, SyntaxNode root, AttributeSyntax attributeSyntax)
            {
                _document = document;
                _root = root;
                _attributeSyntax = attributeSyntax;
                _targetString = 
                    TrivialAttributeReplaceAnalyzer.GetRefinedAttributeWithItsMigrationTarget(_attributeSyntax)
                        .replaceWith;
                _targetFamily = _targetString.Substring(0, 5);
            }

            protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var nameAndArgumentsList = _targetString.Split(new[] {'('}, StringSplitOptions.RemoveEmptyEntries);
                var newAttributeName = SyntaxFactory.ParseName(nameAndArgumentsList[0]);

                AttributeSyntax newAttribute;
                var thereIsNameOnlyAndNoArgumentsList = nameAndArgumentsList.Length == 1;
                if (thereIsNameOnlyAndNoArgumentsList)
                {
                    newAttribute = SyntaxFactory.Attribute(newAttributeName);
                }
                else
                {
                    var argumentsList = $"({nameAndArgumentsList[1]}"; // the ending ')' remains after the initial split
                    newAttribute = SyntaxFactory.Attribute(newAttributeName, 
                        SyntaxFactory.ParseAttributeArgumentList(argumentsList));
                }

                newAttribute = newAttribute.WithAdditionalAnnotations(Formatter.Annotation);

                var newRoot = _root.ReplaceNode(_attributeSyntax, newAttribute);

                return Task.FromResult(_document.WithSyntaxRoot(newRoot));
            }
        }
    }
}