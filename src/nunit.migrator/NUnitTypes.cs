namespace NUnit.Migrator
{
    internal static class NUnitTypes
    {
        public const string ExpectedExceptionAttributeQualifiedName = "NUnit.Framework.ExpectedExceptionAttribute";

        public const string AssertIdentifier = "Assert";

        public const string IsIdentifier = "Is";

        internal static class Assert
        {
            public const string ThrowsIdentifier = "Throws";
            public const string ThatIdentifier = "That";
        }

        internal static class ExpectedExceptionArgument
        {
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
    }
}