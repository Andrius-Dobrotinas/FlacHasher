using FluentAssertions;
using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    [TestFixture]
    public class Hashing_Tests
    {
        DirectoryInfo workingDirectory;

        [OneTimeSetUp]
        public void Setup()
        {
            // The application requires the settings file to exist, but I want to limit testing surface here - hence a directory with an empty settings file
            workingDirectory = TestEnvironment.SetUpWorkingDirWithSettingsFile();
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            workingDirectory?.Delete(recursive: true);
        }

        [TestCaseSource(nameof(GetDecodeAndHashCases))]
        public async Task Hashing_a_file__produces_the_hash__for_the_given_input(string fileToHash, string expectedHashString, FileInfo decoder, string[] decoderParams)
        {
            var expectedHash = Convert.FromHexString(expectedHashString);
            var inputFile = TestEnvironment.GetTestAsset(fileToHash);

            var arguments = HashCommand.Arguments(inputFile, decoder, "MD5", decoderParams);

            var result = await App.RunRaw(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdOut.Should().Equal(expectedHash, "the hash, and nothing else, must be written to std-out");
            });
        }

        [TestCase("MD5", SampleAsset.Sample1.ExpectedMd5)]
        [TestCase("SHA256", SampleAsset.Sample1.ExpectedSha256)]
        public async Task Hashing_a_file__produces_the_hash__using_the_specified_algorithm(string algorithm, string expectedHashString)
        {
            var expectedHash = Convert.FromHexString(expectedHashString);
            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Sample1.Flac.FileName);

            var arguments = HashCommand.Arguments(inputFile, TestEnvironment.GetFlacDecoder(), algorithm, HashCommand.FlacStreamDecoderParams);

            var result = await App.RunRaw(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdOut.Should().Equal(expectedHash);
            });
        }

        [TestCase("{hash}", SampleAsset.Sample1.ExpectedMd5)]
        [TestCase("{name}:{hash}", $"{SampleAsset.Sample1.Flac.FileName}:{SampleAsset.Sample1.ExpectedMd5}")]
        [TestCase("# {hash}", $"# {SampleAsset.Sample1.ExpectedMd5}")]
        public async Task Hashing_a_file__formats_the_hash_as_requested(string format, string expectedOutput)
        {
            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Sample1.Flac.FileName);

            var arguments = HashCommand.Arguments(inputFile, TestEnvironment.GetFlacDecoder(), "MD5", HashCommand.FlacStreamDecoderParams, format);

            var result = await App.Run(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdOut.TrimEnd().Should().Be(expectedOutput, "the hash must be rendered using the requested format");
            });
        }

        // Raw output carries no terminator at all, so this contract only applies to formatted text output
        [TestCase("{hash}", SampleAsset.Sample1.ExpectedMd5)]
        [TestCase("{name}:{hash}", $"{SampleAsset.Sample1.Flac.FileName}:{SampleAsset.Sample1.ExpectedMd5}")]
        public async Task Hashing_a_file__with_a_format__terminates_the_with_a_newline(string outputFormat, string expectedOutput)
        {
            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Sample1.Flac.FileName);

            var arguments = HashCommand.Arguments(inputFile, TestEnvironment.GetFlacDecoder(), "MD5", HashCommand.FlacStreamDecoderParams, outputFormat);

            var result = await App.Run(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdOut.Should().EndWith("\n", "consumers reading the stream need to know where the hash ends");
                result.StdOut.Length.Should().Be(expectedOutput.Length + 1, "the hash must be followed by exactly one line-terminator");
            });
        }

        [TestCase(null)]
        [TestCase("{hash}")]
        public async Task Hashing_a_file__reports_user_messages_on_stderr(string outputFormat)
        {
            const string algorithm = "MD5";

            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Sample1.Flac.FileName);

            var arguments = HashCommand.Arguments(inputFile, TestEnvironment.GetFlacDecoder(), algorithm, HashCommand.FlacStreamDecoderParams, outputFormat);

            var result = await App.Run(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdErr.Should().ContainEquivalentOf(algorithm, "user messaging belongs on std-error");
                result.StdErr.Should().ContainEquivalentOf("Done", "user messaging belongs on std-error");
            });
        }

        static IEnumerable<TestCaseData> GetDecodeAndHashCases()
        {
            var flac = TestEnvironment.GetFlacDecoder();
            yield return new TestCaseData(
                    SampleAsset.Sample1.Flac.FileName,
                    SampleAsset.Sample1.ExpectedMd5,
                    flac,
                    HashCommand.FlacStreamDecoderParams)
                .SetName("{m}(FLAC)(File 1)");

            yield return new TestCaseData(
                    SampleAsset.Sample2.Flac.FileName,
                    SampleAsset.Sample2.ExpectedMd5,
                    flac,
                    HashCommand.FlacStreamDecoderParams)
                .SetName("{m}(FLAC)(File 2)");

            var isLinux = OperatingSystem.IsLinux();

            var apeCase = new TestCaseData(
                    SampleAsset.Sample1.Ape.FileName,
                    SampleAsset.Sample1.ExpectedMd5,
                    !isLinux ? TestEnvironment.GetApeDecoder() : null,
                    new[] { "{file}", "-", "-d" })
                .SetName("{m}(APE)(File 1)");

            yield return !isLinux ? apeCase : apeCase.Ignore("Monkey's Audio (APE) decoder is not available on Linux");
        }
    }
}
