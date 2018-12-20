using NUnit.Framework;

namespace NUnit.Migrator.Tests.ExceptionExpectancy
{
    public class TriviaFixture : ExceptionExpectancyFixProviderTests
    {
        [Test]
        public void WhenExpectedExceptionAttributeIsBelowTest_RegionIsPreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    #region Test
    [Test]
    [ExpectedException(typeof(Exception))]
    public void TestMethod()
    {
        throw new Exception();
    }
    #endregion
}";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    #region Test
    [Test]
    public void TestMethod()
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }
    #endregion
}";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenExpectedExceptionAttributeIsAboveTest_RegionIsPreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    #region Test
    [ExpectedException(typeof(Exception))]
    [Test]
    public void TestMethod()
    {
        throw new Exception();
    }
    #endregion
}";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    #region Test
    [Test]
    public void TestMethod()
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }
    #endregion
}";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenExpectedExceptionAttributeIsBelowTest_CommentsArePreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    [Test]
    [ExpectedException(typeof(Exception))]
    public void TestMethod()
    {
        throw new Exception();
    }
}";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    [Test]
    public void TestMethod()
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }
}";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenExpectedExceptionAttributeIsAboveTest_CommentsArePreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    [ExpectedException(typeof(Exception))]
    [Test]
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
    /// <summary>
    /// There was a comment here
    /// </summary>
    [Test]
    public void TestMethod()
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }
}
";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenExpectedExceptionAttributeIsBelowTest_CommentsAndDocumentationArePreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    /// <param name=""input"">Comment for input</param>
    /// <remarks>This was a remark</remarks>
    [Test]
    [ExpectedException(typeof(Exception))]
    public void TestMethod( string input )
    {
        throw new Exception();
    }
}";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    /// <param name=""input"">Comment for input</param>
    /// <remarks>This was a remark</remarks>
    [Test]
    public void TestMethod( string input )
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }
}";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void WhenExpectedExceptionAttributeIsAboveTest_CommentsAndDocumentationArePreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    /// <param name=""input"">Comment for input</param>
    /// <remarks>This was a remark</remarks>
    [ExpectedException(typeof(Exception))]
    [Test]
    public void TestMethod( string input )
    {
        throw new Exception();
    }
}";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    /// <param name=""input"">Comment for input</param>
    /// <remarks>This was a remark</remarks>
    [Test]
    public void TestMethod( string input )
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }
}";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void ForMultipleTests_WhenExpectedExceptionAttributeIsBelowTest_CommentsAndWhitespaceArePreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was another comment here
    /// </summary>
    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TestMethod2()
    {
        throw new InvalidOperationException();
    }

    /// <summary>
    /// There was a comment here
    /// </summary>
    [Test]
    [ExpectedException(typeof(Exception))]
    public void TestMethod()
    {
        throw new Exception();
    }
}";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was another comment here
    /// </summary>
    [Test]
    public void TestMethod2()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            throw new InvalidOperationException();
        });
    }

    /// <summary>
    /// There was a comment here
    /// </summary>
    [Test]
    public void TestMethod()
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }
}";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void ForMultipleTests_WhenExpectedExceptionAttributeIsAboveTest_CommentsAndWhitespaceArePreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was another comment here
    /// </summary>
    [ExpectedException(typeof(InvalidOperationException))]
    [Test]
    public void TestMethod2()
    {
        throw new InvalidOperationException();
    }

    /// <summary>
    /// There was a comment here
    /// </summary>
    [ExpectedException(typeof(Exception))]
    [Test]
    public void TestMethod()
    {
        throw new Exception();
    }
}";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was another comment here
    /// </summary>
    [Test]
    public void TestMethod2()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            throw new InvalidOperationException();
        });
    }

    /// <summary>
    /// There was a comment here
    /// </summary>
    [Test]
    public void TestMethod()
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }
}";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void For2TCs_WhenExpectedExceptionIsFirstTestCase_CommentsArePreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    /// <param name=""input"">Comment for input</param>
    /// <remarks>This was a remark</remarks>
    [TestCase(1, ExpectedException = typeof(Exception))]
    [TestCase(2)]
    public void TestMethod( int input )
    {
        throw new Exception();
    }
}";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    /// <param name=""input"">Comment for input</param>
    /// <remarks>This was a remark</remarks>
    [TestCase(1)]
    public void TestMethod( int input )
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }

    /// <summary>
    /// There was a comment here
    /// </summary>
    /// <param name=""input"">Comment for input</param>
    /// <remarks>This was a remark</remarks>
    [TestCase(2)]
    public void TestMethod( int input )
    {
        throw new Exception();
    }
}";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void For2TCs_WhenExpectedExceptionIsSecondTestCase_CommentsArePreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    /// <param name=""input"">Comment for input</param>
    /// <remarks>This was a remark</remarks>
    [TestCase(1)]
    [TestCase(2, ExpectedException = typeof(Exception))]
    public void TestMethod( int input )
    {
        throw new Exception();
    }
}";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    /// <summary>
    /// There was a comment here
    /// </summary>
    /// <param name=""input"">Comment for input</param>
    /// <remarks>This was a remark</remarks>
    [TestCase(1)]
    public void TestMethod( int input )
    {
        throw new Exception();
    }

    /// <summary>
    /// There was a comment here
    /// </summary>
    /// <param name=""input"">Comment for input</param>
    /// <remarks>This was a remark</remarks>
    [TestCase(2)]
    public void TestMethod( int input )
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }
}";

            VerifyCSharpFix(source, expected);
        }

        [Test]
        public void For2TCs_WhenExpectedException_RegionsArePreserved()
        {
            var source = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    #region TestRegion
    [TestCase(1)]
    [TestCase(2, ExpectedException = typeof(Exception))]
    public void TestMethod(int input)
    {
        throw new Exception();
    }
    #endregion
}";

            var expected = @"
using NUnit.Framework;
using System;

[TestFixture]
public class TestClass
{
    #region TestRegion
    [TestCase(1)]
    public void TestMethod(int input)
    {
        throw new Exception();
    }

    [TestCase(2)]
    public void TestMethod(int input)
    {
        Assert.Throws<Exception>(() =>
        {
            throw new Exception();
        });
    }
    #endregion
}";

            VerifyCSharpFix(source, expected);
        }
    }
}
