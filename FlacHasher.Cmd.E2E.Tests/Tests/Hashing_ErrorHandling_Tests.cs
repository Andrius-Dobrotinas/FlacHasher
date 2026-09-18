using System.Text;
using CliWrap;
using FluentAssertions;
using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    [TestFixture]
    public class Hashing_ErrorHandling_Tests
    {
        const int md5Length = 16;

        static readonly (string FileName, string ExpectedMd5)[] goodFiles =
        {
            (SampleAsset.Sample1.Flac.FileName, SampleAsset.Sample1.ExpectedMd5),
            (SampleAsset.Sample2.Flac.FileName, SampleAsset.Sample2.ExpectedMd5),
            (SampleAsset.Sample3.Flac.FileName, SampleAsset.Sample3.ExpectedMd5)
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

        [Test]
        public async Task Hashing_a_file__that_does_not_exist__exits_with_error__reports_why()
        {
            var inputFile = new FileInfo(Path.Combine(workingDirectory.FullName, "no-such-file.flac"));

            var result = await RunHashing(inputFile, HashCommand.FlacStreamDecoderParams);

            Assert.Multiple(() =>
            {
                result.StdOut.Should().BeEmpty();
                result.StdErr.Should().ContainEquivalentOf("File not found", "the user has to be told what went wrong to be able to act on it");
                result.ExitCode.Should().Be(20);
            });
        }

        [TestCaseSource(nameof(GetUnhashableFileCases))]
        public async Task Hashing_a_file__that_is_not_valid__produces_no_hash__exits_with_error__and_tells_the_user_why(Func<DirectoryInfo, FileInfo> getInputFile)
        {
            var result = await RunHashing(getInputFile(workingDirectory), HashCommand.FlacStreamDecoderParams);

            Assert.Multiple(() =>
            {
                result.StdOut.Should().BeEmpty("no valid hash can be calculated for the input");
                result.StdErr.Should().ContainEquivalentOf("Couldn't Decode audio", "the user needs to know roughly what went wrong");
                result.StdErr.Should().ContainEquivalentOf("Possible reasons: the file may be corrupt, wrong format or decoder is misconfigured/incorrect parameters");
                result.ExitCode.Should().Be(10, "Indicates audio decoder error");
            });
        }

        [Test]
        public async Task Hashing_with_invalid_decoder_parameters__exits_with_error__and_relays_the_decoders_complaint()
        {
            const string invalidFlag = "--andy-flag";

            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Sample1.Flac.FileName);
            var decoderParams = HashCommand.FlacStreamDecoderParams.Append(invalidFlag).ToArray();

            // Whatever the decoder has to say about the parameters is the reference - hard-coding it would tie the test
            // to one decoder version, and would pass just as well if the application had rejected the parameters itself
            var (decoderExitCode, decoderComplaint) = await RunDecoder(decoderParams);

            if (decoderExitCode != 1 || string.IsNullOrEmpty(decoderComplaint))
                throw new InvalidOperationException(
                    $"This test needs the decoder to reject {invalidFlag} and say why, but it exited with {decoderExitCode} and wrote: {decoderComplaint}");

            var result = await RunHashing(inputFile, decoderParams);

            Assert.Multiple(() =>
            {
                result.StdOut.Should().BeEmpty();
                Normalize(result.StdErr).Should().Contain(Normalize(decoderComplaint), "only the decoder knows what went wrong - that must be relayed to the user");
                result.ExitCode.Should().Be(10, "Indicates audio decoder error");
            });
        }

        [TestCaseSource(nameof(GetFailurePositionCases))]
        public async Task Hashing_multiple_files__stops_at_the_file_that_fails_to_decode__never_processing_the_ones_after(
            FileInfo[] filesToHash, string[] expectedHashesProduced)
        {
            var expectedHashes = expectedHashesProduced.Select(Convert.FromHexString).ToArray();

            var arguments = HashCommand.Arguments(filesToHash, TestEnvironment.GetFlacDecoder(), "MD5", HashCommand.FlacStreamDecoderParams);
            var result = await App.RunRaw(workingDirectory, arguments);

            var actualHashes = Enumerable.Range(0, expectedHashes.Length)
                .Select(x => result.StdOut.Skip(x * md5Length).Take(md5Length).ToArray())
                .ToArray();

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(10, "Indicates audio decoder error");
                result.StdErr.Should().ContainEquivalentOf("Couldn't Decode audio");
                result.StdOut.Length.Should().Be(expectedHashes.Length * md5Length,
                    "processing must stop at the failing file - none of the files after it may be reached");
                actualHashes.Should().BeEquivalentTo(expectedHashes, options => options.WithStrictOrdering());
            });
        }

        [TestCaseSource(nameof(GetFailurePositionCases))]
        public async Task Hashing_multiple_files__with_a_format__stops_at_the_file_that_fails_to_decode__never_processing_the_ones_after(
            FileInfo[] filesToHash, string[] expectedHashesProduced)
        {
            var arguments = HashCommand.Arguments(filesToHash, TestEnvironment.GetFlacDecoder(), "MD5", HashCommand.FlacStreamDecoderParams, outputFormat: "{hash}");
            var result = await App.Run(workingDirectory, arguments);

            var lines = result.StdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(10, "Indicates audio decoder error");
                result.StdErr.Should().ContainEquivalentOf("Couldn't Decode audio");
                lines.Should().BeEquivalentTo(expectedHashesProduced,
                    "processing must stop at the failing file - the formatted-output path must honor that too, not just the raw one");
            });
        }

        static IEnumerable<TestCaseData> GetFailurePositionCases()
        {
            yield return new TestCaseData(
                    new[] { GetAsset(SampleAsset.TruncatedFlac.FileName), GetAsset(goodFiles[0].FileName), GetAsset(goodFiles[1].FileName), GetAsset(goodFiles[2].FileName) },
                    Array.Empty<string>())
                .SetName("{m}(Failing file is first - none of the files get hashed)");

            yield return new TestCaseData(
                    new[] { GetAsset(goodFiles[0].FileName), GetAsset(SampleAsset.TruncatedFlac.FileName), GetAsset(goodFiles[1].FileName), GetAsset(goodFiles[2].FileName) },
                    new[] { goodFiles[0].ExpectedMd5 })
                .SetName("{m}(Failing file is in the middle - the file after it never gets reached)");

            yield return new TestCaseData(
                    new[] { GetAsset(goodFiles[0].FileName), GetAsset(goodFiles[1].FileName), GetAsset(goodFiles[2].FileName), GetAsset(SampleAsset.TruncatedFlac.FileName) },
                    new[] { goodFiles[0].ExpectedMd5, goodFiles[1].ExpectedMd5, goodFiles[2].ExpectedMd5 })
                .SetName("{m}(Failing file is last - every file before it was already hashed)");
        }

        static FileInfo GetAsset(string fileName) => TestEnvironment.GetTestAsset(fileName);


        static async Task<(int ExitCode, string StdErr)> RunDecoder(string[] decoderParams)
        {
            var decoder = TestEnvironment.GetFlacDecoder();
            var stdErr = new StringBuilder();

            var result = await Cli.Wrap(decoder.FullName)
                .WithArguments(decoderParams)
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErr))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            return (result.ExitCode, stdErr.ToString().Trim());
        }

        static string Normalize(string text)
        {
            return text.Replace("\r\n", "\n");
        }

        Task<AppRawResult> RunHashing(FileInfo inputFile, string[] decoderParams)
        {
            var arguments = HashCommand.Arguments(inputFile, TestEnvironment.GetFlacDecoder(), "MD5", decoderParams);

            return App.RunRaw(workingDirectory, arguments);
        }

        static IEnumerable<TestCaseData> GetUnhashableFileCases()
        {
            yield return new TestCaseData(
                    (Func<DirectoryInfo, FileInfo>)(directory => WriteNonAudioFile(directory)))
                .SetName("{m}(Not an audio file)");

            yield return new TestCaseData(
                    (Func<DirectoryInfo, FileInfo>)(_ => TestEnvironment.GetTestAsset(SampleAsset.TruncatedFlac.FileName)))
                .SetName("{m}(Truncated audio file)");

            // Audio the decoder has no business decoding
            yield return new TestCaseData(
                    (Func<DirectoryInfo, FileInfo>)(_ => TestEnvironment.GetTestAsset(SampleAsset.Sample1.Ape.FileName)))
                .SetName("{m}(Wrong audio format)");
        }

        static FileInfo WriteNonAudioFile(DirectoryInfo directory)
        {
            var file = new FileInfo(Path.Combine(directory.FullName, "not-audio.flac"));

            File.WriteAllText(file.FullName, "This is not audio");

            return file;
        }
    }
}
