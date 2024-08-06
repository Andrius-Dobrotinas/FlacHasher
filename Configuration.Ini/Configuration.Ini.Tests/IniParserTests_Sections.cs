using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Configuration.Ini
{
    public partial class IniParserTests
    {
        private IniParser reader;
        private Mock<IEntryParser> entryParser;

        [SetUp]
        public void Setup()
        {
            entryParser = new Mock<IEntryParser>();
            reader = new IniParser(entryParser.Object);

            entryParser.Setup(
                x => x.Parse(
                    It.IsAny<string>()))
                .Returns<string>(
                    arg => new KeyValuePair<string, string>(arg, null));
        }

        [TestCaseSource(nameof(Get_SectionOnly_Combinations))]
        public void Should_CreateAnEntry_ForEachSecion(string[] lines, string[] expectedSectionNames)
        {
            var result = reader.Parse(lines);

            Assert.AreEqual(expectedSectionNames.Length, result.Keys.Count, "The number of sections in the resulting collection");

            foreach (var sectionName in expectedSectionNames)
            {
                Assert.IsTrue(result.ContainsKey(sectionName), $"Must contain a section named '{sectionName}'");
            }
        }

        [TestCase("[ SectionName]", Description = "Space at the beginning")]
        [TestCase("[  SectionName]", Description = "Multiple space chars at the beginning")]
        [TestCase("[SectionName ]", Description = "Space at the end")]
        [TestCase("[SectionName   ]", Description = "Multiple space chars at the end")]
        [TestCase("[ SectionName ]", Description = "Spaces at the beginning and end")]
        [TestCase("[  SectionName ]", Description = "Multiple chars at the beginning and end")]
        public void Should_TrimAnyWhitespace_AtTheBeginningAndEndOfA_SectionName(string sectionDeclarationLine)
        {
            var lines = new string[] { sectionDeclarationLine };

            var sectionName = reader.Parse(lines).Keys.First();

            Assert.AreEqual("SectionName", sectionName);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("     ")]
        public void WhenAnEmptySectionDeclarationIsEncountered__Should_ThrowAnException(string sectionName)
        {
            var lines = new string[] { $"[{sectionName}]" };

            Assert.Throws<InvalidSectionDeclaration>(
                () => reader.Parse(lines));
        }

        [TestCaseSource(nameof(Get_ConfigEntriesWithNoSections))]
        public void WhenNoSectionsAreDeclared_AndValidEntriesArePresent__Should_CreateANamelessSection(IList<string> lines)
        {
            var result = reader.Parse(lines);

            Assert.IsTrue(result.ContainsKey(""), "Must contain an empty key");
            Assert.AreEqual(1, result.Keys.Count, "Must only have one key");
        }

        [TestCase(";[Section]", Description = "Commented-out section")]
        [TestCase("; [Section]", Description = "Commented-out section (with a space between)")]
        public void WhenASectionIsCommentedOut__Should_Not_CreateAnySection(string line)
        {
            var lines = new string[] { line };

            var result = reader.Parse(lines);

            Assert.IsFalse(result.Keys.Any());
        }

        [TestCaseSource(nameof(Get_IgnoredLines))]
        public void WhenNoValidEntriesOrSectionsArePresent__Should_Not_CreateAnySection(IList<string> lines)
        {
            var result = reader.Parse(lines);

            Assert.AreEqual(0, result.Keys.Count, "Must only have one key");
        }

        [Test]
        public void Should_Not_AttemptToTreat_Sections_AsEntries()
        {
            var lines = new string[] { "[Section]" };

            var result = reader.Parse(lines);

            entryParser.Verify(
                x => x.Parse(
                    It.IsAny<string>()),
                Times.Never);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("     ")]
        public void Should_Not_Treat_EmptyLines_AsEntries(string line)
        {
            var lines = new string[]
            {
                "[Section]",
                line
            };

            var result = reader.Parse(lines);

            entryParser.Verify(
                x => x.Parse(
                    It.IsAny<string>()),
                Times.Never);
        }

        [TestCase(";")]
        [TestCase(";asd")]
        [TestCase("; asd")]
        [TestCase(";key=value")]
        [TestCase("; key=value")]
        [TestCase(";[Section]")]
        [TestCase("; [Section]")]
        public void Should_Not_Treat_EmptyLines_And_Commentaries_AsEntries(string line)
        {
            var lines = new string[]
            {
                "[Section]",
                line
            };

            var result = reader.Parse(lines);

            entryParser.Verify(
                x => x.Parse(
                    It.IsAny<string>()),
                Times.Never);
        }

        private static IEnumerable<TestCaseData> Get_IgnoredLines()
        {
            yield return new TestCaseData(
                new List<string>(0))
                .SetDescription("Empty file");

            yield return new TestCaseData(
                new List<string>
                { 
                    ""
                })
                .SetDescription("One empty line");

            yield return new TestCaseData(
                new List<string>
                {
                    "",
                    ""
                })
                .SetDescription("Multiple empty lines");

            yield return new TestCaseData(
                new List<string>
                {
                    ";"
                })
                .SetDescription("One commentary entry");

            yield return new TestCaseData(
                new List<string>
                {
                    ";",
                    ";"
                })
                .SetDescription("Multiple commentary entries");
        }

        private static IEnumerable<TestCaseData> Get_ConfigEntriesWithNoSections()
        {
            yield return new TestCaseData(
                new List<string>
                {
                    "key=value"
                })
                .SetDescription("A single key-value pair");

            yield return new TestCaseData(
                new List<string>
                {
                    "key=value",
                    "key2=value",
                    "key3=value"
                })
                .SetDescription("Multiple key-value pairs");

            yield return new TestCaseData(
                new List<string>
                {
                    "key=value",
                    "",
                    "key2=value",
                })
                .SetDescription("Multiple key-value pairs with blank lines between them");
            
            yield return new TestCaseData(
                new List<string>
                {
                    "",
                    "key=value",
                })
                .SetDescription("File starts with a blank line, but contains a valid entry");

            yield return new TestCaseData(
                new List<string>
                {
                    "",
                    "",
                    "",
                    "key=value",
                })
                .SetDescription("File starts with multiple blank lines, but contains a valid entry");

            yield return new TestCaseData(
                new List<string>
                {
                    "key=value",
                    ""
                })
                .SetDescription("File ends with a blank line");

            yield return new TestCaseData(
                new List<string>
                {
                    "key=value",
                    "",
                    "",
                    ""
                })
                .SetDescription("File ends with multiple blank lines");

            yield return new TestCaseData(
                new List<string>
                {
                    ";",
                    "key=value"
                })
                .SetDescription("File starts with a commentary, but has a valid entry");
        }

        private static IEnumerable<TestCaseData> Get_SectionOnly_Combinations()
        {
            yield return new TestCaseData(
                new string[]
                {
                    "[Section1]"
                },
                new string[] { "Section1" })
                .SetDescription("1 section");

            yield return new TestCaseData(
                new string[]
                {
                    "[Section1]",
                    "[Section2]",
                },
                new string[] { "Section1", "Section2" })
                .SetDescription("2 sections");

            yield return new TestCaseData(
                new string[]
                {
                    "[Section1]",
                    "[Section2]",
                    "[Section3]",
                },
                new string[] { "Section1", "Section2", "Section3" })
                .SetDescription("3 sections");

            yield return new TestCaseData(
                new string[]
                {
                    "[Section1]",
                    "key=value",
                    "[Section2]",
                    "key=value",
                },
                new string[] { "Section1", "Section2" })
                .SetDescription("2 sections with values");

            yield return new TestCaseData(
                new string[]
                {
                    "[Section1]",
                    "key=value",
                    "",
                    "[Section2]",
                    "key=value",
                },
                new string[] { "Section1", "Section2" })
                .SetDescription("Two sections with values and blank lines between them (1)");

            yield return new TestCaseData(
                new string[]
                {
                    "[Section1]",
                    "",
                    "key=value",
                    "",
                    "[Section2]",
                    "key=value",
                },
                new string[] { "Section1", "Section2" })
                .SetDescription("Two sections with values and blank lines between them (2)");
        }
    }
} 