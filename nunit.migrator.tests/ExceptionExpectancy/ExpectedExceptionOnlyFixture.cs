using NUnit.Framework;

namespace NUnit.Migrator.Tests.ExceptionExpectancy
{
    [TestFixture]
    public class ExpectedExceptionOnlyFixture : ExceptionExpectancyFixProviderTests
    {
        [Test]
        public void WhenNoExpectedExceptionPropertiesProvided_FixesToAssertThrowsOfSystemException_AndRemovesAttribute()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenStandaloneExpectedExceptionAttributeOnASeparateLine_RemovesTheWholeLineContainingAttribute()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    [ExpectedException]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [TestCase("(typeof(System.InvalidOperationException))")]
        [TestCase("(ExpectedException = typeof(System.InvalidOperationException))")]
        [TestCase("(\"System.InvalidOperationException\")")]
        [TestCase("(ExpectedExceptionName = \"System.InvalidOperationException\")")]
        public void WhenExceptionTypeNameFullyQualified_FixesToAssertThrowsOfTypeFullyQualified(string attrArguments)
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException" + attrArguments + @"]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<System.InvalidOperationException>(() =>
        {
            throw new System.InvalidOperationException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [TestCase("(typeof(ArgumentException))")]
        [TestCase("(ExpectedException = typeof(ArgumentException))")]
        [TestCase("(\"ArgumentException\")")]
        [TestCase("(ExpectedExceptionName = \"ArgumentException\")")]
        public void WhenExceptionTypeNameIsSimple_FixesToAssertThrowsOfSimpleTypeName(string attrArguments)
        {
            var source = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException" + attrArguments + @"]
    public void TestMethod()
    {
        throw new ArgumentException();
    }
}";
            var expected = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            throw new ArgumentException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [TestCase("(\"\")")]
        [TestCase("(ExpectedExceptionName = \"\")")]
        [TestCase("(ExpectedExceptionName = null)")]
        public void WhenExceptionTypeDefinedExplicitlyAsNullOrEmpty_FixesToAssertThrowsOfSystemException(
            string attrArguments)
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException" + attrArguments + @"]
    public void TestMethod()
    {
        throw new ArgumentException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<System.Exception>(() =>
        {
            throw new ArgumentException();
        });
    }
}";
            VerifyCSharpFix(source, expected);
        }


        [TestCase("ExpectedException = typeof(FirstEx), ExpectedExceptionName = \"LastEx\"")]
        [TestCase("ExpectedExceptionName = \"FirstEx\", ExpectedException = typeof(LastEx)")]
        public void WhenExceptionTypeNamesDefinedMultipleTimesWithNameEqualsSyntaxOnly_FixesToAssertThrowsOfLastDefinedType(
            string attrArguments)
        {
            var source = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException(" + attrArguments + @")]
    public void TestMethod()
    {
        throw new LastEx();
    }
}

public class FirstEx : Exception {}
public class MidEx : Exception {}
public class LastEx : Exception {}";

            var expected = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<LastEx>(() =>
        {
            throw new LastEx();
        });
    }
}

public class FirstEx : Exception {}
public class MidEx : Exception {}
public class LastEx : Exception {}";

            VerifyCSharpFix(source, expected);
        }

        [TestCase("typeof(FirstEx), ExpectedExceptionName = \"LastEx\"")]
        [TestCase("typeof(FirstEx), ExpectedException = typeof(LastEx)")]
        [TestCase("typeof(FirstEx), ExpectedException = typeof(MidEx), ExpectedExceptionName = \"LastEx\"")]
        [TestCase("typeof(FirstEx), ExpectedExceptionName = \"MidEx\", ExpectedException = typeof(LastEx)")]
        [TestCase("\"FirstEx\", ExpectedExceptionName = \"LastEx\"")]
        [TestCase("\"FirstEx\", ExpectedException = typeof(LastEx)")]
        [TestCase("\"FirstEx\", ExpectedException = typeof(MidEx), ExpectedExceptionName = \"LastEx\"")]
        [TestCase("\"FirstEx\", ExpectedExceptionName = \"MidEx\", ExpectedException = typeof(LastEx)")]
        public void WhenExceptionTypeNamesDefinedMultipleTimesWithDiverseSyntax_FixesToAssertThrowsOfFirstAttributeArg(
            string attrArguments)
        {
            var source = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException(" + attrArguments + @")]
    public void TestMethod()
    {
        throw new FirstEx();
    }
}

public class FirstEx : Excetion {}
public class MidEx : Exception {}
public class LastEx : Exception {}";

            var expected = @"
using System;
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<FirstEx>(() =>
        {
            throw new FirstEx();
        });
    }
}

