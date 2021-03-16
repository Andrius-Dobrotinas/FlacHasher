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
        public void When_line_has_data_separated_by_Colon__Must_return_two_segments(string line, string expectedSegment1, string expectedSegment2)
        {
            var segments = target.ParseLine(line, 0);

            Assert.AreEqual(2, segments.Length, "The number of segments");
            Assert.AreEqual(expectedSegment1, segments[0], "Segment 1");
            Assert.AreEqual(expectedSegment2, segments[1], "Segment 2");
        }

        [TestCase(":hash", null, "hash", Description = "Even when one of the segments is empty")]
        [TestCase("file:", "file", null, Description = "Even when one of the segments is empty")]
        [TestCase(":", null, null, Description = "Even when both segments are empty")]
        public void When_either_segment_is_empty__Must_return_two_segments__With_the_empty_one_as_null(string line, string expectedSegment1, string expectedSegment2)
        {
            var segments = target.ParseLine(line, 0);

            Assert.AreEqual(2, segments.Length, "The number of segments");
            Assert.AreEqual(expectedSegment1, segments[0], "Segment 1");
            Assert.AreEqual(expectedSegment2, segments[1], "Segment 2");
        }

        [TestCase(" :asd", null, "asd")]
        [TestCase(":\t", null, null)]
        [TestCase("     : \t\t", null, null)]
        public void When_either_segment_is_whitespace__Must_return_two_segments__With_the_whitespace_one_as_null(string line, string expectedSegment1, string expectedSegment2)
        {
            var segments = target.ParseLine(line, 0);

            Assert.AreEqual(2, segments.Length, "The number of segments");
            Assert.AreEqual(expectedSegment1, segments[0], "Segment 1");
            Assert.AreEqual(expectedSegment2, segments[1], "Segment 2");
        }

        [TestCase("file:hash:")]
        [TestCase(":file:hash:")]
        [TestCase("file::hash")]
        [TestCase("file:hash::")]
        public void When_line_contains_more_than_One_Colon__Must_throw_an_exception(string line)
        {
            Assert.Throws<Exception>(
                () => target.ParseLine(line, 0));
        }

        [TestCase("filehash")]
        [TestCase("file hash")]
        public void When_line_contains_No_Colon__Must_return_the_whole_value_as_the_Second_segment(string line)
        {
            var segments = target.ParseLine(line, 0);

            Assert.AreEqual(2, segments.Length, "The number of segments");
            Assert.AreEqual(null, segments[0], "Segment 1");
            Assert.AreEqual(line, segments[1], "Segment 2");
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\t")]
        public void When_line_is_empty__Must_throw_an_exception(string line)
        {
            Assert.Throws<ArgumentException>(
                () => target.ParseLine(line, 0));
        }

        [TestCase(" file:hash", "file", "hash")]
        [TestCase("file :hash", "file", "hash")]
        [TestCase("file: hash", "file", "hash")]
        [TestCase("file:hash ", "file", "hash")]
        [TestCase("\tfile   :\t\thash ", "file", "hash")]
        [TestCase("\tfil\te   :\t\thas   h ", "fil\te", "has   h")]
        [TestCase("\tfilehash ", null, "filehash")]
        [TestCase("\tfil e\thash ", null, "fil e\thash")]
        public void Must_remove_whitespace_from_the_start_and_end_of_each_segment(string line, string expectedSegment1, string expectedSegment2)
        {
            var segments = target.ParseLine(line, 0);

            Assert.AreEqual(expectedSegment1, segments[0], "Segment 1");
            Assert.AreEqual(expectedSegment2, segments[1], "Segment 2");
        }
    }
}