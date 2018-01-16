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
    public class TrivialAttributeArgumentReplaceFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.DeprecatedReplaceableAttributeArgument.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var attributeSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeArgumentSyntax>();

            context.RegisterCodeFix(new ReplaceDeprecatedAttributeArgument(document, root, attributeSyntax), 
                context.Diagnostics);
        }

        private class ReplaceDeprecatedAttributeArgument : CodeAction
        {
            private readonly Document _document;
            private readonly SyntaxNode _root;
            private readonly AttributeArgumentSyntax _argumentSyntax;

            private readonly string _targetString;
            
            public sealed override string EquivalenceKey => 
                $"DeprecatedAttributeArgumentReplaceableWith{_targetString}FixKey";

            public sealed override string Title => $"Replace with {_targetString}";

            public ReplaceDeprecatedAttributeArgument(Document document, SyntaxNode root, 
                AttributeArgumentSyntax argumentSyntax)
            {
                _document = document;
                _root = root;
                _argumentSyntax = argumentSyntax;
                _targetString =
                    TrivialAttributeArgumentReplaceAnalyzer.ReplacementTable[argumentSyntax.NameEquals.Name.ToString()];
            }

            protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var newArgument = SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals(_targetString), 
                    _argumentSyntax.NameColon, _argumentSyntax.Expression);

                var newRoot = _root.ReplaceNode(_argumentSyntax, newArgument);

                return Task.FromResult(_document.WithSyntaxRoot(newRoot));
            }
        }
    }
}