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

        [TestCaseSource(nameof(GetFileCountAndOutputModeCases))]
        public async Task Hashing_with_progress__lists_every_hash_on_stderr(SampleFile[] filesToHash, string outputFormat)
        {
            var expectedHashes = filesToHash.Select(x => x.ExpectedMd5).ToArray();

            var arguments = BuildHashArguments(filesToHash, outputFormat);

            // Raw output isn't text, so std-out is read as bytes and left alone here
            var result = await App.RunRaw(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdErr.Should().ContainAll(expectedHashes, "those who read the console output are given the results in one place, without the progress output in between");
            });
        }

        static string[] BuildHashArguments(SampleFile[] filesToHash, string outputFormat = null)
        {
            var inputFiles = filesToHash.Select(x => TestEnvironment.GetTestAsset(x.FileName));

            return HashCommand.Arguments(inputFiles, TestEnvironment.GetFlacDecoder(), "MD5", HashCommand.FlacStreamDecoderParams, outputFormat, printProgress: true);
        }

        // The summary is meant for multiple files, but nothing in the application ties it to a file count.
        // Raw output has no format of its own, and yet its summary must still be rendered as text.
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
