using Microsoft.CodeAnalysis;

namespace NUnit.Migrator
{
    internal static class Descriptors
    {
        public static readonly DiagnosticDescriptor ExceptionExpectancy = new DiagnosticDescriptor(
            id: "NU2M01",
            title: "Exception expectancy at attribute level",
            messageFormat: "Method '{0}' contains 'ExpectedException' attribute and/or " +
                           "'TestCase' exception related arguments which should be replaced with Assert.Throws<T>.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Neither 'ExpectedException' attribute nor 'TestCase' exception related arguments are " +
                         "supported in NUnit v3 any longer. Consider replacing them with appropriate Assert.Throws<T>" +
                         "constructs.");

        public static readonly DiagnosticDescriptor Constraint = new DiagnosticDescriptor(
            id: "NU2M02",
            title: "Unsupported constraint (translatable to V3)",
            messageFormat: "'{0}' constraint should be replaced with '{1}'.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Various constraint forms have been replaced with newer ones (e.g. Is.* or Does.*) " +
                         "and are no longer supported. See: https://github.com/nunit/docs/wiki/Breaking-Changes, " +
                         "'Assertions and Constraints' section.");
    }
}