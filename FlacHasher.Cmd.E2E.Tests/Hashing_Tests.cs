using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    [TestFixture]
    public class Hashing_Tests
    {
        const string inputFileName = "sample.flac";

        // Pinned from the decoded output of TestAssets/sample.flac. See TestAssets/make-test-assets.ps1.
        const string expectedMd5 = "770ec9cbf2ff85a82670e10d807d82d1";

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

            TestEnvironment.GetTestAsset(inputFileName)
                .CopyTo(Path.Combine(workingDirectory.FullName, inputFileName));
        }

        [TearDown]
        public void Teardown()
        {
            workingDirectory?.Delete(recursive: true);
        }

        [Test]
        public async Task When_Computing_A_Hash_Of_A_File__Writes_The_Hash_To_Standard_Output()
        {
            var result = await App.Run(
                workingDirectory,
                "hash",
                $"--input={inputFileName}",
                "--algorithm=MD5",
                "--format={hash}",
                "--decoder-verbose=false");
            
            Assert.AreEqual(0, result.ExitCode, $"Standard error:\n{result.StdErr}");
            Assert.AreEqual(expectedMd5, result.StdOut.Trim());
        }
    }
}
