using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Formatting;

namespace NUnit.Migrator.AssertionsAndConstraints
{
    internal class MemberAccessCodeAction<TMemberAccessContainerNode> : CodeAction
        where TMemberAccessContainerNode : SyntaxNode
    {
        private readonly Document _document;
        private readonly TMemberAccessContainerNode _container;
        private readonly MemberAccessBasedMigration<TMemberAccessContainerNode> _migration;
        private readonly Lazy<TMemberAccessContainerNode> _fixedContainer;

        public sealed override string EquivalenceKey => $"{_migration.DiagnosticDescriptor.Id}FixKey";

        public sealed override string Title => 
            Texts.CodeActionTitle($"Replace with {_migration.CreateReplaceWithTargetString(_fixedContainer.Value)}");

        internal MemberAccessCodeAction(Document document, TMemberAccessContainerNode container, 
            MemberAccessBasedMigration<TMemberAccessContainerNode> migration)
        {
            _document = document;
            _container = container;
            _migration = migration;
            _fixedContainer = new Lazy<TMemberAccessContainerNode>(() => _migration.CreateFixedContainer(_container));
        }

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var root = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var fixedMemberAccessContainer = _fixedContainer.Value.WithAdditionalAnnotations(Formatter.Annotation);
            var newRoot = root.ReplaceNode(_container, fixedMemberAccessContainer);

            return _document.WithSyntaxRoot(newRoot);
        }
    }
}