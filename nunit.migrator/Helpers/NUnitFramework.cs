using Microsoft.CodeAnalysis;

#pragma warning disable S1144 // Unused private types or members should be removed

namespace NUnit.Migrator.Helpers
{
    internal static class NUnitFramework
    {
        public const string TestCaseAttributeSimpleName = "TestCase";
        public const string ExpectedExceptionSimpleName = "ExpectedException";
        public const string AssertIdentifier = "Assert";
        public const string IsIdentifier = "Is";

        private const string ExpectedExceptionAttributeQualifiedName = "NUnit.Framework.ExpectedExceptionAttribute";
        private const string TestCaseAttributeQualifiedName = "NUnit.Framework.TestCaseAttribute";

        internal static class Assert
        {
            public const string ThrowsIdentifier = "Throws";
            public const string ThatIdentifier = "That";
        }

        internal static class ExpectedExceptionArgument
        {
            public const string ExpectedException = "ExpectedException";
            public const string ExpectedExceptionName = "ExpectedExceptionName";
            public const string ExpectedMessage = "ExpectedMessage";
            public const string MatchType = "MatchType";
            public const string UserMessage = "UserMessage";
        }

        internal static class Is
        {
            public const string EqualTo = "EqualTo";
            public const string StringContaining = "StringContaining";
            public const string StringMatching = "StringMatching";
        }

        internal static class MessageMatch
        {
            public const string Exact = "Exact";
            public const string Contains = "Contains";
            public const string Regex = "Regex";
        }

        internal class Symbols
        {
            public INamedTypeSymbol ExpectedException { get; private set; }

            public INamedTypeSymbol TestCase { get; private set; }

            private bool ArePresent => ExpectedException != null && TestCase != null;

            internal static bool TryGetNUnitSymbols(Compilation compilation, out Symbols nunit)
            {
                nunit = new Symbols
                {
                    ExpectedException = compilation.GetTypeByMetadataName(ExpectedExceptionAttributeQualifiedName),
                    TestCase = compilation.GetTypeByMetadataName(TestCaseAttributeQualifiedName)
                };

                return nunit.ArePresent;
            }
        }
    }

}

#pragma warning restore S1144 // Unused private types or members should be removed