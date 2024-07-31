using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Hashing.Verification
{
    public class HashMapParserTests
    {
        HashMapParser target;
        Mock<IHashEntryCollectionParser> parser;
        Mock<IEqualityComparer<string>> stringComparer;


        [SetUp]
        public void Setup()
        {
            parser = new Mock<IHashEntryCollectionParser>();
            stringComparer = new Mock<IEqualityComparer<string>>();
            target = new HashMapParser(parser.Object, stringComparer.Object);

            stringComparer.Setup(x => x.Equals(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        }

        [TestCaseSource(nameof(Get_WithFilesNames))]
        public void When_file_names_Are_Specified__Must_set_IsPositionBased_to_False(IList<KeyValuePair<string, string>> parsedData)
        {
            var sourceLines = parsedData.Select(x => "line").ToArray();

            Setup_Parser(sourceLines, parsedData);

            var result = target.Parse(sourceLines);

            Assert.IsFalse(result.IsPositionBased);
        }

        [TestCaseSource(nameof(Get_WithFilesNames))]
        public void Must_return_the_Parsed_lines_in_the_order_they_were_parsed__When_file_names_Are_Specified(IList<KeyValuePair<string, string>> parsedData)
        {
            var sourceLines = parsedData.Select(x => "line").ToArray();

            Setup_Parser(sourceLines, parsedData);

            var result = target.Parse(sourceLines);

            AssertThat.CollectionsMatchExactly(parsedData, result.Hashes);
        }

        [TestCaseSource(nameof(Get_NoFilenames))]
        public void Must_return_the_Parsed_lines_in_the_order_they_were_parsed__When_file_names_are_Not_Specified(IList<KeyValuePair<string, string>> parsedData)
        {
            var sourceLines = parsedData.Select(x => "line").ToArray();

            Setup_Parser(sourceLines, parsedData);

            var result = target.Parse(sourceLines);

            AssertThat.CollectionsMatchExactly(parsedData, result.Hashes);
        }

        [TestCaseSource(nameof(Get_NoFilenames))]
        public void When_file_names_are_Not_Specified__Must_set_IsPositionBased_to_True(IList<KeyValuePair<string, string>> parsedData)
        {
            var sourceLines = parsedData.Select(x => "line").ToArray();

            Setup_Parser(sourceLines, parsedData);

            var result = target.Parse(sourceLines);

            Assert.IsTrue(result.IsPositionBased);
        }

        [TestCaseSource(nameof(Get_SomeFilenamesMissing))]
        public void When_Some_file_names_are_Not_Specified__Must_throw_an_exception(IList<KeyValuePair<string, string>> parsedData)
        {
            var sourceLines = parsedData.Select(x => "line").ToArray();

            Setup_Parser(sourceLines, parsedData);

            Assert.Throws<MissingFileNameException>(
                () => target.Parse(sourceLines));
        }

        [TestCaseSource(nameof(Get_NoHashes))]
        public void When_Hash_is_not_specified__Must_throw_an_exception(string description, IList<KeyValuePair<string, string>> parsedData)
        {
            var sourceLines = parsedData.Select(x => "line").ToArray();

            Setup_Parser(sourceLines, parsedData);

            Assert.Throws<MissingHashValueException>(
                () => target.Parse(sourceLines));
        }

        [TestCaseSource(nameof(Get_RepeatedFilenames2))]
        public void When_file_name_is_repeated__Must_throw_an_exception(
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

            var sourceLines = parsedData.Select(x => "line").ToArray();

            Setup_Parser(sourceLines, parsedData);

            var target = new HashMapParser(parser.Object, stringComparer.Object);

            Assert.Throws<DuplicateFileException>(
                () => target.Parse(sourceLines));
        }

        private void Setup_Parser(string[] sourceLines, IEnumerable<KeyValuePair<string, string>> returnValue)
        {
            parser.Setup(
                x => x.Parse(
                    It.Is<string[]>(
                        arg => arg == sourceLines)))
                .Returns(returnValue);
        }

        private static IEnumerable<TestCaseData> Get_WithFilesNames()
        {
            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("file", "hash")
                });

            yield return new TestCaseData(
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("file 1", "hash"),
                    new KeyValuePair<string, string>("file 2", "hash"),
                    new KeyValuePair<string, string>("file 3", "hash")
                });
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
                "With file name",
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("file", null)
                });

            yield return new TestCaseData(
                "With no file name",
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>(null, null)
                });

            yield return new TestCaseData(
                "With file name",
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("file", "hash"),
                    new KeyValuePair<string, string>("file 2", null)
                });

            yield return new TestCaseData(
                "With no file name",
                new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>(null, "hash"),
                    new KeyValuePair<string, string>(null, null)
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