public class FirstEx : Excetion {}
public class MidEx : Exception {}
public class LastEx : Exception {}";

            VerifyCSharpFix(source, expected);
        }

        [TestCase("MessageMatch.Exact", "Is.EqualTo")]
        [TestCase("MessageMatch.Contains", "Does.Contain")]
        [TestCase("MessageMatch.Regex", "Does.Match")]
        [TestCase("MessageMatch.StartsWith", "Does.StartWith")]
        public void WhenExceptionMessageMatchTypeSpecified_FixesToMessageStringSpecificCheck(string attributeArg,
            string expectedAssertionForm)
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException(ExpectedMessage = ""Invalid op message text."", MatchType=" + attributeArg + @")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException(""Invalid op message text."");
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        var ex = Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException(""Invalid op message text."");
        });
        Assert.That(ex.Message, " + expectedAssertionForm + @"(""Invalid op message text.""));
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }

        [TestCase("Simple Test With No Escaped Characters")]
        [TestCase("A Test with \\\"Double\\\" quotes escaped")] // A Test with \"Double\" quotes escaped
        [TestCase("A Test with \\'Single\\' Quotes Escaped")] // A Test with \'Single\' Quotes Escaped
        [TestCase("A Test with \\\\Slashes\\\\ Escaped")] // A Test with \\Slashes\\ Escaped
        public void WhenExceptionMessageSpecified_ContainsEscapedCharacters_FixesToAssertThrowsWithVariableAssignmentAndMessageExactCheck(string exceptionMessage)
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = " + $"\"{exceptionMessage}\"" + @")]
    public void TestMethod()
        {
            throw new Exception(" + $"\"{exceptionMessage}\"" + @");
        }
    }
";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        var ex = Assert.Throws<Exception>(() =>
        {
            throw new Exception(" + $"\"{exceptionMessage}\"" + @");
        });
        Assert.That(ex.Message, Is.EqualTo(" + $"\"{exceptionMessage}\"" + @"));
    }
}
";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenExceptionMessageSpecified_FixesToAssertThrowsWithVariableAssignmentAndMessageExactCheck()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException(ExpectedMessage = ""Invalid op message text."")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException(""Invalid op message text."");
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        var ex = Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException(""Invalid op message text."");
        });
        Assert.That(ex.Message, Is.EqualTo(""Invalid op message text.""));
    }
}";
            VerifyCSharpFix(source, expected, allowNewCompilerDiagnostics: true);
        }

        [TestCase("Simple Test With No Escaped Characters")]
        [TestCase("A Test with \\\"Double\\\" quotes escaped")] // A Test with \"Double\" quotes escaped
        [TestCase("A Test with \\'Single\\' Quotes Escaped")] // A Test with \'Single\' Quotes Escaped
        [TestCase("A Test with \\\\Slashes\\\\ Escaped")] // A Test with \\Slashes\\ Escaped
        public void WhenUserMessageSpecified_ContainsEscapedCharacters_FixesToAssertThrowsWithVariableAssignmentAndMessageExactCheck(string userMessage)
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    [Test]
    [ExpectedException(typeof(Exception), UserMessage = " + $"\"{userMessage}\"" + @")]
    public void TestMethod()
        {
            throw new Exception();
        }
    }
";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        }, " + $"\"{userMessage}\"" + @");
    }
}
";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenUserMessageSpecified_FixesToAssertThrowsWithCustomFailureMessageProvidedAsInAttributeArg()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException(UserMessage = ""This test failed."")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException();
        }, ""This test failed."");
    }
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenFixtureImplementsIExpectException_FixesToAssertThrowsWithHandlerMethodInvocation()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass : IExpectException
{
    [Test, ExpectedException]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }

    void HandleException(System.Exception exception) {}
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass : IExpectException
{
    [Test]
    public void TestMethod()
    {
        var ex = Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException();
        });
        HandleException(ex);
    }

    void HandleException(System.Exception exception) {}
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenTestSpecifiesHandlerName_FixesToAssertThrowsWithHandlerMethodInvocation()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException(Handler = ""MyExceptionHandler"")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }

    void MyExceptionHandler(System.Exception exception) {}
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        var ex = Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException();
        });
        MyExceptionHandler(ex);
    }

    void MyExceptionHandler(System.Exception exception) {}
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenTestSpecifiesHandlerNameAndFixtureImplementsIExpectException_FixesToAssertThrowsWithHandlerMethodInvocationOverrideFromTestAttribute()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass : IExpectException
{
    [Test, ExpectedException(Handler = ""MyExceptionHandler"")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }

    void HandleException(System.Exception exception) {}
    void MyExceptionHandler(System.Exception exception) {}
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass : IExpectException
{
    [Test]
    public void TestMethod()
    {
        var ex = Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException();
        });
        MyExceptionHandler(ex);
    }

    void HandleException(System.Exception exception) {}
    void MyExceptionHandler(System.Exception exception) {}
}";
            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenBothHandlerNameAndExpectedMessageSpecified_FixesToAssertThrowsWithMessageAssertCheckFirstAndThenHandlerInvocation()
        {
            var source = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test, ExpectedException(Handler = ""MyExceptionHandler"", ExpectedMessage=""Msg!"")]
    public void TestMethod()
    {
        throw new System.InvalidOperationException();
    }

    void MyExceptionHandler(System.Exception exception) {}
}";
            var expected = @"
using NUnit.Framework;

[TestFixture]
public class TestClass
{
    [Test]
    public void TestMethod()
    {
        var ex = Assert.Throws<System.Exception>(() =>
        {
            throw new System.InvalidOperationException();
        });
        Assert.That(ex.Message, Is.EqualTo(""Msg!""));
        MyExceptionHandler(ex);
    }

    void MyExceptionHandler(System.Exception exception) {}
}";
            VerifyCSharpFix(source, expected);
        }
    }
}