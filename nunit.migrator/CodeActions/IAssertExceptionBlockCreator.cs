using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Migrator.CodeActions
{
    /// <summary>
    ///     Provides method body with assertion that it throws an exception of given type and/or asserts on its properties.
    /// </summary>
    internal interface IAssertExceptionBlockCreator
    {
        BlockSyntax Create(MethodDeclarationSyntax method, TypeSyntax assertedType);
    }
}