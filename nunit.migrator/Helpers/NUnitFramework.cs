using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace NUnit.Migrator.Helpers
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
#pragma warning disable S1144 // Unused private types or members should be removed
    internal static class NUnitFramework
    {
        public const string TestCaseAttributeSimpleName = "TestCase";
        public const string ExpectedExceptionSimpleName = "ExpectedException";
        public const string AssertIdentifier = "Assert";
        public const string DoesIdentifier = "Does";
        public const string IsIdentifier = "Is";

        private const string ExpectedExceptionAttributeQualifiedName = "NUnit.Framework.ExpectedExceptionAttribute";
        private const string TestCaseAttributeQualifiedName = "NUnit.Framework.TestCaseAttribute";
        private const string TextConstraintsQualifiedName = "NUnit.Framework.Text";
        private const string IsConstraintsQualifiedName = "NUnit.Framework.Is";
        private const string AssertQualifiedName = "NUnit.Framework.Assert";
        private const string TestCaseSourceQualifiedName = "NUnit.Framework.TestCaseSourceAttribute";
        private const string ValueSourceQualifiedName = "NUnit.Framework.ValueSourceAttribute";
        private const string SuiteAttributeQualifiedName = "NUnit.Framework.SuiteAttribute";
        private const string RequiredAddinAttributeQualifiedName = "NUnit.Framework.RequiredAddinAttribute";

        internal static class Assert
        {
            public const string ThrowsIdentifier = "Throws";
            public const string ThatIdentifier = "That";
        }

        internal static class Does
        {
            public const string StartWith = "StartWith";
            public const string Contain = "Contain";
            public const string Match = "Match";
        }

        internal static class ExpectedExceptionArgument
        {
            public const string ExpectedException = "ExpectedException";
            public const string ExpectedExceptionName = "ExpectedExceptionName";
            public const string ExpectedMessage = "ExpectedMessage";
            public const string Handler = "Handler";
            public const string MatchType = "MatchType";
            public const string UserMessage = "UserMessage";
        }

        internal static class Is
        {
            public const string EqualTo = "EqualTo";
        }

        internal static class MessageMatch
        {
            public const string Exact = "Exact";
            public const string Contains = "Contains";
            public const string Regex = "Regex";
            public const string StartsWith = "StartsWith";
        }

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        internal class Symbols
        {
            public INamedTypeSymbol ExpectedException { get; private set; }

            public INamedTypeSymbol TestCase { get; private set; }

            public INamedTypeSymbol Text { get; private set; }

            public INamedTypeSymbol Is { get; private set; }

            public INamedTypeSymbol Assert { get; private set; }

            public INamedTypeSymbol TestCaseSource { get; private set; }

            public INamedTypeSymbol ValueSource { get; private set; }

            public INamedTypeSymbol Suite { get; private set; }

            public INamedTypeSymbol RequiredAddin { get; private set; }

            private bool ArePresent =>
                ExpectedException != null
                && TestCase != null
                && Text != null
                && Is != null
                && Assert != null
                && TestCaseSource != null
                && ValueSource != null
                && Suite != null
                && RequiredAddin != null;

            internal static bool TryGetNUnitSymbols(Compilation compilation, out Symbols nunit)
            {
                nunit = new Symbols
                {
                    ExpectedException = compilation.GetTypeByMetadataName(ExpectedExceptionAttributeQualifiedName),
                    TestCase = compilation.GetTypeByMetadataName(TestCaseAttributeQualifiedName),
                    Text = compilation.GetTypeByMetadataName(TextConstraintsQualifiedName),
                    Is = compilation.GetTypeByMetadataName(IsConstraintsQualifiedName),
                    Assert = compilation.GetTypeByMetadataName(AssertQualifiedName),
                    TestCaseSource = compilation.GetTypeByMetadataName(TestCaseSourceQualifiedName),
                    ValueSource = compilation.GetTypeByMetadataName(ValueSourceQualifiedName),
                    Suite = compilation.GetTypeByMetadataName(SuiteAttributeQualifiedName),
                    RequiredAddin = compilation.GetTypeByMetadataName(RequiredAddinAttributeQualifiedName),
                };

                return nunit.ArePresent;
            }
        }
    }
#pragma warning restore S1144 // Unused private types or members should be removed
}