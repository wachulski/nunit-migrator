using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Migrator.CodeActions;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class ExceptionExpectancyFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            Descriptors.ExceptionExpectancy.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var compilation = await document.Project.GetCompilationAsync(context.CancellationToken)
                .ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);

            if (!NUnitFramework.Symbols.TryGetNUnitSymbols(compilation, out NUnitFramework.Symbols nunit))
                return;

            var methodToBeFixed = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();
            Debug.Assert(methodToBeFixed != null, "methodToBeFixed != null");

            var fix = new ExceptionExpectancyCodeAction(document, methodToBeFixed, semanticModel, nunit);

            context.RegisterCodeFix(fix, context.Diagnostics);
        }
    }
}