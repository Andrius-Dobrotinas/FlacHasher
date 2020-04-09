using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Configuration.Ini
{
    public partial class IniParserTests
    {
        [TestCaseSource(nameof(Get_ConfigurationEntries))]
        public void Should_ParseEveryConfigEntry(IList<string> lines, IList<string> entryLines)
        {
            var result = reader.Parse(lines);

            entryParser.Verify(
                x => x.Parse(
                    It.IsAny<string>()),
                Times.Exactly(entryLines.Count),
                "Must invoke the function for each line, for a start");

            foreach (var entryLine in entryLines)
            {
                entryParser.Verify(
                    x => x.Parse(
                        It.Is<string>(
                            arg => arg == entryLine)),
                "Must invoke the function");
            }
        }

        [Test]
        public void When_EntryParserReturnsNull__ShouldIgnoreTheEntry__WhenNoSectionIsDeclared()
        {
            var value = "notAValidEntry";

            var lines = new string[]
            {
                value
            };

            entryParser.Setup(
                x => x.Parse(
                    It.IsAny<string>()))
                .Returns<KeyValuePair<string, string>?>(null);

            var result = reader.Parse(lines);

            entryParser.Verify(
                x => x.Parse(
                    It.Is<string>(
                        arg => arg == value)),
                "Must attempt to parse the line");

            Assert.IsTrue(!result.Keys.Any(), "Must not add anything to the dictionary");
        }

        [Test]
        public void When_EntryParserReturnsNull__ShouldIgnoreTheEntry__WhenASectionIsDeclared()
        {
            var value = "notAValidEntry";

            var lines = new string[]
            {
                "[Section]",
                value
            };

            entryParser.Setup(
                x => x.Parse(
                    It.IsAny<string>()))
                .Returns<KeyValuePair<string, string>?>(null);

            var result = reader.Parse(lines);

            entryParser.Verify(
                x => x.Parse(It.Is<string>(
                    arg => arg == value)),
                "Must attempt to parse the line");

            Assert.IsTrue(!result["Section"].Keys.Any(), "Must not add anything to the specified section");
            Assert.IsFalse(result.Keys.Where(x => x != "Section").Any(), "Must not add anything to the dictionary");
        }

        [TestCaseSource(nameof(Get_ConfigEntriesWithSections))]
        public void Should_PutEachConfigEntry_IntoAnAppropriateSection(string[] lines, Dictionary<string, Dictionary<string, string>> expectedResult)
        {
            entryParser.Setup(
                x => x.Parse(
                    It.IsAny<string>()))
                .Returns<string>(
                    arg => {
                        var segments = arg.Split('=');
                        return new KeyValuePair<string, string>(segments.First(), segments.Last());
                    });

            var result = reader.Parse(lines);

            Assert.AreEqual(expectedResult.Keys.Count, result.Keys.Count, "The number of sections in the resulting collection");

            foreach (var section in expectedResult)
            {
                Assert.IsTrue(result.ContainsKey(section.Key), $"The result must contain a section named '{section.Key}'");

                var actualSection = result[section.Key];

                Assert.AreEqual(section.Value.Keys.Count, actualSection.Keys.Count, "Section key count");

                foreach (var entry in section.Value)
                {
                    Assert.IsTrue(actualSection.ContainsKey(entry.Key), $"The section must contain an entry named '{section.Key}' under '{section.Key}' section");

                    var actualEntryValue = actualSection[entry.Key];

                    Assert.AreEqual(entry.Value, actualEntryValue, $"{section.Key}->{entry.Key} value must be {entry.Value}. Actual: {actualEntryValue}");
                }
            }
        }
        
        private static IEnumerable<TestCaseData> Get_ConfigEntriesWithSections()
        {
            yield return new TestCaseData(
                new string[]
                {
                    "key=value"
                },
                new Dictionary<string, Dictionary<string, string>>
                {
                    { "", new Dictionary<string, string> { { "key", "value" } } }
                })
                .SetDescription("just 1 entry, no section");

            yield return new TestCaseData(
                new string[]
                {
                    "key=value",
                    "key2=value2"
                },
                new Dictionary<string, Dictionary<string, string>>
                {
                    { "", new Dictionary<string, string>
                        {
                            { "key", "value" },
                            { "key2", "value2" }
                        }
                    }
                })
                .SetDescription("multiple entries, no section");

            yield return new TestCaseData(
                new string[]
                {
                    "key=value",
                    "",
                    "key2=value2"
                },
                new Dictionary<string, Dictionary<string, string>>
                {
                    { "", new Dictionary<string, string>
                        {
                            { "key", "value" },
                            { "key2", "value2" }
                        }
                    }
                })
                .SetDescription("multiple entries, blank line in-between, no section");

            yield return new TestCaseData(
                new string[]
                {
                    "[Section1]",
                    "key=value",
                    "key2=value2"
                },
                new Dictionary<string, Dictionary<string, string>>
                {
                    { "Section1", new Dictionary<string, string>
                        {
                            { "key", "value" },
                            { "key2", "value2" }
                        }
                    }
                })
                .SetDescription("1 section, multiple entries");

            yield return new TestCaseData(
                new string[]
                {
                    "[Section1]",
                    "key=value",
                    "[Section2]",
                    "key2=value2",
                    "key3=value3"
                },
                new Dictionary<string, Dictionary<string, string>>
                {
                    { "Section1", new Dictionary<string, string>{ { "key", "value" }} },
                    { "Section2", new Dictionary<string, string>
                        {
                            { "key2", "value2" },
                            { "key3", "value3" }
                        }
                    }
                })
                .SetDescription("Multiple sections, multiple entries");

            yield return new TestCaseData(
                new string[]
                {
                    "key=value",
                    "[Section2]",
                    "key2=value2"
                },
                new Dictionary<string, Dictionary<string, string>>
                {
                    { "", new Dictionary<string, string>{ { "key", "value" }} },
                    { "Section2", new Dictionary<string, string>
                        {
                            { "key2", "value2" },
                        }
                    }
                })
                .SetDescription("An entry with no section, other entry defined in a section");
        }

        private static IEnumerable<TestCaseData> Get_ConfigurationEntries()
        {
            yield return new TestCaseData(
                new List<string>
                {
                    "key=value"
                },
                new List<string>
                {
                    "key=value"
                })
                .SetDescription("1 config entry");

            yield return new TestCaseData(
                new List<string>
                {
                    "key=value",
                    "key2=value"
                },
                new List<string>
                {
                    "key=value",
                    "key2=value"
                })
                .SetDescription("Multiple config entries");
        }
    }
}