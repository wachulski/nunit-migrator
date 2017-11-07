using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.CodeActions
{
    internal class AssertHandlerMethodDecorator : AssertExceptionBlockDecorator
    {
        private readonly string _handlerName;

        public AssertHandlerMethodDecorator(IAssertExceptionBlockCreator blockCreator, string handlerName) 
            : base(blockCreator)
        {
            _handlerName = handlerName;
        }

        public override BlockSyntax Create(MethodDeclarationSyntax method, TypeSyntax assertedType)
        {
            var body = base.Create(method, assertedType);

            return body.AddStatements(CreateExceptionHandlerInvocationStatement());
        }

        private StatementSyntax CreateExceptionHandlerInvocationStatement()
        {
            var argumentToPass = AssignAssertThrowsToLocalVariableDecorator.LocalExceptionVariableName;

            return SyntaxFactory.ParseStatement($"{_handlerName}({argumentToPass});");
        }
    }
}