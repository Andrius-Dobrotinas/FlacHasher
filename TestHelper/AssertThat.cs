using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy
{
    public static class AssertThat
    {
        private static string DefaultToString<T>(T @object) => @object?.ToString();

        /// <summary>
        /// Verifies that the <paramref name="target"/> collection doesn't contain any elements from <paramref name="unexpected"/>.
        /// </summary>
        public static void DoesNotContainAny(IEnumerable<string> target, IEnumerable<string> unexpected, string reason = null)
        {
            var contains = target.Intersect(unexpected);
            if (contains.Any())
            {
                var reasonStr = reason != null
                    ? $"because {reason}"
                    : "";
                throw new AssertionException($"The collection shouldn't contain the following items {reasonStr}:\n{FormatAsJsArray(contains)}");
            }
        }

        /// <summary>
        /// Verifies that two collections have the same elements in the same order.
        /// Reports problems in more detail.
        /// </summary>
        public static void CollectionsMatchExactly<T>(IEnumerable<T> target, IEnumerable<T> expected, string testName = null)
        {
            CollectionsMatchExactly(target, expected, DefaultToString, testName);
        }

        /// <summary>
        /// Verifies that two collections have the same elements in the same order.
        /// Reports problems in more detail.
        /// </summary>
        public static void CollectionsMatchExactly<T>(IEnumerable<T> target, IEnumerable<T> expected, Func<T, string> toString, string testName = null)
        {
            var targetEnumerated = target.ToList();
            var expectedEnumerated = expected.ToList();

            testName = testName == null ? "" : $"{testName}: ";

            if (!expectedEnumerated.Any())
                Assert.IsEmpty(targetEnumerated, $"{testName}The collection is expected to be empty");

            for (var i = 0; i < expectedEnumerated.Count; i++)
            {
                // If the target collection is smaller than expected
                if (targetEnumerated.Count - 1 < i)
                    throw new AssertionException($"{testName}The collection is missing items: {FormatAsJsArray(expectedEnumerated.Skip(i), toString)}\n" +
                        $"{GetExpectedReceivedString(targetEnumerated, expectedEnumerated, toString)}");

                Assert.AreEqual(expectedEnumerated[i], targetEnumerated[i], @$"{testName}Expected element ""{toString(expectedEnumerated[i])}"" at position: {i} but found ""{toString(targetEnumerated[i])}""" + "\n" +
                                            $"{GetExpectedReceivedString(targetEnumerated, expectedEnumerated, toString)}");
            }

            // If the target collection is larger than expected
            if (targetEnumerated.Count > expectedEnumerated.Count)
                throw new AssertionException($"{testName}The collection contains extra elements: {FormatAsJsArray(targetEnumerated.Skip(expectedEnumerated.Count), toString)}\n" +
                                            $"{GetExpectedReceivedString(targetEnumerated, expectedEnumerated, toString)}");
        }

        private static string FormatAsJsArray<T>(IEnumerable<T> elements, Func<T, string> toString)
        {
            return FormatAsJsArray(elements.Select(toString));
        }

        private static string FormatAsJsArray(IEnumerable<string> elements)
        {
            return $"[\n{string.Join(",\n", elements.Select(x => "\t\"" + x + "\""))}\n]";
        }

        private static string GetExpectedReceivedString<T>(IEnumerable<T> target, IEnumerable<T> expected, Func<T, string> toString)
        {
            return $"Expected: {FormatAsJsArray(expected, toString)}\n" +
                    $"Received: {FormatAsJsArray(target, toString)}";
        }
    }
}
