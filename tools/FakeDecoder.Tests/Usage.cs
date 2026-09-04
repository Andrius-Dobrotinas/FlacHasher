using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Andy.FakeDecoder
{
    public class Usage
    {
        const int usageErrorExitCode = 1;

        // Any file that certainly exists, so that the two-sources case is rejected for having two sources rather than for a missing file
        static readonly string existingFile = Assembly.GetExecutingAssembly().Location;

        [Test]
        public async Task When__NoArgumentsAreGiven__Must_Write_NothingToProcess_AndTheFlagList__And_Return_Zero()
        {
            var result = await App.Run();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.That(result.StdErr, Does.Contain("Nothing to process"));
                Assert.That(result.StdErr, Does.Contain(Arguments.UsageText));
            });
        }

        /// <summary>
        /// The flag list is there for a human running the program bare. A test drives it with arguments it has spelled
        /// out already, and would only have to filter the help back out of everything it reads off stderr.
        /// </summary>
        [Test]
        public async Task When__ArgumentsAreGiven_ButNoSource__Must_Write_NothingToProcess_WithoutTheFlagList()
        {
            var result = await App.Run("--exit-code", "5");

            Assert.Multiple(() =>
            {
                Assert.That(result.StdErr, Does.Contain("Nothing to process"));
                Assert.That(result.StdErr, Does.Not.Contain(Arguments.UsageText));
            });
        }

        [Test]
        public async Task When__NoArgumentsAreGiven__Must_Write_NothingToProcess_BeforeTheFlagList()
        {
            var result = await App.Run();

            Assert.Greater(
                result.StdErr.IndexOf(Arguments.UsageText),
                result.StdErr.IndexOf("Nothing to process"),
                "The flag list has to come after the reason it's being shown");
        }

        [TestCaseSource(nameof(GetInvalidArguments))]
        public async Task When__AnArgumentIsInvalid__Must_Write_TheFlagList_And_Return_One(string[] arguments)
        {
            var result = await App.Run(arguments);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(usageErrorExitCode, result.ExitCode, result.StdErr);
                Assert.That(result.StdErr, Does.Contain(Arguments.UsageText));
            });
        }

        /// <summary>
        /// Keeping the help off standard output is what stops it from leaking into every byte-for-byte assertion elsewhere.
        /// </summary>
        [TestCaseSource(nameof(GetInvalidArguments))]
        public async Task When__AnArgumentIsInvalid__Must_Write_Nothing_ToStdout(string[] arguments)
        {
            var result = await App.Run(arguments);

            Assert.IsEmpty(result.StdOut);
        }

        /// <summary>
        /// The defect this fences: the file passed the existence check and then threw on the open, and the run died
        /// with a runtime-chosen exit code in place of the one it was asked for.
        /// </summary>
        [Test]
        public async Task When__TheSourceFileCannotBeOpened__Must_Write_TheFlagList_And_Return_One()
        {
            var path = Path.Combine(Path.GetTempPath(), $"fakedecoder-held-{Guid.NewGuid():N}.bin");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });

            try
            {
                using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    var result = await App.Run("--file", path, "--exit-code", "5");

                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(usageErrorExitCode, result.ExitCode, result.StdErr);
                        Assert.That(result.StdErr, Does.Contain(Arguments.UsageText));
                        Assert.IsEmpty(result.StdOut);
                    });
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        static IEnumerable<TestCaseData> GetInvalidArguments()
        {
            yield return Case("An unknown flag", "--decode-everything", "yes");
            yield return Case("A flag missing its value", "--exit-code");
            yield return Case("Both sources", "--stdin", "--file", existingFile);
            yield return Case("An unparseable number", "--read-chunk-size", "quite a few");
            yield return Case("A source file that isn't there", "--file", TestPayload.MissingSourceFile.FullName);
            yield return Case("An empty source path", "--file", "");
            yield return Case("A flag that needs a source, without one", "--keep-stdout-open", "500");
            // With a source, so that it's the size that's rejected rather than the missing source
            yield return Case("A buffer bigger than the maximum", "--stdin", "--expand", (Arguments.MaxBufferBytes + 1).ToString());
            yield return Case("A repeated flag", "--exit-code", "1", "--exit-code", "2");
            yield return Case("A value that is a flag name", "--progress-message", "--stdin");
        }

        static TestCaseData Case(string name, params string[] arguments)
        {
            return new TestCaseData(new object[] { arguments }).SetName($"{{m}}({name})");
        }
    }
}
