using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace NUnit.Migrator.AssertionsAndConstraints
{
    internal interface IFixer
    {
        DiagnosticDescriptor FixableDiagnostic { get; }

        /// <summary>
        /// Plugs fixing if the given context is eligible for it.
        /// </summary>
        void RegisterFixing(CodeFixContext context, Document document, SyntaxNode documentRoot);
    }
}