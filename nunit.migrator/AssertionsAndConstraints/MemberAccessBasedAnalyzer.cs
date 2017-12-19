using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.AssertionsAndConstraints
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemberAccessBasedAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ICompilationStartAnalyzing[] MigrationAnalyzers = {
            new AssertionMigration(), new ConstraintMigration()
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(
                MigrationAnalyzers
                    .Select(ma => ma.SupportedDiagnostic)
                    .ToArray());

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(ctx =>
            {
                if (!NUnitFramework.Symbols.TryGetNUnitSymbols(ctx.Compilation, out NUnitFramework.Symbols nunit))
                    return;

                foreach (var analyzer in MigrationAnalyzers)
                {
                    analyzer.RegisterAnalysis(ctx, nunit);
                }
            });
        }
    }
}