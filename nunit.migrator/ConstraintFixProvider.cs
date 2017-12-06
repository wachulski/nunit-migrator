using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class ConstraintFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            Descriptors.Constraint.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var memberAccess = root.FindNode(context.Span).FirstAncestorOrSelf<MemberAccessExpressionSyntax>();

            context.RegisterCodeFix(new ConstraintCodeAction(document, memberAccess), context.Diagnostics);
        }
    }

    public class ConstraintCodeAction : CodeAction
    {
        private readonly Document _document;
        private readonly MemberAccessExpressionSyntax _memberAccess;
        private readonly SyntaxNode _v3ApiMemberAccessSyntax;

        public override string Title => $"Replace with {_v3ApiMemberAccessSyntax}";

        public override string EquivalenceKey => "ConstraintCodeActionKey";

        public ConstraintCodeAction(Document document, MemberAccessExpressionSyntax memberAccess)
        {
            _document = document;
            _memberAccess = memberAccess;
            _v3ApiMemberAccessSyntax = ConstraintAnalyzer.MemberAccessFixingMap[_memberAccess.Name.Identifier.Text];
        }

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var root = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(_memberAccess, _v3ApiMemberAccessSyntax);

            return _document.WithSyntaxRoot(newRoot);
        }
    }
}