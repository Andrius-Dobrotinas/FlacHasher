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
                result.StdErr.Should().ContainEquivalentOf("Audio decoding failed", "the user needs to know roughly what went wrong");
                result.StdErr.Should().Contain("Decoder output:", "the decoder's own output has to be relayed to the user");
                GetRelayedDecoderOutput(result.StdErr).Should().NotBeNullOrWhiteSpace("the decoder's own output is the only account of what went wrong - it must reach the user");
                result.StdErr.Should().ContainEquivalentOf("Possible reasons: the file may be corrupt, wrong format or decoder is misconfigured/incorrect parameters");
                result.ExitCode.Should().Be(10, "Indicates audio decoder error");
            });
        }

        [Test]
        public async Task Hashing_with_invalid_decoder_parameters__exits_with_error__and_relays_the_decoders_complaint()
        {
            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Sample1.Flac.FileName);
            var decoderParams = HashCommand.FlacStreamDecoderParams.Append("--andy-flag").ToArray();

            var result = await RunHashing(inputFile, decoderParams);

            Assert.Multiple(() =>
            {
                result.StdOut.Should().BeEmpty();
                result.StdErr.Should().Contain("Decoder output:", "only the decoder knows what the problem with parameters is - that must be relayed to the user");
                
                // What the decoder says is not pinned down - that would tie the test to one decoder version
                GetRelayedDecoderOutput(result.StdErr).Should().NotBeNullOrWhiteSpace("the decoder's complaint is the only account of what went wrong");
                result.ExitCode.Should().Be(10, "Indicates audio decoder error");
            });
        }

        [Test]
        public async Task Hashing_a_file__when_the_decoder_process_fails__relays_its_exit_code_and_error_output()
        {
            const int decoderExitCode = 5;
            const string decoderErrorMessage = "simulated decoder failure";

            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Sample1.Flac.FileName);
            var decoderParams = new[] { "--stdin", "--exit-code", decoderExitCode.ToString(), "--error-message", decoderErrorMessage };

            var arguments = HashCommand.Arguments(inputFile, TestEnvironment.GetFakeDecoder(), "MD5", decoderParams);
            var result = await App.RunRaw(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.StdOut.Should().BeEmpty("no valid hash can be calculated when the decoder itself failed");
                result.StdErr.Should().ContainEquivalentOf($"exited with code {decoderExitCode}", "the decoder's exit code has to be relayed to the user");
                result.StdErr.Should().Contain(decoderErrorMessage, "the decoder's error output has to be relayed to the user");
                result.StdErr.Should().NotContainEquivalentOf("ActualException", "the user must not be pointed at an internal exception member - the actual error message is relayed instead");
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
                result.StdErr.Should().ContainEquivalentOf("Audio decoding failed");
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
                result.StdErr.Should().ContainEquivalentOf("Audio decoding failed");
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

        static string GetRelayedDecoderOutput(string stdErr)
        {
            const string marker = "Decoder output:";
            var markerIndex = stdErr.IndexOf(marker, StringComparison.Ordinal);

            return markerIndex < 0 ? string.Empty : stdErr[(markerIndex + marker.Length)..];
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
