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

        public static readonly DiagnosticDescriptor Assertion = new DiagnosticDescriptor(
            id: "NU2M03",
            title: "Unsupported assertion (translatable to V3)",
            messageFormat: "'{0}' assertion should be replaced with '{1}'.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Some assertion forms have been replaced with a newer one (Assert.That) " +
                         "and are no longer supported. See: https://github.com/nunit/docs/wiki/Breaking-Changes, " +
                         "'Assertions and Constraints' section.");

        public static readonly DiagnosticDescriptor StaticSource = new DiagnosticDescriptor(
            id: "NU2M04",
            title: "Source attributes must refer only to static members.",
            messageFormat: "{0} attribute refers to '{1}' which is not static.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The TestCaseSource and ValueSource attributes must refer only to static fields, " +
                         "properties or methods.");

        public static readonly DiagnosticDescriptor NoLongerSupportedAttribute = new DiagnosticDescriptor(
            id: "NU2M05",
            title: "Attribute no longer supported.",
            messageFormat: "'{0}' attribute is no longer supported.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Various NUnit attributes are no longer supported. " +
                         "See: https://github.com/nunit/docs/wiki/Breaking-Changes, 'Attributes' section.");

        public static readonly DiagnosticDescriptor AttributeChangedSemantic = new DiagnosticDescriptor(
            id: "NU2M06",
            title: "Attribute no longer has the semantic it used to have in the previous version.",
            messageFormat: "'{0}' attribute is no longer treated as '{1}'.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Some attributes changed their meaning in the new version of the library. " +
                         "See: https://github.com/nunit/docs/wiki/Breaking-Changes, 'Attributes' section.");

        public static readonly DiagnosticDescriptor DeprecatedReplaceableAttribute = new DiagnosticDescriptor(
            id: "NU2M07",
            title: "Attribute is deprected but can be easily replaced with a proper one from the new API.",
            messageFormat: "'{0}' attribute is deprecated and should be replaced with '{1}'.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Some attributes have been deprecated and for some replacements exist. " +
                         "See: https://github.com/nunit/docs/wiki/Breaking-Changes, 'Attributes' section.");

        public static readonly DiagnosticDescriptor DeprecatedReplaceableAttributeArgument = new DiagnosticDescriptor(
            id: "NU2M08",
            title: "Attribute argument is deprected but can be easily replaced with a proper one from the new API.",
            messageFormat: "'{0}' attribute argument is deprecated and should be replaced with '{1}'.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Some attribute arguments have been deprecated and for some replacements exist. " +
                         "See: https://github.com/nunit/docs/wiki/Breaking-Changes, 'Attributes' section.");
    }
}