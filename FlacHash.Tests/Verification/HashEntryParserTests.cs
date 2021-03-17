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
        public void When_line_has_data_separated_by_Colon__Must_return_the_first_one_as_the_Key_and_the_second_one_as_the_Value(string line, string expectedKey, string expectedValue)
        {
            var result = target.Parse(line, 0);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase(":hash", null, "hash", Description = "Even when one of the segments is empty")]
        [TestCase("file:", "file", null, Description = "Even when one of the segments is empty")]
        [TestCase(":", null, null, Description = "Even when both segments are empty")]
        public void When_either_segment_is_empty__Must_return_the_whitespace_one_as_null(string line, string expectedKey, string expectedValue)
        {
            var result = target.Parse(line, 0);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase(" :asd", null, "asd")]
        [TestCase(":\t", null, null)]
        [TestCase("     : \t\t", null, null)]
        public void When_either_segment_is_whitespace__Must_return_the_whitespace_one_as_null(string line, string expectedKey, string expectedValue)
        {
            var result = target.Parse(line, 0);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase("file:hash:")]
        [TestCase(":file:hash:")]
        [TestCase("file::hash")]
        [TestCase("file:hash::")]
        public void When_line_contains_more_than_One_Colon__Must_throw_an_exception(string line)
        {
            Assert.Throws<Exception>(
                () => target.Parse(line, 0));
        }

        [TestCase("filehash")]
        [TestCase("file hash")]
        public void When_line_contains_No_Colon__Must_return_the_whole_string_as_the_Valu(string line)
        {
            var result = target.Parse(line, 0);

            Assert.AreEqual(null, result.Key, "Key");
            Assert.AreEqual(line, result.Value, "Value");
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\t")]
        public void When_line_is_empty__Must_throw_an_exception(string line)
        {
            Assert.Throws<ArgumentException>(
                () => target.Parse(line, 0));
        }

        [TestCase(" file:hash", "file", "hash")]
        [TestCase("file :hash", "file", "hash")]
        [TestCase("file: hash", "file", "hash")]
        [TestCase("file:hash ", "file", "hash")]
        [TestCase("\tfile   :\t\thash ", "file", "hash")]
        [TestCase("\tfil\te   :\t\thas   h ", "fil\te", "has   h")]
        [TestCase("\tfilehash ", null, "filehash")]
        [TestCase("\tfil e\thash ", null, "fil e\thash")]
        public void Must_remove_whitespace_from_the_start_and_end_of_both_properties(string line, string expectedKey, string expectedValue)
        {
            var result = target.Parse(line, 0);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }
    }
}