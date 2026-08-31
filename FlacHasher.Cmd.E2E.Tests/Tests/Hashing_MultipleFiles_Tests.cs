using FluentAssertions;
using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    [TestFixture]
    public class Hashing_MultipleFiles_Tests
    {
        public record SampleFile(string FileName, string ExpectedMd5);

        const int md5Length = 16;

        // Deliberately not in alphabetical order, so that an application that sorted its input would be caught out
        static readonly SampleFile[] flacSet =
        {
            new(SampleAsset.Sample2.Flac.FileName, SampleAsset.Sample2.ExpectedMd5),
            new(SampleAsset.Sample3.Flac.FileName, SampleAsset.Sample3.ExpectedMd5),
            new(SampleAsset.Sample1.Flac.FileName, SampleAsset.Sample1.ExpectedMd5)
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
            var arguments = BuildHashArguments(flacSet);

            var result = await App.RunRaw(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdOut.Length.Should().Be(flacSet.Length * md5Length, "the hashes must be written back to back, with nothing in between them");
            });
        }

        [TestCaseSource(nameof(GetFileOrderCases))]
        public async Task Hashing_multiple_files__writes_the_hashes_in_the_order_the_files_were_given(SampleFile[] filesToHash)
        {
            var expectedHashes = filesToHash.Select(x => Convert.FromHexString(x.ExpectedMd5)).ToArray();

            var arguments = BuildHashArguments(filesToHash);

            var result = await App.RunRaw(workingDirectory, arguments);

            // The hashes carry no file names, so their position is the only thing tying them to the files they were computed on
            var actualHashes = Enumerable.Range(0, filesToHash.Length)
                .Select(x => result.StdOut.Skip(x * md5Length).Take(md5Length).ToArray())
                .ToArray();

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                actualHashes.Should().BeEquivalentTo(expectedHashes, options => options.WithStrictOrdering());
            });
        }

        [Test]
        public async Task Hashing_multiple_files__with_a_format__writes_one_line_per_input_file()
        {
            var arguments = BuildHashArguments(flacSet, "{hash}");

            var result = await App.Run(workingDirectory, arguments);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                result.StdOut.Should().EndWith("\n", "every line, the last one included, must be terminated");
                result.StdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries).Should().HaveCount(flacSet.Length);
            });
        }

        [Test]
        public async Task Hashing_multiple_files__with_a_format__writes_every_hash_in_the_requested_format()
        {
            var expectedHashes = flacSet.Select(x => x.ExpectedMd5).ToArray();

            var arguments = BuildHashArguments(flacSet, "{hash}");

            var result = await App.Run(workingDirectory, arguments);

            var lines = result.StdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.Multiple(() =>
            {
                result.ExitCode.Should().Be(0, $"the process must return a non-error code; standard error was:\n{result.StdErr}");

                lines.Should().BeEquivalentTo(expectedHashes, options => options.WithStrictOrdering(), "every hash, not just the first one, must be rendered using the requested format");
            });
        }

        static string[] BuildHashArguments(SampleFile[] filesToHash, string outputFormat = null)
        {
            var inputFiles = filesToHash.Select(x => TestEnvironment.GetTestAsset(x.FileName));

            return HashCommand.Arguments(inputFiles, TestEnvironment.GetFlacDecoder(), "MD5", HashCommand.FlacStreamDecoderParams, outputFormat);
        }

        // One order alone cannot tell "emits in the order given" apart from "emits in some fixed order that happens to match"
        static IEnumerable<TestCaseData> GetFileOrderCases()
        {
            yield return new TestCaseData((object)flacSet)
                .SetName("{m}(Order 1)");

            yield return new TestCaseData((object)flacSet.Reverse().ToArray())
                .SetName("{m}(Order 2)");
        }
    }
}
