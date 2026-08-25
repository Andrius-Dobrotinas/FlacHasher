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

        [TestCaseSource(nameof(GetHashingTestCases))]
        public async Task Hashing_a_file_writes_the_Hash_to_StdOut(string fileToHash, string expectedHashString, FileInfo decoder, string[] decoderArguments)
        {
            var inputFile = TestEnvironment.GetTestAsset(fileToHash);

            var arguments = new List<string>
            {
                "hash",
                $"--input={inputFile.FullName}",
                $"--decoder={decoder.FullName}",
                "--algorithm=MD5",
                "--process-timeout=30",
                "--decoder-verbose=false"
            };
            arguments.AddRange(decoderArguments);

            var result = await App.RunRaw(workingDirectory, arguments.ToArray());

            var expectedHash = Convert.FromHexString(expectedHashString);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedHash, result.StdOut.Take(expectedHash.Length).ToArray(), "Output the hash to Std-out");
                Assert.AreEqual((byte)'\n', result.StdOut.Last(), "Indicate the end of the hash with a new-line");
                Assert.AreEqual(expectedHash.Length + 1, result.StdOut.Length, "Not output anything else to the Std-out");

                Assert.AreEqual(0, result.ExitCode, $"Exit Code. Standard error output:\n{result.StdErr}");
            });
        }

        [TestCaseSource(nameof(GetHashingTestCases))]
        public async Task Hashing_a_file_writes_the_Hash_to_StdOut__in_the_requested_format(string fileToHash, string expectedHash, FileInfo decoder, string[] decoderArguments)
        {
            var inputFile = TestEnvironment.GetTestAsset(fileToHash);

            var arguments = new List<string>
            {
                "hash",
                $"--input={inputFile.FullName}",
                $"--decoder={decoder.FullName}",
                "--algorithm=MD5",
                "--format={hash}",
                "--process-timeout=30",
                "--decoder-verbose=false"
            };
            arguments.AddRange(decoderArguments);

            var result = await App.Run(workingDirectory, arguments.ToArray());

            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedHash, result.StdOut.Trim(), "Output the hash to Std-out");
                Assert.That(result.StdOut.EndsWith(Environment.NewLine), "Append a new-line to the hash");
                Assert.AreEqual(expectedHash.Length + Environment.NewLine.Length, result.StdOut.Length, "Not output anything else to the Std-out");

                Assert.AreEqual(0, result.ExitCode, $"Exit Code. Standard error output:\n{result.StdErr}");
            });
        }

        [TestCase()]
        [TestCase("--format={hash}")]
        public async Task Hashing_a_file__reflects_hashing_algorithm_in_std_Err__Regardless_of_formatting(params string[] decoderArguments)
        {
            var decoder = TestEnvironment.GetFlacDecoder();
            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Flac1.FileName);

            var arguments = new List<string>
            {
                "hash",
                $"--input={inputFile.FullName}",
                $"--decoder={decoder.FullName}",
                "--algorithm=MD5",
                "--process-timeout=30",
                "--decoder-verbose=false"
            };
            arguments.AddRange(decoderArguments);

            var result = await App.Run(workingDirectory, arguments.ToArray());

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, $"Exit Code. Standard error output:\n{result.StdErr}");
                Assert.That(result.StdErr.Contains("MD5"));
            });
        }

        static IEnumerable<TestCaseData> GetHashingTestCases()
        {
            yield return new TestCaseData(
                    SampleAsset.Flac1.FileName,
                    SampleAsset.Flac1.ExpectedMd5,
                    TestEnvironment.GetFlacDecoder(),
                    Array.Empty<string>())
                .SetName("{m}(FLAC)");

            var isLinux = OperatingSystem.IsLinux();

            var apeCase = new TestCaseData(
                    SampleAsset.Ape1.FileName,
                    SampleAsset.Ape1.ExpectedMd5,
                    !isLinux ? TestEnvironment.GetApeDecoder() : null,
                    new[] { "--params={file}", "--params=-", "--params=-d" })
                .SetName("{m}(APE)");

            yield return !isLinux ? apeCase : apeCase.Ignore("Monkey's Audio (APE) decoder is not available on Linux");
        }
    }
}
