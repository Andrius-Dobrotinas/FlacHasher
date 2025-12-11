using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.FlacHash.Hashfile.Read
{
    public class HashEntryCollectionParserTests
    {
        private HashEntryCollectionParser target;
        private Mock<IHashEntryParser> lineParser;

        [SetUp]
        public void Setup()
        {
            lineParser = new Mock<IHashEntryParser>();
            target = new HashEntryCollectionParser(lineParser.Object);
        }

        [Test]
        public void Must_return_all_parsed_values_to_the_caller()
        {
            var lines = new[] { "line1", "line2", "line3", "line4" };

            var expected = new[]
            {
                new KeyValuePair<string, string>("file1", "hash1"),
                new KeyValuePair<string, string>("file2", "hash2"),
                new KeyValuePair<string, string>("file3", "hash3"),
                new KeyValuePair<string, string>("file4", "hash4"),
            };

            for (var i = 0; i < lines.Length; i++)
            {
                var index = i;
                lineParser
                    .Setup(x => x.Parse(lines[index]))
                    .Returns(expected[index]);
            }

            var result = target.Parse(lines).ToArray();

            AssertThat.CollectionsMatchExactly(expected, result);
        }

        [Test]
        public void When_lineParser_returns_null_for_some_lines__Must_return_only_non_null_elements_to_the_caller()
        {
            var lines = new[] { "line1", "line2", "line3", "line4" };

            var value1 = new KeyValuePair<string, string>("file1", "hash1");
            var value4 = new KeyValuePair<string, string>("file4", "hash4");

            lineParser.Setup(x => x.Parse("line1")).Returns(value1);
            lineParser.Setup(x => x.Parse("line2")).Returns((KeyValuePair<string, string>?)null);
            lineParser.Setup(x => x.Parse("line3")).Returns((KeyValuePair<string, string>?)null);
            lineParser.Setup(x => x.Parse("line4")).Returns(value4);

            var result = target.Parse(lines).ToArray();

            var expected = new[] { value1, value4 };

            AssertThat.CollectionsMatchExactly(expected, result);
        }

        [Test]
        public void When_lineParser_throws_an_error_for_a_line__Must_rethrow_exception_with_line_number()
        {
            var lines = new[] { "line1", "line2", "line3", "line4" };

            var exception = new InvalidOperationException("Parsing failed");

            lineParser.Setup(x => x.Parse("line1")).Returns(new KeyValuePair<string, string>("file1", "hash1"));
            lineParser.Setup(x => x.Parse("line3")).Returns(new KeyValuePair<string, string>("file3", "hash3"));
            lineParser.Setup(x => x.Parse("line4")).Returns(new KeyValuePair<string, string>("file4", "hash4"));
            lineParser.Setup(x => x.Parse("line2")).Throws(exception);

            var ex = Assert.Throws<HashFileException>(() => target.Parse(lines).ToArray());

            Assert.That(ex.InnerException, Is.SameAs(exception));
            Assert.That(ex.Message, Does.Contain("line #2"));
        }
    }
}
