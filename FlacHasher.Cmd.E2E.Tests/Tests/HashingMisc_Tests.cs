using FluentAssertions;
using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    [TestFixture]
    public class HashingMisc_Tests
    {
        public record SampleFile(string FileName, string ExpectedMd5);

        static readonly SampleFile[] flacSet =
        {
            new(SampleAsset.Sample1.Flac.FileName, SampleAsset.Sample1.ExpectedMd5),
            new(SampleAsset.Sample2.Flac.FileName, SampleAsset.Sample2.ExpectedMd5),
            new(SampleAsset.Sample3.Flac.FileName, SampleAsset.Sample3.ExpectedMd5)
        };

        DirectoryInfo workingDirectory;

        [OneTimeSetUp]
        public void Setup()
        {
            workingDirectory = TestEnvironment.SetUpWorkingDirWithSettingsFile();
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            workingDirectory?.Delete(recursive: true);
        }

        const string summaryOpener = "======== Results =========";
        const string summaryCloser = "======== The End =========";

        /// <summary>
        /// Raw output has no format of its own, and yet its summary must still be rendered as text.
        /// </summary>
        [TestCaseSource(nameof(GetFileCountAndOutputModeCases))]
        public async Task Hashing__with_progress__writes_a_results_summary_with_all_hashes_listed__on_stderr(SampleFile[] filesToHash, string outputFormat)
        {
            var expectedHashes = filesToHash.Select(x => x.ExpectedMd5).ToArray();

            var arguments = BuildHashArguments(filesToHash, outputFormat);

            var result = await App.RunRaw(workingDirectory, arguments);

            var summaryStart = result.StdErr.IndexOf(summaryOpener, StringComparison.Ordinal);
            var summaryEnd = result.StdErr.IndexOf(summaryCloser, StringComparison.Ordinal);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                summaryStart.Should().BeGreaterThanOrEqualTo(0, "those who read the console output are given the results in a delimited summary");
                summaryEnd.Should().BeGreaterThan(summaryStart, "the summary's closer must come after its opener");

                if (summaryStart >= 0 && summaryEnd > summaryStart)
                {
                    var summary = result.StdErr.Substring(summaryStart, summaryEnd - summaryStart);
                    summary.Should().ContainAll(expectedHashes, "the summary must list every file's hash");

                    var beforeSummary = result.StdErr.Substring(0, summaryStart);
                    var afterSummary = result.StdErr.Substring(summaryEnd + summaryCloser.Length);

                    beforeSummary.Should().NotContainAny(expectedHashes, "each hash must be rendered only once, INSIDE the results summary");
                    afterSummary.Should().NotContainAny(expectedHashes, "each hash must be rendered only once, INSIDE the results summary");
                }
            });
        }

        [TestCaseSource(nameof(GetFileCountAndOutputModeCases))]
        public async Task Hashing__without_progress__writes_no_summary_and_no_hashes__on_stderr(SampleFile[] filesToHash, string outputFormat)
        {
            var expectedHashes = filesToHash.Select(x => x.ExpectedMd5).ToArray();

            var arguments = BuildHashArguments(filesToHash, outputFormat, printProgress: false);

            var result = await App.RunRaw(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdErr.Should().NotContain(summaryOpener, "no results summary is wanted");
                result.StdErr.Should().NotContain(summaryCloser, "no results summary is wanted");

                result.StdErr.Should().NotContainAny(expectedHashes, "hashes aren't echoed to stderr anywhere");
            });
        }

        static string[] BuildHashArguments(SampleFile[] filesToHash, string outputFormat = null, bool printProgress = true)
        {
            var inputFiles = filesToHash.Select(x => TestEnvironment.GetTestAsset(x.FileName));

            return HashCommand.Arguments(inputFiles, TestEnvironment.GetFlacDecoder(), "MD5", HashCommand.FlacStreamDecoderParams, outputFormat, printProgress: printProgress);
        }

        static IEnumerable<TestCaseData> GetFileCountAndOutputModeCases()
        {
            yield return new TestCaseData(flacSet.Take(1).ToArray(), null)
                .SetName("{m}(One file)(Raw Bytes)");

            yield return new TestCaseData(flacSet, null)
                .SetName("{m}(Multiple files)(Raw Bytes)");

            yield return new TestCaseData(flacSet.Take(1).ToArray(), "{hash}")
                .SetName("{m}(One file)(Formatted)");

            yield return new TestCaseData(flacSet, "{hash}")
                .SetName("{m}(Multiple files)(Formatted)");
        }
    }
}
