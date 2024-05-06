using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public class HashEntryParserTests
    {
        private HashEntryParser target = new HashEntryParser();

        [TestCase("file:hash", "file", "hash")]
        [TestCase("One:Two", "One", "Two")]
        public void When_line_has_data_separated_by_Colon__Must_return_the_First_one_as_the_Key_and_the_Second_one_as_the_Value(string line, string expectedKey, string expectedValue)
        {
            var result = target.Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase(":hash", null, "hash", Description = "Even when one of the segments is empty")]
        [TestCase("file:", "file", null, Description = "Even when one of the segments is empty")]
        [TestCase(":", null, null, Description = "Even when both segments are empty")]
        public void Must_return_Empty_segments_within_a_line_as_Null(string line, string expectedKey, string expectedValue)
        {
            var result = target.Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase(" :asd", null, "asd")]
        [TestCase(":\t", null, null)]
        [TestCase("     : \t\t", null, null)]
        public void Must_return_Whitespace_segments_within_a_line_as_Null(string line, string expectedKey, string expectedValue)
        {
            var result = target.Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase("file:hash:")]
        [TestCase(":file:hash:")]
        [TestCase("file::hash")]
        [TestCase("file:hash::")]
        public void When_line_contains_more_Two_segments__Must_throw_an_exception(string line)
        {
            Assert.Throws<Exception>(
                () => target.Parse(line));
        }

        [TestCase("filehash")]
        [TestCase("file hash")]
        public void When_line_contains_No_Separator__Must_return_the_Whole_string_as_the_Value(string line)
        {
            var result = target.Parse(line);

            Assert.AreEqual(null, result.Key, "Key");
            Assert.AreEqual(line, result.Value, "Value");
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\t")]
        public void When_line_is_empty_or_whitespace__Must_throw_an_exception(string line)
        {
            Assert.Throws<ArgumentException>(
                () => target.Parse(line));
        }

        [TestCase(" file:hash", "file", "hash")]
        [TestCase("file :hash", "file", "hash")]
        [TestCase("file: hash", "file", "hash")]
        [TestCase("file:hash ", "file", "hash")]
        [TestCase("\tfile   :\t\thash ", "file", "hash")]
        [TestCase("\tfil\te   :\t\thas   h ", "fil\te", "has   h")]
        [TestCase("\tfilehash ", null, "filehash")]
        [TestCase("\tfil e\thash ", null, "fil e\thash")]
        public void Must_remove_whitespace_from_the_start_and_end_of_both_segments(string line, string expectedKey, string expectedValue)
        {
            var result = target.Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }
    }
}