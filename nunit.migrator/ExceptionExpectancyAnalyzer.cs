using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;
using NUnit.Migrator.Model;

namespace NUnit.Migrator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExceptionExpectancyAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.ExceptionExpectancy);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(ctx =>
            {
                if (!NUnitFramework.Symbols.TryGetNUnitSymbols(ctx.Compilation, out NUnitFramework.Symbols nunit))
                    return;

                ctx.RegisterSyntaxNodeAction(syntaxNodeContext =>
                    AnalyzeMethod(syntaxNodeContext, nunit), SyntaxKind.MethodDeclaration);
            });
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, NUnitFramework.Symbols nunit)
        {
            var methodSyntax = (MethodDeclarationSyntax) context.Node;
            
            if (ExceptionExpectancyMethodModel.TryFindDiagnostic(methodSyntax, context.SemanticModel, nunit, 
                out Diagnostic diagnostic))
            {
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}