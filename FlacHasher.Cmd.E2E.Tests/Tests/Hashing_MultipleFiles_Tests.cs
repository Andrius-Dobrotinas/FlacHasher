using FluentAssertions;
using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    [TestFixture]
    public class Hashing_MultipleFiles_Tests
    {
        const int md5Length = 16;

        // Deliberately not in alphabetical order, so that an application that sorted its input would be caught out
        static readonly string[] filesToHash =
        {
            SampleAsset.Sample2.Flac.FileName,
            SampleAsset.Sample3.Flac.FileName,
            SampleAsset.Sample1.Flac.FileName
        };

        static readonly string[] expectedHashes =
        {
            SampleAsset.Sample2.ExpectedMd5,
            SampleAsset.Sample3.ExpectedMd5,
            SampleAsset.Sample1.ExpectedMd5
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
        public async Task Hashing_multiple_files__writes_one_hash_per_input_file()
        {
            var arguments = BuildHashArguments(filesToHash);

            var result = await App.RunRaw(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdOut.Length.Should().Be(filesToHash.Length * md5Length, "the hashes must be written back to back, with nothing in between them");
            });
        }

        [TestCaseSource(nameof(GetFileOrderCases))]
        public async Task Hashing_multiple_files__writes_the_hashes_in_the_order_the_files_were_given(string[] files, string[] expectedHashStrings)
        {
            var expected = expectedHashStrings.Select(Convert.FromHexString).ToArray();

            var arguments = BuildHashArguments(files);

            var result = await App.RunRaw(workingDirectory, arguments);

            // The hashes carry no file names, so their position is the only thing tying them to the files they were computed on
            var actualHashes = Enumerable.Range(0, files.Length)
                .Select(x => result.StdOut.Skip(x * md5Length).Take(md5Length).ToArray())
                .ToArray();

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                actualHashes.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
            });
        }

        [Test]
        public async Task Hashing_multiple_files__with_a_format__writes_one_line_per_input_file()
        {
            var arguments = BuildHashArguments(filesToHash, "{hash}");

            var result = await App.Run(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdOut.Should().EndWith("\n", "every line, the last one included, must be terminated");
                result.StdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries).Should().HaveCount(filesToHash.Length);
            });
        }

        [Test]
        public async Task Hashing_multiple_files__with_a_format__writes_every_hash_in_the_requested_format()
        {
            var arguments = BuildHashArguments(filesToHash, "{hash}");

            var result = await App.Run(workingDirectory, arguments);

            var lines = result.StdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                lines.Should().BeEquivalentTo(expectedHashes, options => options.WithStrictOrdering(), "every hash, not just the first one, must be rendered using the requested format");
            });
        }

        static string[] BuildHashArguments(string[] filesToHash, string outputFormat = null)
        {
            var inputFiles = filesToHash.Select(TestEnvironment.GetTestAsset);

            return HashCommand.Arguments(inputFiles, TestEnvironment.GetFlacDecoder(), "MD5", HashCommand.FlacStreamDecoderParams, outputFormat);
        }

        // One order alone cannot tell "emits in the order given" apart from "emits in some fixed order that happens to match"
        static IEnumerable<TestCaseData> GetFileOrderCases()
        {
            yield return new TestCaseData(filesToHash, expectedHashes)
                .SetName("{m}(Order 1)");

            yield return new TestCaseData(filesToHash.Reverse().ToArray(), expectedHashes.Reverse().ToArray())
                .SetName("{m}(Order 2)");
        }
    }
}
