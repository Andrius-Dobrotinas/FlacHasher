using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Hashfile.Read
{
    public class HashEntryParserTests
    {
        [TestCase(null)]
        [TestCase("")]
        public void Constructor__Must_reject_an_Empty_Separator(string separator)
        {
            Assert.Throws<ArgumentNullException>(
                () => new HashEntryParser(separator));
        }

        [TestCase("\r")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        public void Constructor__Must_reject_Newline_Separator_values(string separator)
        {
            Assert.Throws<ArgumentException>(
                () => new HashEntryParser(separator));
        }

        [TestCase(" ")]
        [TestCase("     ")]
        [TestCase("\t")]
        public void Constructor__Accept_whitespace_Separators(string separator)
        {
            Assert.DoesNotThrow(
                () => new HashEntryParser(separator));
        }

        [TestCase("file:hash", ":", "file", "hash")]
        [TestCase("On.e:Tw-o", ":", "On.e", "Tw-o")]
        [TestCase("file1-hash1", "-", "file1", "hash1")]
        [TestCase("file:1-hash1", "-", "file:1", "hash1", Description = "A char that could possibly be used as a separator in another life is in the first value")]
        [TestCase("file2 hash2", " ", "file2", "hash2")]
        [TestCase("file3\thash3", "\t", "file3", "hash3")]
        [TestCase("file 3.1\thash 34", "\t", "file 3.1", "hash 34", Description = "Both values contain spaces")]
        [TestCase("file1>@l;'s]\"-hash1", "-", "file1>@l;'s]\"", "hash1", Description = "Both calues contain all sorts of weird special characters, some of which could pass as a separator in another life")]
        [TestCase("crazy;file,name ; X2 \t@ #!45)(*&^%$#@.exe:hash|Y2-1234", ":", "crazy;file,name ; X2 \t@ #!45)(*&^%$#@.exe", "hash|Y2-1234", Description = "Both calues contain all sorts of weird special characters, some of which could pass as a separator in another life")]
        public void When_Line_has_values_separated_by_a_SingleChar_Separator__Must_return_the_First_segment_as_the_Key_and_the_Second_one_as_the_Value(string line, string separator, string expectedKey, string expectedValue)
        {
            var result = new HashEntryParser(separator).Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase("file: hash", ": ", "file", "hash")]
        [TestCase("fileX=>hashY", "=>", "fileX", "hashY")]
        [TestCase("file1 => hash1", " => ", "file1", "hash1")]
        [TestCase("file2\t-\thash2", "\t-\t", "file2", "hash2")]
        [TestCase("file3    hash3", "    ", "file3", "hash3")]
        [TestCase("file3 .    hash3", "    ", "file3 .", "hash3")]
        [TestCase("file4 Hash:hash4", " Hash:", "file4", "hash4")]
        [TestCase("file name X=>hash Y", "=>", "file name X", "hash Y")]
        [TestCase("file name \tX2 => hash Y2", " => ", "file name \tX2", "hash Y2", Description = "Both calues contain all sorts of weird special characters, some of which could pass as a separator in another life")]
        [TestCase("file:\\name, X2 ; @ #!45)(*&^%$#@.exe=>hash:Y2-1234", "=>", "file:\\name, X2 ; @ #!45)(*&^%$#@.exe", "hash:Y2-1234", Description = "Both calues contain all sorts of weird special characters, some of which could pass as a separator in another life")]
        public void When_Line_has_values_separated_by_a_MultiChar_Separator__Must_return_the_First_segment_as_the_Key_and_the_Second_one_as_the_Value(string line, string separator, string expectedKey, string expectedValue)
        {
            var result = new HashEntryParser(separator).Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase(":hash", null, "hash", Description = "Even when one of the segments is empty")]
        [TestCase("file:", "file", null, Description = "Even when one of the segments is empty")]
        [TestCase(":", null, null, Description = "Even when both segments are empty")]
        public void Must_return_Empty_segments_within_a_line_as_Null(string line, string expectedKey, string expectedValue)
        {
            var result = new HashEntryParser(":").Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase(" :asd", null, "asd")]
        [TestCase(":\t", null, null)]
        [TestCase("     : \t\t", null, null)]
        public void Must_return_Whitespace_segments_within_the_line_as_Null(string line, string expectedKey, string expectedValue)
        {
            var result = new HashEntryParser(":").Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase("file:hash:", "file:hash", null)]
        [TestCase("file:two:hash", "file:two", "hash")]
        [TestCase(":file:hash:", ":file:hash", null)]
        [TestCase("file::hash", "file:", "hash")]
        [TestCase("file:hash::", "file:hash:", null)]
        [TestCase("::hash", ":", "hash")]
        public void When_Line_contains_More_than_Two_segments__Must_return_LastOne_asValue_and_the_rest_asKey(string line, string expectedKey, string expectedValue)
        {
            var result = new HashEntryParser(":").Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCase("filehash", ":")]
        [TestCase("file hash", ":")]
        [TestCase("file hash", "\t")]
        [TestCase("file hash", "  ")]
        [TestCase("file= >hash", "=>")]
        public void When_Line_contains_No_Separator__Must_return_the_Whole_string_as_Value(string line, string separator)
        {
            var result = new HashEntryParser(separator).Parse(line);

            Assert.AreEqual(null, result.Key, "Key");
            Assert.AreEqual(line, result.Value, "Value");
        }

        [TestCase("", ":")]
        [TestCase("", " ")]
        [TestCase(" ", ":")]
        [TestCase(" ", " ")]
        [TestCase("\t\t", ":")]
        [TestCase("\t\t", " ")]
        [TestCase("\t\t", "\t")]
        [TestCase("\t\t", "\t\t")]
        public void When_Line_is_empty_or_whitespace__Must_throw_an_exception__Regardless_of_separator_value(string line, string separator)
        {
            Assert.Throws<ArgumentException>(
                () => new HashEntryParser(separator).Parse(line));
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
            var result = new HashEntryParser(":").Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }
    }
}