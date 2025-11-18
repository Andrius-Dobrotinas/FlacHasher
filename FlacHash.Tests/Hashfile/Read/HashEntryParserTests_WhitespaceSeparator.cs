using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Hashfile.Read
{
    public class HashEntryParserTests_WhitespaceSeparator
    {
        [TestCaseSource(nameof(GetCases_Whitespaces))]
        public void When__Line_has_values_separated_by_Arbitrary_Whitespace__Must_return_the_First_segment_as_the_Key_and_the_Second_one_as_the_Value(string whitespace, bool keyQuoted, bool valueQuoted)
        {
            var key = "ichi";
            var value = "ni";
            var keyValue = keyQuoted ? @$"""{key}""" : key;
            var valueValue = valueQuoted ? @$"""{value}""" : value;
            var line = $"{keyValue}{whitespace}{valueValue}";

            var result = new HashEntryParser(" ").Parse(line);

            Assert.AreEqual(key, result.Key, "Key");
            Assert.AreEqual(value, result.Value, "Value");
        }

        [TestCaseSource(nameof(GetCases_Whitespace_NoLinesWhereBothSegmentsAreUnquoted))]
        public void When__Line_has_values_separated_by_Arbitrary_Whitespace__Must_treat_whitespace_within_Quotes_as_part_of_segment_value(string whitespace, bool keyQuoted, bool valueQuoted)
        {
            var keyValue = keyQuoted ? @$"""ichi ni""" : "ichini";
            var valueValue = valueQuoted ? @$"""san\tgo roku""" : "sangoroku";
            var line = $"{keyValue}{whitespace}{valueValue}";

            var result = new HashEntryParser(" ").Parse(line);

            Assert.AreEqual(keyValue.Trim('\"'), result.Key, "Key");
            Assert.AreEqual(valueValue.Trim('\"'), result.Value, "Value");
        }

        [TestCase("segment \"\"", "segment", null)]
        [TestCase("segment \" \"", "segment", null)]
        [TestCase("segment \"   \"", "segment", null)]
        [TestCase("segment\t\"\"", "segment", null)]
        [TestCase("\" \" segment", null, "segment")]
        [TestCase("\"  \" segment", null, "segment")]
        [TestCase("\"   \" segment", null, "segment")]
        [TestCase("\" \"\tsegment", null, "segment")]
        public void When_Segments_are_wrapped_in_Quotes__Must_Throw_an_exception(string line, string expectedKey, string expectedValue)
        {
            Assert.Throws<FormatException>(
                () => new HashEntryParser(" ").Parse(line));
        }

        [TestCase("filehash")]
        [TestCase("file:hash")]
        [TestCase("file=hash")]
        [TestCase(" filehash")]
        [TestCase("     filehash")]
        [TestCase("     filehash    ")]
        public void When_Line_contains_No_Separator__Must_return_the_Whole_string_as_Value__Trimmed(string line)
        {
            var result = new HashEntryParser(" ").Parse(line);

            Assert.AreEqual(null, result.Key, "Key");
            Assert.AreEqual(line.Trim(), result.Value, "Value");
        }

        [TestCase("\"file=>hash\"", "file=>hash")]
        [TestCase(" \"file=>hash\"", "file=>hash")]
        [TestCase("     \"file=>hash\"", "file=>hash")]
        [TestCase("\"file=>hash\" ", "file=>hash")]
        [TestCase("\"file=>hash\"   ", "file=>hash")]
        public void When_Line_contains_No_Separator_and_is_wrapped_in_Quotes__Must_return_the_Whole_string_sans_quotes_as_Value(string line, string expected)
        {
            var result = new HashEntryParser(" ").Parse(line);

            Assert.AreEqual(null, result.Key, "Key");
            Assert.AreEqual(expected, result.Value, "Value");
        }

        [TestCase("file two hash", "file", "two")]
        [TestCase("file\ttwo    hash", "file", "two")]
        [TestCase("haha what! hash four", "haha", "what!")]
        [TestCase("\"one\" \"two\" \"three\"", "one", "two")]
        [TestCase("\"one\"\t\"two\"     \"three\"", "one", "two")]
        [TestCase("\"ein\" \"zwei\" \"drei\" \"four\"", "ein", "zwei")]
        public void When_Line_contains_More_than_Two_segments__Must_return_FirstTwo_as_Key_and_Value__Dropping_the_rest(string line, string expectedKey, string expectedValue)
        {
            var result = new HashEntryParser(" ").Parse(line);

            Assert.AreEqual(expectedKey, result.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value, "Value");
        }

        [TestCaseSource(nameof(GetCases_Whitespaces))]
        public void When__Line_Starts_with_Whitespace__Must_ignore_the_whitespace(string whitespace, bool keyQuoted, bool valueQuoted)
        {
            var key = "ichi";
            var value = "ni";
            var keyValue = keyQuoted ? @$"""{key}""" : key;
            var valueValue = valueQuoted ? @$"""{value}""" : value;

            var line = $"{whitespace}{keyValue}{whitespace}{valueValue}";

            var result = new HashEntryParser(" ").Parse(line);

            Assert.AreEqual(key, result.Key, "Key");
            Assert.AreEqual(value, result.Value, "Value");
        }

        [TestCaseSource(nameof(GetCases_Whitespaces))]
        public void When__Line_Ends_with_Whitespace__Must_ignore_the_whitespace(string whitespace, bool keyQuoted, bool valueQuoted)
        {
            var key = keyQuoted ? @"""ichi""" : "ichi";
            var value = valueQuoted ? @"""ni""" : "ni";
            var line = $"{key}{whitespace}{value}{whitespace}";
            var result = new HashEntryParser(" ").Parse(line);

            Assert.AreEqual("ichi", result.Key, "Key");
            Assert.AreEqual("ni", result.Value, "Value");
        }

        [TestCaseSource(nameof(GetCases_Whitespaces))]
        public void When__Line_Starts_and_Ends_with_Whitespace__Must_ignore_the_whitespace(string whitespace, bool keyQuoted, bool valueQuoted)
        {
            var key = "ichi";
            var value = "ni";
            var keyValue = keyQuoted ? @$"""{key}""" : key;
            var valueValue = valueQuoted ? @$"""{value}""" : value;
            var line = $"{whitespace}{keyValue}{whitespace}{valueValue}{whitespace}";

            var result = new HashEntryParser(" ").Parse(line);

            Assert.AreEqual(key, result.Key, "Key");
            Assert.AreEqual(value, result.Value, "Value");
        }

        static IEnumerable<TestCaseData> GetCases_Whitespaces()
        {
            yield return new TestCaseData(" ", false, false);
            yield return new TestCaseData(" ", true, false);
            yield return new TestCaseData(" ", true, true);
            yield return new TestCaseData(" ", false, true);
            yield return new TestCaseData("  ", false, false);
            yield return new TestCaseData("  ", true, false);
            yield return new TestCaseData("  ", true, true);
            yield return new TestCaseData("  ", false, true);
            yield return new TestCaseData("\t", false, false);
            yield return new TestCaseData("\t", true, false);
            yield return new TestCaseData("\t", true, true);
            yield return new TestCaseData("\t", false, true);
            yield return new TestCaseData(" \t", false, false);
            yield return new TestCaseData(" \t", true, false);
            yield return new TestCaseData(" \t", true, true);
            yield return new TestCaseData(" \t", false, true);
            yield return new TestCaseData(" \t ", false, false);
            yield return new TestCaseData(" \t ", true, false);
            yield return new TestCaseData(" \t ", true, true);
            yield return new TestCaseData(" \t ", false, true);
        }

        static IEnumerable<TestCaseData> GetCases_Whitespace_NoLinesWhereBothSegmentsAreUnquoted()
        {
            return GetCases_Whitespaces().Where(x =>
            {
                var keyQuoted = (bool)x.Arguments[1];
                var valQuoted = (bool)x.Arguments[2];
                return keyQuoted != false || valQuoted != false;
            });
        }
    }
}