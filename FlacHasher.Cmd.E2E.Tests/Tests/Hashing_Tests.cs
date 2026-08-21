using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    [TestFixture]
    public class Hashing_Tests
    {
        DirectoryInfo workingDirectory;

        FileInfo decoder;

        [SetUp]
        public void Setup()
        {
            decoder = TestEnvironment.GetDecoderOrFailTest();

            workingDirectory = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"flachash-e2e-{Guid.NewGuid():N}"));

            // The app requires this file to exist, but none of the settings used by this test come from it.
            File.WriteAllText(
                Path.Combine(workingDirectory.FullName, "settings.ini"),
                "");

            TestEnvironment.GetTestAsset(SampleAsset.Flac1.FileName)
                .CopyTo(Path.Combine(workingDirectory.FullName, SampleAsset.Flac1.FileName));
        }

        [TearDown]
        public void Teardown()
        {
            workingDirectory?.Delete(recursive: true);
        }

        [Test]
        public async Task Must_Compute_Hash_For_A_File__And_Write_To_StdOut()
        {
            var result = await App.Run(
                workingDirectory,
                "hash",
                $"--input={SampleAsset.Flac1.FileName}",
                "--algorithm=MD5",
                "--format={hash}",
                $"--decoder={decoder.FullName}",
                "--process-timeout=30",
                "--decoder-verbose=false");
            
            Assert.AreEqual(0, result.ExitCode, $"Standard error:\n{result.StdErr}");
            Assert.AreEqual(SampleAsset.Flac1.ExpectedMd5, result.StdOut.Trim());
        }
    }
}
