using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;
using static NUnit.Migrator.Descriptors;

namespace NUnit.Migrator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CatchAllSymbolAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            CatchAllAsError,
            CatchAllAsWarning);

        public override void Initialize(AnalysisContext context)
        {            
            context.RegisterCompilationStartAction(ctx =>
            {
                if (!NUnitFramework.Symbols.TryGetNUnitSymbols(ctx.Compilation, out NUnitFramework.Symbols nunit))
                    return;

                var currentDirectorySymbol = ctx.Compilation
                    .GetTypeByMetadataName("System.Environment")?
                    .GetMembers("CurrentDirectory")
                    .FirstOrDefault();
                if (currentDirectorySymbol == null)
                    return;

                RegisterSyntaxAnalysis<AttributeSyntax>(
                    SyntaxKind.Attribute,
                    ctx,
                    new ISymbol[] {nunit.TearDown, nunit.SetUpFixture},
                    symbolLocatorOverride: symbol => symbol?.ContainingSymbol);

                RegisterSyntaxAnalysis<ObjectCreationExpressionSyntax>(
                    SyntaxKind.ObjectCreationExpression,
                    ctx,
                    new ISymbol[] {nunit.NullOrEmptyStringConstraint},
                    syntax => syntax.Type);

                RegisterSyntaxAnalysis<MemberAccessExpressionSyntax>(
                    SyntaxKind.SimpleMemberAccessExpression, 
                    ctx, 
                    new ISymbol[] {nunit.TestContext}, 
                    symbolLocatorOverride: symbol => symbol?.ContainingSymbol);

                RegisterSyntaxAnalysis<MemberAccessExpressionSyntax>(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ctx,
                    new [] {currentDirectorySymbol}.Union(nunit.TestCaseData.GetMembers("Throws")).ToArray());
            });
        }

        private static void RegisterSyntaxAnalysis<TSyntax>(
            SyntaxKind kind, 
            CompilationStartAnalysisContext compilationContext,  
            ISymbol[] acceptedSymbols, 
            Func<TSyntax, CSharpSyntaxNode> syntaxLocatorOverride = null, 
            Func<ISymbol, ISymbol> symbolLocatorOverride = null)
            where TSyntax : CSharpSyntaxNode
        {
            compilationContext.RegisterSyntaxNodeAction(
                syntaxContext =>
                {
                    var initialNode = (TSyntax) syntaxContext.Node;
                    var effectiveNode = syntaxLocatorOverride?.Invoke(initialNode) ?? initialNode;
                    var symbol = syntaxContext.SemanticModel.GetSymbolInfo(effectiveNode).Symbol;
                    symbol = symbolLocatorOverride?.Invoke(symbol) ?? symbol;

                    if (!acceptedSymbols.Any(s => s.Equals(symbol)))
                        return;

                    (string message, DiagnosticDescriptor descriptor) mapping = CatchAllSymbolMap[symbol.MetadataName];
                    syntaxContext.ReportDiagnostic(Diagnostic.Create(mapping.descriptor,
                        effectiveNode.GetLocation(), mapping.message));
                },
                kind);
        }

        private static readonly IReadOnlyDictionary<string, (string message, DiagnosticDescriptor descriptor)> 
            CatchAllSymbolMap = new Dictionary<string, (string, DiagnosticDescriptor)>
        {
            ["TearDownAttribute"] =
            ("There is a change to the logic by which teardown methods are called. See more: " +
             "https://github.com/nunit/docs/wiki/TearDown-Attribute", CatchAllAsWarning),

            ["SetUpFixtureAttribute"] =
            ("Now uses OneTimeSetUpAttribute and OneTimeTearDownAttribute to designate higher-level setup and " +
             "teardown methods. SetUpAttribute and TearDownAttribute are no longer allowed.", CatchAllAsWarning),

            ["NullOrEmptyStringConstraint"] =
            ("No longer supported. Use 'Assert.That(..., Is.Null.Or.Empty)'", CatchAllAsError),

            ["TestContext"] =
            ("The fields available in the TestContext have changed, although the same information remains available " +
            "as for NUnit V2. See more: https://github.com/nunit/docs/wiki/TestContext", CatchAllAsWarning),

            ["CurrentDirectory"] =
            ("No longer set to the directory containing the test assembly. " +
             "Use TestContext.CurrentContext.TestDirectory to locate that directory.", CatchAllAsWarning),

            ["Throws"] =
            ("The Throws Named Property is no longer available. Use Assert.Throws or Assert.That in your test case.", 
                CatchAllAsError),
        };
    }
}