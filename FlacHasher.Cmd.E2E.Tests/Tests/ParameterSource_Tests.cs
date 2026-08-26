using FluentAssertions;
using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    [TestFixture]
    public class ParameterSource_Tests
    {
        DirectoryInfo workingDirectory;

        [SetUp]
        public void Setup()
        {
            var decoder = TestEnvironment.GetFlacDecoder();

            workingDirectory = TestEnvironment.SetUpWorkingDirWithSettingsFile(
                $"""
                ProcessTimeoutSec=30

                [Decoder]
                Decoder={decoder.FullName}
                """);
        }

        [TearDown]
        public void Teardown()
        {
            workingDirectory?.Delete(recursive: true);
        }

        [Test]
        public async Task Must_Use_Decoder_From_Settings_File__When_Not_Specified_On_Cmdline()
        {
            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Flac1.FileName);

            var result = await App.Run(
                workingDirectory,
                "hash",
                $"--input={inputFile.FullName}",
                "--algorithm=MD5",
                "--format={hash}",
                "--decoder-verbose=false");

            result.ExitCode.Should().Be(0, $"the process must have run successfully for std-error to be meaningful; standard error was:\n{result.StdErr}");
            result.StdOut.Trim().Should().Be(SampleAsset.Flac1.ExpectedMd5);
        }
    }
}
