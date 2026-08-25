using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    [TestFixture]
    public class Hashing_Tests
    {
        DirectoryInfo workingDirectory;

        [SetUp]
        public void Setup()
        {
            // The application requires the settings file to exist, but I want to limit testing surface here - hence a directory with an empty settings file
            workingDirectory = TestEnvironment.SetUpWorkingDirWithSettingsFile();
        }

        [TearDown]
        public void Teardown()
        {
            workingDirectory?.Delete(recursive: true);
        }

        [Test]
        public async Task Must_Compute_Hash_For_A_File__And_Write_To_StdOut()
        {
            var decoder = TestEnvironment.GetFlacDecoder();
            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Flac1.FileName);

            var result = await App.Run(
                workingDirectory,
                "hash",
                $"--input={inputFile.FullName}",
                "--algorithm=MD5",
                "--format={hash}",
                $"--decoder={decoder.FullName}",
                "--process-timeout=30",
                "--decoder-verbose=false");
            
            Assert.AreEqual(0, result.ExitCode, $"Standard error:\n{result.StdErr}");
            Assert.AreEqual(SampleAsset.Flac1.ExpectedMd5, result.StdOut.Trim());
        }

        [Test]
        [Platform(Exclude = "Linux", Reason = "Monkey's Audio (APE) decoder is not available on Linux")]
        public async Task Must_Compute_Hash_For_An_Ape_File__And_Write_To_StdOut()
        {
            var apeDecoder = TestEnvironment.GetApeDecoder();
            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Ape1.FileName);

            var result = await App.Run(
                workingDirectory,
                "hash",
                $"--input={inputFile.FullName}",
                "--algorithm=MD5",
                "--format={hash}",
                $"--decoder={apeDecoder.FullName}",
                "--params={file}",
                "--params=-", // write to std-out
                "--params=-d",
                "--process-timeout=30",
                "--decoder-verbose=false");

            Assert.AreEqual(0, result.ExitCode, $"Standard error:\n{result.StdErr}");
            Assert.AreEqual(SampleAsset.Ape1.ExpectedMd5, result.StdOut.Trim());
        }
    }
}
