using Microsoft.CodeAnalysis;

namespace NUnit.Migrator
{
    internal static class Descriptors
    {
        public static readonly DiagnosticDescriptor ExceptionExpectancy = new DiagnosticDescriptor(
            id: "NUnit2Migra001",
            title: "Exception expectancy at attribute level",
            messageFormat: "Method '{0}' contains 'ExpectedException' attribute and/or " +
                           "'TestCase' exception related arguments which should be replaced with Assert.Throws<T>.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Neither 'ExpectedException' attribute nor 'TestCase' exception related arguments are " +
                         "supported in NUnit v3 any longer. Consider replacing them with appropriate Assert.Throws<T>" +
                         "constructs.");
    }
}