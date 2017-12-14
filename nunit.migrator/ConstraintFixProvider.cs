using System.Collections.Generic;
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
            _v3ApiMemberAccessSyntax = CreateV3ConstraintToFixWith(memberAccess);
        }

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var root = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(_memberAccess, _v3ApiMemberAccessSyntax);

            return _document.WithSyntaxRoot(newRoot);
        }

        internal static SyntaxNode CreateV3ConstraintToFixWith(MemberAccessExpressionSyntax v2ConstraintMemberAccess)
        {
            var v3Constraint = MemberAccessFixingMap[GetMemberAccessLookupName(v2ConstraintMemberAccess)];

            if (v2ConstraintMemberAccess.Name is GenericNameSyntax genericName)
                v3Constraint = SyntaxFactory.ParseExpression($"{v3Constraint}{genericName.TypeArgumentList}");

            return v3Constraint;
        }

        internal static bool IsMemberAccessMigratableConstraint(MemberAccessExpressionSyntax memberAccess)
        {
            string memberAccessMethodName = memberAccess.Name.Identifier.Text;

            if (!MemberAccessMethodNames.Contains(memberAccessMethodName)) // to filter out early
                return false;

            if (!MemberAccessFixingMap.ContainsKey(GetMemberAccessLookupName(memberAccess)))
                return false;

            return true;
        }

        private static string GetMemberAccessLookupName(MemberAccessExpressionSyntax memberAccess) =>
            $"{memberAccess.Expression}.{memberAccess.Name.Identifier.Text}";

        private static readonly IImmutableDictionary<string, SyntaxNode> MemberAccessFixingMap =
            new Dictionary<string, SyntaxNode>
            {
                ["Text.All"]              = Parse("Is.All"),
                ["Text.Contains"]         = Parse("Does.Contain"),
                ["Text.DoesNotContain"]   = Parse("Does.Not.Contain"),
                ["Text.StartsWith"]       = Parse("Does.StartWith"),
                ["Text.DoesNotStartWith"] = Parse("Does.Not.StartWith"),
                ["Text.EndsWith"]         = Parse("Does.EndWith"),
                ["Text.DoesNotEndWith"]   = Parse("Does.Not.EndWith"),
                ["Text.Matches"]          = Parse("Does.Match"),
                ["Text.DoesNotMatch"]     = Parse("Does.Not.Match"),
                ["Is.StringStarting"]     = Parse("Does.StartWith"),
                ["Is.StringEnding"]       = Parse("Does.EndWith"),
                ["Is.StringContaining"]   = Parse("Does.Contain"),
                ["Is.StringMatching"]     = Parse("Does.Match"),
                ["Is.InstanceOfType"]     = Parse("Is.InstanceOf"),
            }.ToImmutableDictionary();

        // quick filtering purpose
        private static readonly IImmutableSet<string> MemberAccessMethodNames =
            MemberAccessFixingMap.Keys
                .Select(k => k.Split('.')[1])
                .ToImmutableHashSet();

        private static SyntaxNode Parse(string strExpression) => SyntaxFactory.ParseExpression(strExpression);
    }
}