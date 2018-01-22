using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.Attributes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class IgnoreReasonFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(new[]
        {
            Descriptors.IgnoreReason.Id
        });

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var attributeSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeSyntax>();
            var model = new TestIgnoringModel(attributeSyntax);

            context.RegisterCodeFix(new FixMissingIgnoreReason(document, root, model), 
                context.Diagnostics);
        }

        class FixMissingIgnoreReason : CodeAction
        {
            private readonly Document _document;
            private readonly SyntaxNode _root;
            private readonly TestIgnoringModel _model;

            public FixMissingIgnoreReason(Document document, SyntaxNode root, TestIgnoringModel model)
            {
                _document = document;
                _root = root;
                _model = model;
            }

            public override string Title { get; } = "Fix ignore reason";

            public override string EquivalenceKey { get; } = "IgnoreReasonFixer";

            protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var newRoot = _root.ReplaceNode(_model.Attribute, _model.GetFixedAttribute());

                return Task.FromResult(_document.WithSyntaxRoot(newRoot));
            }
        }
    }
}