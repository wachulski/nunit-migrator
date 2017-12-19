using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace NUnit.Migrator.AssertionsAndConstraints
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class MemberAccessBasedFixProvider : CodeFixProvider
    {
        private static readonly IFixer[] MigrationFixers = {
            new AssertionMigration(), new ConstraintMigration()
        };

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            MigrationFixers
                .Select(mf => mf.FixableDiagnostic.Id)
                .ToArray());

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var migrationFixer in MigrationFixers)
            {
                migrationFixer.RegisterFixing(context, document, root);
            }
        }
    }
}