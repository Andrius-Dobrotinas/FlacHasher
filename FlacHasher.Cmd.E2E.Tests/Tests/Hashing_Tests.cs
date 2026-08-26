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
            arguments.AddRange(decoderArguments.Select(x => $"--params={x}"));

            var result = await App.RunRaw(workingDirectory, arguments.ToArray());

            var expectedHash = Convert.FromHexString(expectedHashString);

            Assert.Multiple(() =>
            {
                result.StdOut.Take(expectedHash.Length).ToArray().Should().Equal(expectedHash, "the hash must be written to std-out");
                result.StdOut.Last().Should().Be((byte)'\n', "the hash must end with a new-line");
                result.StdOut.Length.Should().Be(expectedHash.Length + 1, "nothing else should be written to std-out");

                result.ExitCode.Should().Be(0, $"the must process return a non-error code; standard error was:\n{result.StdErr}");
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
            arguments.AddRange(decoderArguments.Select(x => $"--params={x}"));

            var result = await App.Run(workingDirectory, arguments.ToArray());

            Assert.Multiple(() =>
            {
                result.StdOut.Trim().Should().Be(expectedHash, "the hash must be written to std-out");
                result.StdOut.Should().EndWith(Environment.NewLine, "the hash must end with a new-line");
                result.StdOut.Length.Should().Be(expectedHash.Length + Environment.NewLine.Length, "nothing else should be written to std-out");

                result.ExitCode.Should().Be(0, $"the must process return a non-error code; standard error was:\n{result.StdErr}");
            });
        }

        [TestCase()]
        [TestCase("{hash}")]
        [TestCase("{hash} SHA256")]
        [TestCase("{file}{hash}")]
        [TestCase("# {file}{hash}")]
        [TestCase("{file}{hash} SHA256")]
        public async Task Hashing_a_file__reflects_hashing_algorithm_in_std_Err__Regardless_of_formatting(params string[] format)
        {
            var decoder = TestEnvironment.GetFlacDecoder();
            var inputFile = TestEnvironment.GetTestAsset(SampleAsset.Flac1.FileName);
            const string algo = "MD5";

            var arguments = new List<string>
            {
                "hash",
                $"--input={inputFile.FullName}",
                $"--decoder={decoder.FullName}",
                $"--algorithm={algo}",
                "--process-timeout=30",
                "--decoder-verbose=false"
            };
            arguments.AddRange(format.Select(x => $"--format={x}"));

            var result = await App.Run(workingDirectory, arguments.ToArray());

            result.ExitCode.Should().Be(0, $"the process must have run successfully for std-error to be meaningful; standard error was:\n{result.StdErr}");
            result.StdErr.Should().Contain(algo, "the hashing algorithm should be reported on std-error");
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
                    new[] { "{file}", "-", "-d" })
                .SetName("{m}(APE)");

            yield return !isLinux ? apeCase : apeCase.Ignore("Monkey's Audio (APE) decoder is not available on Linux");
        }
    }
}
