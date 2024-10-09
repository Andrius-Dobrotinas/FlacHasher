using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy
{
    public class AssertThat_CollectionsMatchExactly_Tests
    {
        [Test]
        public void When_Both_Collections_HaveTheSameElementsInTheSameOrder__Must_Pass()
        {
            var one = new string[] { "one", "two", "three" };
            var two = new string[] { "one", "two", "three" };

            Assert.DoesNotThrow(() => AssertThat.CollectionsMatchExactly(one, two));
        }

        [Test]
        public void When_Both_Collections_AreEmpty__Must_Pass()
        {
            var one = new string[0];
            var two = new string[0];

            Assert.DoesNotThrow(() => AssertThat.CollectionsMatchExactly(one, two));
        }

        [Test]
        public void When_Both_Collections_HaveTheSameElements_InDifferentOrders__Must_Fail()
        {
            var one = new string[] { "one", "two", "three" };
            var two = new string[] { "one", "three", "two" };

            Assert.Throws<AssertionException>(() => AssertThat.CollectionsMatchExactly(one, two));
        }

        [TestCaseSource(nameof(Get_NoMatch))]
        public void When_Collections_AreDifferent__Must_Fail(IEnumerable<string> one, IEnumerable<string> two)
        {
            Assert.Throws<AssertionException>(() => AssertThat.CollectionsMatchExactly(one, two));
        }

        [Test]
        public void When_CollectionsMatch_ButTheresExtraItems__Must_Fail()
        {
            var one = new string[] { "one", "two", "three" };
            var two = new string[] { "one", "two", "three", "four" };

            Assert.Throws<AssertionException>(() => AssertThat.CollectionsMatchExactly(one, two));
        }

        [Test]
        public void When_CollectionsDontMatch__Must_Fail()
        {
            var one = new string[] { "one", "two", "three" };
            var two = new string[] { "one", "two", "four" };

            Assert.Throws<AssertionException>(() => AssertThat.CollectionsMatchExactly(one, two));
        }

        private static IEnumerable<TestCaseData> Get_NoMatch()
        {
            yield return new TestCaseData(
                new string[] { "one"},
                new string[0]);

            yield return new TestCaseData(
                new string[0],
                new string[] { "one" });

            yield return new TestCaseData(
                new string[] { "one", "two", "three" },
                new string[] { "two", "three" });

            yield return new TestCaseData(
                new string[] { "two", "three" },
                new string[] { "one", "two", "three" });
        }
    }
}