using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NUnit.Migrator.Helpers
{
    internal static class Formatting
    {
        public static SyntaxTrivia RecognizeEndOfLine(SyntaxNode node)
        {
            var endOfLine = node.DescendantTrivia().FirstOrDefault(t => t.IsKind(SyntaxKind.EndOfLineTrivia));

            return endOfLine != default(SyntaxTrivia) 
                ? endOfLine
                : SyntaxFactory.CarriageReturnLineFeed;
        }
    }
}