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

            workingDirectory = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"flachash-e2e-{Guid.NewGuid():N}"));

            File.WriteAllText(
                Path.Combine(workingDirectory.FullName, "settings.ini"),
                $"ProcessTimeoutSec=30\n\n[Decoder]\nDecoder={decoder.FullName}\n");

            TestEnvironment.GetTestAsset(SampleAsset.Flac1.FileName)
                .CopyTo(Path.Combine(workingDirectory.FullName, SampleAsset.Flac1.FileName));
        }

        [TearDown]
        public void Teardown()
        {
            workingDirectory?.Delete(recursive: true);
        }

        [Test]
        public async Task Must_Use_Decoder_From_Settings_File__When_Not_Specified_On_Cmdline()
        {
            var result = await App.Run(
                workingDirectory,
                "hash",
                $"--input={SampleAsset.Flac1.FileName}",
                "--algorithm=MD5",
                "--format={hash}",
                "--decoder-verbose=false");

            Assert.AreEqual(0, result.ExitCode, $"Standard error:\n{result.StdErr}");
            Assert.AreEqual(SampleAsset.Flac1.ExpectedMd5, result.StdOut.Trim());
        }
    }
}
