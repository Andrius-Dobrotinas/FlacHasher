using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Verification
{
    public class ValidatingFileHashParserTests
    {
        private ValidatingFileHashParser target;
        private Mock<IFileHashParser> parser;

        [SetUp]
        public void Setup()
        {
            parser = new Mock<IFileHashParser>();
            target = new ValidatingFileHashParser(parser.Object);
        }

        [TestCaseSource(nameof(Get_NoFilenames))]
        public void When_no_file_names_are_specified__Return_the_result(IList<KeyValuePair<string, string>> parsedData)
        {
            var sourceLines = new string[parsedData.Count];

            Setup_Parser(sourceLines, parsedData);

            var result = target.Parse(sourceLines).ToArray();

            Assert.AreEqual(parsedData.Count, result.Length, "The resulting collection must be the correct length");

            for (int i = 0; i < parsedData.Count; i++)
            {
                Assert.AreEqual(parsedData[i], result[i], $"Item {i} reference must match");
            }
        }

        [TestCaseSource(nameof(Get_SomeFilenamesMissing))]
        public void When_some_file_names_are_not_specified__Must_throw_an_exception(IList<KeyValuePair<string, string>> parsedData)
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

        [TestCaseSource(nameof(Get_RepeatedFilenames2))]
        public void When_file_name_is_repeated__based_on_the_supplied_string_Comparer__Must_throw_an_exception(
            IList<(string, string, bool)> file_hash_isRepeated)
        {
            var stringComparer = new Mock<IEqualityComparer<string>>();
            
            foreach (var entry in file_hash_isRepeated.Select(x => new { Filename = x.Item1, IsRepeated = x.Item3 }))
            {
                stringComparer.Setup(
                    x => x.Equals(
                        It.IsAny<string>(),
                        It.Is<string>(
                            arg => arg == entry.Filename)))
                    .Returns(entry.IsRepeated);
            }

            var parsedData = file_hash_isRepeated
                .Select(x => new KeyValuePair<string, string>(x.Item1, x.Item2))
                .ToList();

            var sourceLines = new string[parsedData.Count];


            Setup_Parser(sourceLines, parsedData);
            
            var target = new ValidatingFileHashParser(parser.Object, stringComparer.Object);

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
                    new KeyValuePair<string, string>(null, "hash"),
                    new KeyValuePair<string, string>(null, "hash")
                });

            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>(null, "hash"),
                    new KeyValuePair<string, string>(null, "hash 2"),
                    new KeyValuePair<string, string>(null, "hash")
                });
        }

        private static IEnumerable<TestCaseData> Get_SomeFilenamesMissing()
        {
            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>(null, "hash"),
                    new KeyValuePair<string, string>("file", "hash")
                });

            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>(null, "hash"),
                    new KeyValuePair<string, string>(null, "hash 2"),
                    new KeyValuePair<string, string>("file", "hash"),
                    new KeyValuePair<string, string>("file", "hash")
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

        private static IEnumerable<TestCaseData> Get_RepeatedFilenames2()
        {
            yield return new TestCaseData(
                new List<(string, string, bool)> {
                    ("file", "hash", false),
                    ("File", "hash2", true)
                });

            yield return new TestCaseData(
                new List<(string, string, bool)> {
                    ("file 1", "hash", false),
                    ("file 2", "hash 2", false),
                    ("FILE 1", "hash 3", true),
                    ("file 3", "hash 4", false),
                });
        }
    }
}