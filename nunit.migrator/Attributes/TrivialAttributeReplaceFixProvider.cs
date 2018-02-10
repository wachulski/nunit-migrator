using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    }
}