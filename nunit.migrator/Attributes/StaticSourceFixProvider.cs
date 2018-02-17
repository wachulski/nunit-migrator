using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.Attributes
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class StaticSourceFixProvider : CodeFixProvider
    {
        private const string EquivalenceKey = "StaticSourceFix";

        private static readonly string ActionTitle = Texts.CodeActionTitle("Make test data source static");

        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.StaticSource.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var codeAction = CodeAction.Create(
                ActionTitle,
                async cancelToken =>
                {
                    var unchangedDoc = context.Document;
                    var diagnosticProperties = context.Diagnostics.First().Properties;

                    var memberSymbol = await GetMemberSymbol(diagnosticProperties, unchangedDoc, cancelToken);
                    if (memberSymbol == null)
                        return unchangedDoc;

                    var memberSyntaxRef = await memberSymbol.DeclaringSyntaxReferences[0].GetSyntaxAsync(cancelToken);
                    var memberSyntax = memberSyntaxRef.FirstAncestorOrSelf<MemberDeclarationSyntax>();
                    var fixedMemberSyntax = GetFixedMember(memberSyntax);
                    if (fixedMemberSyntax == null)
                        return unchangedDoc;

                    return await GetFixedDocument(unchangedDoc.Project, memberSyntax, fixedMemberSyntax, cancelToken);
                },
                EquivalenceKey);

            context.RegisterCodeFix(codeAction, context.Diagnostics);

            return Task.FromResult(0);
        }

        private static async Task<ISymbol> GetMemberSymbol(ImmutableDictionary<string, string> properties,
            Document unchangedDocument, CancellationToken cancellationToken)
        {
            var (containingTypeName, memberName) = StaticSourceAttribute.GetContainingTypeAndMemberNames(properties);
            var semanticModel = await unchangedDocument.GetSemanticModelAsync(cancellationToken);
            var foundMember = semanticModel
                .Compilation
                .GetSymbolsWithName(n => n == memberName, SymbolFilter.Member, cancellationToken)
                .FirstOrDefault(s =>
                    StaticSourceAttribute.SerializeContainingSymbol(s.ContainingType) == containingTypeName);

            return foundMember;
        }

        private static SyntaxNode GetFixedMember(MemberDeclarationSyntax memberSyntax)
        {
            var staticToken = SyntaxFactory.Token(SyntaxKind.StaticKeyword);
            SyntaxNode fixedMemberSyntax;

            switch (memberSyntax)
            {
                case PropertyDeclarationSyntax propertySyntax:
                    fixedMemberSyntax = propertySyntax.AddModifiers(staticToken);
                    break;
                case FieldDeclarationSyntax fieldSyntax:
                    fixedMemberSyntax = fieldSyntax.AddModifiers(staticToken);
                    break;
                case MethodDeclarationSyntax methodSyntax:
                    fixedMemberSyntax = methodSyntax.AddModifiers(staticToken);
                    break;
                default:
                    return null;
            }

            return fixedMemberSyntax;
        }

        private static async Task<Document> GetFixedDocument(Project project, MemberDeclarationSyntax memberSyntax,
            SyntaxNode fixedMemberSyntax, CancellationToken cancellationToken)
        {
            var documentToFix = project.GetDocument(memberSyntax.SyntaxTree);
            var root = await memberSyntax.SyntaxTree.GetRootAsync(cancellationToken);
            var fixedRoot = root.ReplaceNode(memberSyntax, fixedMemberSyntax);
            var fixedDocument = documentToFix.WithSyntaxRoot(fixedRoot);
            return fixedDocument;
        }
    }
}