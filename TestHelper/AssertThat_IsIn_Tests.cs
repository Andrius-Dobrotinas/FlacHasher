using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy
{
    public class AssertThat_IsIn_Tests
    {
        [TestCase("one", new string[] { "one" })]
        [TestCase("one", new string[] { "one", "two", "three" })]
        [TestCase("one", new string[] { "two", "one", "three" })]
        [TestCase("one", new string[] { "two", "three", "one" })]
        [TestCase("one", new string[] { "one", "three", "one" })]
        [TestCase("one", new string[] { "one", "one", "one" })]
        [TestCase("Val Kilmer", new string[] { "Bruce Willis", "Val Kilmer", "Jim Morrison"})]
        [TestCase("", new string[] { "two", "" })]
        [TestCase(null, new string[] { "two", null, "one" })]
        public void True__When_ExpectedValue_Is_In_The_Collection__Case_Sensitive(string target, params string[] collection)
        {
            Assert.DoesNotThrow(() => AssertThat.IsIn(target, collection));
        }

        [TestCase("one", new string[0])]
        [TestCase("one", new string[] { "two" })]
        [TestCase("one", new string[] { "One" })]
        [TestCase("one", new string[] { "two", "three" })]
        [TestCase("one", new string[] { "two", "three", "onE" })]
        [TestCase("", new string[] { "two", "three", "onE" })]
        [TestCase(null, new string[] { "two", "three", "onE" })]
        [TestCase("", new string[0])]
        [TestCase(null, new string[0])]
        public void False__When_ExpectedValue_Is_Not_In_The_Collection__Case_Sensitive(string target, params string[] collection)
        {
            Assert.Throws<AssertionException>(() => AssertThat.IsIn(target, collection));
        }
    }
}