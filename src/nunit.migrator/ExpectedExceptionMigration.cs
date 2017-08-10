using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace NUnit.Migrator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExpectedExceptionAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.ExpectedException);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(ctx =>
            {
                var expectedExceptionSymbol = ctx.Compilation.GetTypeByMetadataName(
                    NUnitTypes.ExpectedExceptionAttributeQualifiedName);
                if (expectedExceptionSymbol == null)
                    return;

                ctx.RegisterSyntaxNodeAction(syntaxNodeContext =>
                    AnalyzeAttribute(syntaxNodeContext, expectedExceptionSymbol), SyntaxKind.Attribute);
            });
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context, 
            INamedTypeSymbol expectedExceptionDefinitionSymbol)
        {
            if (IsNodeBeingAnalyzedExpectedAttributeToBeMigrated(context, expectedExceptionDefinitionSymbol,
                out AttributeSyntax attributeBeingAnalyzed))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.ExpectedException, 
                    attributeBeingAnalyzed.GetLocation()));
            }
        }

        private static bool IsNodeBeingAnalyzedExpectedAttributeToBeMigrated(SyntaxNodeAnalysisContext context,
            INamedTypeSymbol expectedExceptionDefinitionSymbol, out AttributeSyntax attributeBeingAnalyzed)
        {
            attributeBeingAnalyzed = (AttributeSyntax) context.Node;
            var symbolBeingAnalyzed = context.SemanticModel.GetSymbolInfo(attributeBeingAnalyzed).Symbol?
                .ContainingSymbol;
            Debug.Assert(symbolBeingAnalyzed != null, "symbolBeingAnalyzed != null");

            return expectedExceptionDefinitionSymbol.Equals(symbolBeingAnalyzed);
        }
    }

    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class ExpectedExceptionFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            Descriptors.ExpectedException.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var attributeToBeMigrated = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeSyntax>();
            Debug.Assert(attributeToBeMigrated != null, "attributeToBeMigrated != null");
            var testMethodDeclaration = attributeToBeMigrated.FirstAncestorOrSelf<MethodDeclarationSyntax>();

            var codeFixAction = new ConvertToAssertThrowsCodeAction(context.Document, attributeToBeMigrated,
                testMethodDeclaration);

            context.RegisterCodeFix(codeFixAction, context.Diagnostics);
        }

        private class ConvertToAssertThrowsCodeAction : CodeAction
        {
            private const string LocalExceptionVariableName = "ex";

            private readonly Document _document;
            private readonly AttributeSyntax _expectedExceptionAttribute;
            private readonly MethodDeclarationSyntax _testMethodDeclaration;

            public ConvertToAssertThrowsCodeAction(Document document, AttributeSyntax expectedExceptionAttribute, 
                MethodDeclarationSyntax testMethodDeclaration)
            {
                _document = document;
                _expectedExceptionAttribute = expectedExceptionAttribute;
                _testMethodDeclaration = testMethodDeclaration;
            }

            public override string EquivalenceKey => "ConvertToAssertThrowsCodeAction";

            public override string Title => "Convert to Assert.Throws<T>";

            protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var editor = await DocumentEditor.CreateAsync(_document, cancellationToken).ConfigureAwait(false);
                var expectedExceptionArgs = new ExpectedExceptionAttributeArgs(_expectedExceptionAttribute);
                if (!expectedExceptionArgs.AreValid)
                    return _document;

                var fixedTestMethod = FixTestMethodThatExpectsExceptionToBeThrown(expectedExceptionArgs);
                editor.ReplaceNode(_testMethodDeclaration, fixedTestMethod);

                return editor.GetChangedDocument();
            }

            private SyntaxNode FixTestMethodThatExpectsExceptionToBeThrown(
                ExpectedExceptionAttributeArgs expectedExceptionArgs)
            {
                var fixedTestMethodBlock = CreateFixedTestMethodBodyBlock(expectedExceptionArgs);
                var assertedMethod = RemoveExpectedExceptionAttributeAndReplaceMethodBody(fixedTestMethodBlock);
                var assertedFormattedMethod = FormatMethodAccordingToDocumentFormatingRules(assertedMethod);

                return assertedFormattedMethod;
            }

            private BlockSyntax CreateFixedTestMethodBodyBlock(
                ExpectedExceptionAttributeArgs expectedExceptionArgs)
            {
                var assertThrowsInvocation = CreateAssertThrowsInvocation(expectedExceptionArgs);

                if (expectedExceptionArgs.ExpectedMessage == null)
                    return SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(assertThrowsInvocation));

                return SyntaxFactory.Block(
                    CreateActualExceptionVariableAssignment(assertThrowsInvocation), 
                    CreateAssertExceptionMessageStatement(expectedExceptionArgs));
            }

            private static ExpressionStatementSyntax CreateAssertExceptionMessageStatement(
                ExpectedExceptionAttributeArgs expectedExceptionArgs)
            {
                return SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, 
                            SyntaxFactory.IdentifierName(NUnitTypes.AssertIdentifier),
                            SyntaxFactory.IdentifierName(NUnitTypes.Assert.ThatIdentifier)),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                CreateExceptionMessageAssertThatArguments(expectedExceptionArgs)))));
            }

            private static IEnumerable<ArgumentSyntax> CreateExceptionMessageAssertThatArguments(
                ExpectedExceptionAttributeArgs expectedExceptionArgs)
            {
                yield return SyntaxFactory.Argument(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(LocalExceptionVariableName),
                        SyntaxFactory.IdentifierName(nameof(System.Exception.Message))));

                yield return SyntaxFactory.Argument(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NUnitTypes.IsIdentifier),
                            SyntaxFactory.IdentifierName(
                                GetExpectedExceptionMessageAssertionMethod(expectedExceptionArgs))),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.ParseToken($"\"{expectedExceptionArgs.ExpectedMessage}\"")))))));
            }

            private static string GetExpectedExceptionMessageAssertionMethod(
                ExpectedExceptionAttributeArgs expectedExceptionArgs)
            {
                var matchType = expectedExceptionArgs.MatchType;

                switch (matchType)
                {
                    case NUnitTypes.MessageMatch.Contains: return NUnitTypes.Is.StringContaining;
                    case NUnitTypes.MessageMatch.Regex: return NUnitTypes.Is.StringMatching;
                    default: return NUnitTypes.Is.EqualTo;
                }
            }

            private static LocalDeclarationStatementSyntax CreateActualExceptionVariableAssignment(
                ExpressionSyntax assertThrowsInvocation)
            {
                return SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName("var"),
                        SyntaxFactory.SeparatedList(new [] 
                        {
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.ParseToken(LocalExceptionVariableName),
                                null,
                                SyntaxFactory.EqualsValueClause(assertThrowsInvocation))
                        })));
            }

            private InvocationExpressionSyntax CreateAssertThrowsInvocation(
                ExpectedExceptionAttributeArgs expectedExceptionArgs)
            {
                return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NUnitTypes.AssertIdentifier),
                            SyntaxFactory.GenericName(
                                SyntaxFactory.ParseToken(NUnitTypes.Assert.ThrowsIdentifier),
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(expectedExceptionArgs.AssertedExceptionType)))),
                        SyntaxFactory.ArgumentList(
                            CreateAssertThrowsArguments(expectedExceptionArgs)));
            }

            private SeparatedSyntaxList<ArgumentSyntax> CreateAssertThrowsArguments(
                ExpectedExceptionAttributeArgs expectedExceptionArgs)
            {
                var arguments = new List<ArgumentSyntax>
                {
                    SyntaxFactory.Argument(
                        SyntaxFactory.ParenthesizedLambdaExpression(
                            _testMethodDeclaration.Body.WithoutTrailingTrivia()))
                };

                if (expectedExceptionArgs.UserMessage != null)
                {
                    arguments.Add(SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.ParseToken($"\"{expectedExceptionArgs.UserMessage}\""))));
                }

                return SyntaxFactory.SeparatedList(arguments);
            }

            private MethodDeclarationSyntax RemoveExpectedExceptionAttributeAndReplaceMethodBody(
                BlockSyntax newTestMethodBody)
            {
                return _testMethodDeclaration
                    .RemoveNode(_expectedExceptionAttribute, SyntaxRemoveOptions.KeepDirectives)
                    .WithBody(newTestMethodBody)
                    .WithAdditionalAnnotations(Formatter.Annotation);
            }

            private SyntaxNode FormatMethodAccordingToDocumentFormatingRules(MethodDeclarationSyntax assertedMethod)
            {
                return Formatter.Format(
                    assertedMethod,
                    Formatter.Annotation,
                    _document.Project.Solution.Workspace,
                    _document.Project.Solution.Workspace.Options);
            }
        }
    }

    /// <summary>
    /// Value type bag for serialize/deserialize NUnit.Framework.ExpectedException arguments between analyzer and
    /// code fixer.
    /// </summary>
    internal class ExpectedExceptionAttributeArgs
    {
        public bool AreValid { get; private set; }

        public string ExpectedMessage { get; private set; }

        public string UserMessage { get; private set; }

        public string MatchType { get; private set; }

        public TypeSyntax AssertedExceptionType => SyntaxFactory.ParseTypeName(
            AssertedExceptionTypeName ?? "System.Exception");

        private string AssertedExceptionTypeName { get; set; }

        public ExpectedExceptionAttributeArgs(AttributeSyntax attribute)
        {
            Debug.Assert(attribute != null, "attribute != null");

            AreValid = true; // in case any argument is invalid when parsing, it should become false

            if (attribute.ArgumentList == null || !attribute.ArgumentList.Arguments.Any())
                return;

            foreach (var argument in attribute.ArgumentList.Arguments)
            {
                ParseArgumentSyntax(argument);
            }
        }

        private void ParseArgumentSyntax(AttributeArgumentSyntax argument)
        {
            var nameEquals = argument.NameEquals?.Name?.Identifier.ValueText;

            switch (argument.Expression)
            {
                case LiteralExpressionSyntax literalSyntax when IsLiteralValidAssertedType(literalSyntax, nameEquals):
                    AssertedExceptionTypeName = literalSyntax.Token.ValueText;
                    break;
                case LiteralExpressionSyntax literalSyntax when nameEquals == NUnitTypes
                                                                             .ExpectedExceptionArgument.ExpectedMessage:
                    ExpectedMessage = literalSyntax.Token.ValueText;
                    break;
                case LiteralExpressionSyntax literalSyntax when nameEquals == NUnitTypes
                                                                             .ExpectedExceptionArgument.UserMessage:
                    UserMessage = literalSyntax.Token.ValueText;
                    break;
                case MemberAccessExpressionSyntax memberAccess when nameEquals == NUnitTypes
                                                                             .ExpectedExceptionArgument.MatchType:
                    MatchType = memberAccess.Name.ToString();
                    break;
                case TypeOfExpressionSyntax typeSyntax:
                    AssertedExceptionTypeName = typeSyntax.Type.ToString();
                    break;
                default:
                    AreValid = false;
                    break;
            }
        }

        private bool IsLiteralValidAssertedType(LiteralExpressionSyntax literal, string nameEquals) => 
            !IsLiteralNullOrEmpty(literal) 
            && (nameEquals == null || nameEquals == NUnitTypes.ExpectedExceptionArgument.ExpectedExceptionName);

        private static bool IsLiteralNullOrEmpty(LiteralExpressionSyntax literal) => 
            literal.Kind() == SyntaxKind.NullLiteralExpression || string.IsNullOrEmpty(literal.Token.ValueText);
    }
}