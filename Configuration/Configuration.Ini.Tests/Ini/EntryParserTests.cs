using NUnit.Framework;

namespace Andy.Configuration.Ini
{
    public class EntryParserTests
    {
        private EntryParser target = new EntryParser();

        [TestCase("key=value", "key", "value")]
        [TestCase("key=value with spaces", "key", "value with spaces", Description = "Value with spaces")]
        [TestCase("key with spaces=value", "key with spaces", "value")]
        [TestCase(@"key=""value in quotes""", "key", @"""value in quotes""", Description = "Value in quotes")]
        public void Should_Parse_KeyValuePairs_SeparatedByAnEqalsSign(string input, string expectedKey, string expectedValue)
        {
            var result = target.Parse(input);

            Assert.AreEqual(expectedKey, result.Value.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value.Value, "Value");
        }

        [TestCase("key=value=two", "key", "value=two")]
        [TestCase("key=key=value=two", "key", "key=value=two")]
        [TestCase("key==value", "key", "=value")]
        [TestCase("key=value=", "key", "value=")]
        public void Should_TreatEverySubsequentEqualsSignAsPartOfValue(string input, string expectedKey, string expectedValue)
        {
            var result = target.Parse(input);

            Assert.AreEqual(expectedKey, result.Value.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value.Value, "Value");
        }

        [TestCase("key=value;", "key", "value;")]
        [TestCase("key=;", "key", ";")]
        [TestCase("key=;value", "key", ";value")]
        [TestCase("key=value#", "key", "value#")]
        [TestCase("key=#", "key", "#")]
        [TestCase("key=#value", "key", "#value")]
        public void Should_NotMindSpecialReservedCharacters(string input, string expectedKey, string expectedValue)
        {
            var result = target.Parse(input);

            Assert.AreEqual(expectedKey, result.Value.Key, "Key");
            Assert.AreEqual(expectedValue, result.Value.Value, "Value");
        }

        [Test]
        public void When_ThereIsNoValue_ShouldUseEmptyStringForValue()
        {
            var input = "key=";
            var result = target.Parse(input);

            Assert.AreEqual("key", result.Value.Key, "Key");
            Assert.AreEqual("", result.Value.Value, "Value");
        }

        [TestCase("key =value", ExpectedResult = "key")]
        [TestCase(" key=value", ExpectedResult = "key")]
        public string Should_TrimWhitespaceAroundKeys(string input)
        {
            return target.Parse(input).Value.Key;
        }

        [TestCase("key= value", ExpectedResult = " value")]
        [TestCase("key=value ", ExpectedResult = "value ")]
        [TestCase("key= ", ExpectedResult = " ", Description = "Whitespace-only value")]
        [TestCase("key=     ", ExpectedResult = "     ", Description = "Whitespace-only value")]
        public string Should_PreserveWhitespaceAroundValues(string input)
        {
            return target.Parse(input).Value.Value;
        }

        [TestCase("")]
        [TestCase("garbage")]
        [TestCase("=value")]
        [TestCase("=")]
        public void When_ThereIsNoKeyValuePair__Should_ReturnNull(string input)
        {
            var result = target.Parse(input);

            Assert.IsNull(result);
        }
    }
}