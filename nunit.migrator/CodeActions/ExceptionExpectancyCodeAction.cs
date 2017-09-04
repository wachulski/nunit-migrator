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
using NUnit.Migrator.Helpers;
using NUnit.Migrator.Model;

namespace NUnit.Migrator.CodeActions
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
        private readonly TestMethodNamer _testMethodNamer;

        public ExceptionExpectancyCodeAction(Document document, MethodDeclarationSyntax method, 
            SemanticModel semanticModel, NUnitFramework.Symbols nunit)
        {
            _document = document;
            _method = method;
            _model = new ExceptionExpectancyMethodModel(method, semanticModel, nunit);
            _clusters = TestCaseExceptionEquivalenceCluster.CreateMany(_model);
            _methodLineSeparator = GetMethodLineSeparator(method);
            _testMethodNamer = new TestMethodNamer(_method, _model.ExceptionFreeTestCaseAttributeNodes.Any());
            
            Debug.Assert(_clusters.Length > 0, "_clusters.Length > 0");
        }

        public override string EquivalenceKey => "ExceptionExpectancyCodeActionKey";

        public override string Title => "Replace body with 'Assert.Throws<T>' and remove exception related attribute(s)";

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
            
            if (TryProduceExceptionUnrelatedTestMethod(out MethodDeclarationSyntax fixedExceptionUnrelatedMethod))
            {
                fixedMethods.Add(fixedExceptionUnrelatedMethod);
            }

            foreach (var cluster in _clusters)
            {
                fixedMethods.Add(ProduceTestMethodForExceptionCluster(cluster, _clusters.Length));
            }

            return fixedMethods;
        }

        private MethodDeclarationSyntax ProduceTestMethodForExceptionCluster(
            TestCaseExceptionEquivalenceCluster cluster, int clustersCount)
        {
            var testCasesToRemain = cluster.EquivalentItems.Select(i => i.AttributeNode).ToArray();
            var exceptionExpectancy = cluster.EquivalentItems.First();

            var clusterMethod = _method
                .WithoutExceptionExpectancyInAttributes(testCasesToRemain)
                .WithBody(CreateAssertedBlock(exceptionExpectancy))
                .WithTrailingTrivia(CreateClusterMethodTrailingTrivia(cluster))
                .WithIdentifier(_testMethodNamer.CreateName(exceptionExpectancy, clustersCount));

            return clusterMethod;
        }

        private SyntaxTriviaList CreateClusterMethodTrailingTrivia(TestCaseExceptionEquivalenceCluster cluster)
        {
            return cluster == _clusters.Last()
                ? _method.GetTrailingTrivia()
                : _methodLineSeparator;
        }
        
        private BlockSyntax CreateAssertedBlock(ExceptionExpectancyAtAttributeLevel exceptionExpectancy)
        {
            return exceptionExpectancy.GetAssertExceptionBlockCreator().Create(_method,
                exceptionExpectancy.AssertedExceptionType).WithAdditionalAnnotations(Formatter.Annotation);
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
                .WithTrailingTrivia(_methodLineSeparator);

            return true;
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