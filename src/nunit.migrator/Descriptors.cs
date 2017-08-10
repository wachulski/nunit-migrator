using Microsoft.CodeAnalysis;

namespace NUnit.Migrator
{
    internal class Descriptors
    {
        public static readonly DiagnosticDescriptor ExpectedException = new DiagnosticDescriptor(
            id: "NUnit2Migra001",
            title: "ExpectedException attribute conversion",
            messageFormat: "'ExpectedException' attribute is not supported in NUnit v3. Consider replacing with Assert.Throws<T>.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "'ExpectedException' should be replaced with Assert.Throws<T>.");
    }
}