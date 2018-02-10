using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.Attributes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class ChangedSemanticFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.AttributeChangedSemantic.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var attributeSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeSyntax>();
            var methodOrClassSyntax = attributeSyntax?.Parent?.Parent; // first parent is attribute list
            if (methodOrClassSyntax == null)
                return;

            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken);
            var classOrMethodSymbol = semanticModel.GetDeclaredSymbol(methodOrClassSyntax);
            if (classOrMethodSymbol == null
                || (classOrMethodSymbol.Kind != SymbolKind.Method && classOrMethodSymbol.Kind != SymbolKind.NamedType)
                || !classOrMethodSymbol.GetAttributes().Any(IsTestRelated))
            {
                return;
            }

            var codeAction = new ReplaceDeprecatedAttribute(
                document, root, attributeSyntax);
            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static bool IsTestRelated(AttributeData attr) => 
            attr.AttributeClass.ContainingNamespace.ToString() == "NUnit.Framework"
            && (attr.AttributeClass.Name == "TestAttribute" 
                || attr.AttributeClass.Name == "TestCaseAttribute"
                || attr.AttributeClass.Name == "TestFixtureAttribute");
    }
}