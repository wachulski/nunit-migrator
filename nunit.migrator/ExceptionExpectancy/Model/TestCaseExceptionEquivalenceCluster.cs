using System.Collections.Generic;
using System.Linq;
using NUnit.Migrator.Helpers;

namespace NUnit.Migrator.ExceptionExpectancy.Model
{
    internal class TestCaseExceptionEquivalenceCluster
    {
        public ExceptionExpectancyAtAttributeLevel[] EquivalentItems { get; }

        private TestCaseExceptionEquivalenceCluster(IEnumerable<ExceptionExpectancyAtAttributeLevel> equivalentItems)
        {
            EquivalentItems = equivalentItems.ToArray();
        }

        public static TestCaseExceptionEquivalenceCluster[] CreateMany(ExceptionExpectancyMethodModel model)
        {
            // degenerated case of a single ExpectedException for a test method
            if (model.ExceptionRelatedAttributes.Length == 1
                && model.ExceptionRelatedAttributes[0] is ExpectedExceptionAttribute)
            {
                return new[] 
                {
                    new TestCaseExceptionEquivalenceCluster(new[]
                    {
                        model.ExceptionRelatedAttributes[0]
                    })
                } ;
            }

            return model.ExceptionRelatedAttributes.OfType<TestCaseExpectingExceptionAttribute>()
                .GroupBy(tc => tc, Comparer.Instance)
                .Select(g => new TestCaseExceptionEquivalenceCluster(g))
                .ToArray();
        }

        private class Comparer : IEqualityComparer<ExceptionExpectancyAtAttributeLevel>
        {
            public static readonly Comparer Instance = new Comparer();

            public bool Equals(ExceptionExpectancyAtAttributeLevel x, ExceptionExpectancyAtAttributeLevel y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.AssertedExceptionType.ToString() == y.AssertedExceptionType.ToString()
                       && x.ExpectedMessage == y.ExpectedMessage
                       && EffectiveMatchType(x.MatchType) == EffectiveMatchType(y.MatchType);
            }

            public int GetHashCode(ExceptionExpectancyAtAttributeLevel obj)
            {
                return new
                {
                    typeString = obj.AssertedExceptionType.ToString(),
                    obj.ExpectedMessage,
                    matchType = EffectiveMatchType(obj.MatchType)
                }.GetHashCode();
            }

            private static string EffectiveMatchType(string matchType)
            {
                return matchType ?? NUnitFramework.MessageMatch.Exact;
            }
        }
    }
}