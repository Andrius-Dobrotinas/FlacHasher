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
            var decoder = TestEnvironment.GetDecoderOrFailTest();

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

            Assert.AreEqual(0, result.ExitCode, $"Standard error:\n{result.StdErr}");
            Assert.AreEqual(SampleAsset.Flac1.ExpectedMd5, result.StdOut.Trim());
        }
    }
}
