using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public class ValidatingHashFileParserTests
    {
        private ValidatingHashFileParser target;
        private Mock<IHashFileParser> parser;

        [SetUp]
        public void Setup()
        {
            parser = new Mock<IHashFileParser>();
            target = new ValidatingHashFileParser(parser.Object);
        }

        [TestCaseSource(nameof(Get_NoFilenames))]
        public void When_file_name_is_not_specified__Must_throw_an_exception(IList<KeyValuePair<string, string>> parsedData)
        {
            var sourceLines = new string[parsedData.Count];

            Setup_Parser(sourceLines, parsedData);

            Assert.Throws<MissingFileNameException>(
                () => target.Parse(sourceLines).ToArray());
        }

        [TestCaseSource(nameof(Get_NoHashes))]
        public void When_hash_is_not_specified__Must_throw_an_exception(IList<KeyValuePair<string, string>> parsedData)
        {
            var sourceLines = new string[parsedData.Count];

            Setup_Parser(sourceLines, parsedData);

            Assert.Throws<MissingHashValueException>(
                () => target.Parse(sourceLines).ToArray());
        }

        [TestCaseSource(nameof(Get_RepeatedFilenames))]
        public void When_file_name_is_repeated__Must_throw_an_exception(IList<KeyValuePair<string, string>> parsedData)
        {
            var sourceLines = new string[parsedData.Count];

            Setup_Parser(sourceLines, parsedData);

            Assert.Throws<DuplicateFileException>(
                () => target.Parse(sourceLines).ToArray());
        }

        private void Setup_Parser(string[] sourceLines, IEnumerable<KeyValuePair<string, string>> returnValue)
        {
            parser.Setup(
                x => x.Parse(
                    It.Is<string[]>(
                        arg => arg == sourceLines)))
                .Returns(returnValue);
        }

        private static IEnumerable<TestCaseData> Get_NoFilenames()
        {
            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>(null, "hash")
                });

            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("file", "hash"),
                    new KeyValuePair<string, string>(null, "hash")
                });
        }

        private static IEnumerable<TestCaseData> Get_NoHashes()
        {
            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("file", null)
                });

            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("file", "hash"),
                    new KeyValuePair<string, string>("file 2", null)
                });
        }

        private static IEnumerable<TestCaseData> Get_RepeatedFilenames()
        {
            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("file", "hash"),
                    new KeyValuePair<string, string>("file", "hash2")
                });

            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("file 1", "hash"),
                    new KeyValuePair<string, string>("file 2", "hash 2"),
                    new KeyValuePair<string, string>("file 1", "hash 3"),
                });
        }
    }
}