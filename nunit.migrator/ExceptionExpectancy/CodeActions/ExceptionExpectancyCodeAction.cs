using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using NUnit.Migrator.ExceptionExpectancy.Model;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.ExceptionExpectancy.CodeActions
{
    /// <summary>
    /// Removes exception related attributes and/or attribute arguments and produces equivalent test (case) method(s)
    /// asserting on exceptions being thrown.
    /// </summary>
    internal class ExceptionExpectancyCodeAction : CodeAction
    {
        private readonly ExceptionExpectancyMethodModel _model;
        private readonly Document _document;
        private readonly MethodDeclarationSyntax _method;
        private readonly TestCaseExceptionEquivalenceCluster[] _clusters;
        private readonly SyntaxTriviaList _methodLineSeparator;

        public ExceptionExpectancyCodeAction(Document document, MethodDeclarationSyntax method, 
            SemanticModel semanticModel, NUnitFramework.Symbols nunit)
        {
            _document = document;
            _method = method;
            _model = new ExceptionExpectancyMethodModel(method, semanticModel, nunit);
            _clusters = TestCaseExceptionEquivalenceCluster.CreateMany(_model);
            _methodLineSeparator = GetMethodLineSeparator(method);
            
            Debug.Assert(_clusters.Length > 0, "_clusters.Length > 0");
        }

        public override string EquivalenceKey => "ExceptionExpectancyCodeActionKey";

        public override string Title => 
            Texts.CodeActionTitle("Replace body with 'Assert.Throws<T>' and remove exception related attribute(s)");

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var root = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var fixedTestMethods = ProduceFixedTestMethods();
            root = root.ReplaceNode(_method, fixedTestMethods);

            return _document.WithSyntaxRoot(root);
        }

        private IEnumerable<MethodDeclarationSyntax> ProduceFixedTestMethods()
        {
            var fixedMethods = new List<MethodDeclarationSyntax>();
            var testMethodNamer = new TestMethodNamer(_method, _model.ExceptionFreeTestCaseAttributeNodes.Any());

            if (TryProduceExceptionUnrelatedTestMethod(out MethodDeclarationSyntax fixedExceptionUnrelatedMethod))
            {
                fixedMethods.Add(fixedExceptionUnrelatedMethod);
            }

            foreach (var cluster in _clusters)
            {
                fixedMethods.Add(ProduceTestMethodForExceptionCluster(cluster, _clusters.Length, testMethodNamer));
            }

            return AddTriviaToFixedMethods(fixedMethods);
        }

        private MethodDeclarationSyntax ProduceTestMethodForExceptionCluster(
            TestCaseExceptionEquivalenceCluster cluster, int clustersCount, TestMethodNamer testMethodNamer)
        {
            var testCasesToRemain = cluster.EquivalentItems.Select(i => i.AttributeNode).ToArray();
            var exceptionExpectancy = cluster.EquivalentItems.First();

            var clusterMethod = _method.WithoutExceptionExpectancyInAttributes(testCasesToRemain)
                .WithBody(CreateAssertedBlock(exceptionExpectancy))
                .WithIdentifier(testMethodNamer.CreateName(exceptionExpectancy, clustersCount))
                .WithoutTrivia();

            return clusterMethod;
        }

        private BlockSyntax CreateAssertedBlock(ExceptionExpectancyAtAttributeLevel exceptionExpectancy)
        {
            return exceptionExpectancy.GetAssertExceptionBlockCreator()
                .Create(_method, exceptionExpectancy.AssertedExceptionType).WithAdditionalAnnotations(Formatter.Annotation);
        }

        private bool TryProduceExceptionUnrelatedTestMethod(out MethodDeclarationSyntax fixedExceptionUnrelatedMethod)
        {
            if (!_model.ExceptionFreeTestCaseAttributeNodes.Any())
            {
                fixedExceptionUnrelatedMethod = null;
                return false;
            }

            fixedExceptionUnrelatedMethod = _method
                .WithoutExceptionExpectancyInAttributes(_model.ExceptionFreeTestCaseAttributeNodes)
                .WithoutTrivia();

            return true;
        }

        private IEnumerable<MethodDeclarationSyntax> AddTriviaToFixedMethods(List<MethodDeclarationSyntax> fixedMethods)
        {
            if (fixedMethods.Count == 1)
            {
                return fixedMethods.Select(m => m.WithTriviaFrom(_method));
            }

            var first = fixedMethods.First().WithLeadingTrivia(_method.GetLeadingTrivia());
            var allButFirst = fixedMethods.Skip(1).Select(ProduceMethodWithAdjustedLeadingTrivia).ToList();
            var middleLength = allButFirst.Count - 1; // skipping the last element
            var last = allButFirst.Last().WithTrailingTrivia(_method.GetTrailingTrivia());

            return new[]{first}.Concat(allButFirst.Take(middleLength)).Concat(new[]{last});

            MethodDeclarationSyntax ProduceMethodWithAdjustedLeadingTrivia(MethodDeclarationSyntax m)
            {
                var trivia = SyntaxFactory.TriviaList(_methodLineSeparator);
                var indentation = _method.GetLeadingTrivia().LastOrDefault(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
                if (indentation != default(SyntaxTrivia))
                {
                    trivia = trivia.Add(indentation);
                }
                return m.WithLeadingTrivia(trivia);
            }
        }

        private static SyntaxTriviaList GetMethodLineSeparator(SyntaxNode methodNode)
        {
            var endOfLine = Formatting.RecognizeEndOfLine(methodNode);
            var lineBreak = new[] {endOfLine, endOfLine};
            return SyntaxFactory.TriviaList(lineBreak);
        }

        private class TestMethodNamer
        {
            private readonly MethodDeclarationSyntax _originalTestMethod;
            private readonly bool _doExceptionUnrelatedTCsExist;
            private readonly Dictionary<string, int> _methodNamesCount;

            public TestMethodNamer(MethodDeclarationSyntax originalTestMethod, bool doExceptionUnrelatedTCsExist)
            {
                _originalTestMethod = originalTestMethod;
                _doExceptionUnrelatedTCsExist = doExceptionUnrelatedTCsExist;
                _methodNamesCount = new Dictionary<string, int>();
            }

            public SyntaxToken CreateName(ExceptionExpectancyAtAttributeLevel attribute, int clustersCount)
            {
                return clustersCount > 1 ||
                       clustersCount == 1 && _doExceptionUnrelatedTCsExist
                    ? CreateClusterTestMethodName(attribute)
                    : _originalTestMethod.Identifier;
            }

            private SyntaxToken CreateClusterTestMethodName(ExceptionExpectancyAtAttributeLevel attribute)
            {
                var exceptionTypeName = attribute.AssertedExceptionType.ToString().Split('.').Last();
                var proposedName = $"{_originalTestMethod.Identifier}_ShouldThrow{exceptionTypeName}";
                _methodNamesCount.TryGetValue(proposedName, out int actualMethodCount);
                _methodNamesCount[proposedName] = ++actualMethodCount;

                return actualMethodCount == 1
                    ? SyntaxFactory.ParseToken(proposedName)
                    : SyntaxFactory.ParseToken(proposedName + actualMethodCount);
            }
        }
    }
}