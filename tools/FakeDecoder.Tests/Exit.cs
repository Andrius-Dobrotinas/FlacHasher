using NUnit.Framework;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Andy.FakeDecoder
{
    public class Exit
    {
        const string successMessage = "IT-WENT-WELL";
        const string errorMessage = "IT-WENT-WRONG";

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(42)]
        public async Task When__AnExitCodeIsGiven__Must_Return_ExactlyThatCode(int exitCode)
        {
            var result = await App.Run("--file", TestPayload.SourceFile.FullName, "--exit-code", exitCode.ToString());

            Assert.AreEqual(exitCode, result.ExitCode, result.StdErr);
        }

        [Test]
        [Platform(Include = "Win")]
        public async Task When__ExitCodeIsNegative__Must_Return_ItVerbatim_On_Windows()
        {
            var result = await App.Run("--exit-code", "-1");

            Assert.AreEqual(-1, result.ExitCode, result.StdErr);
        }

        /// <summary>
        /// Unix keeps only the low 8 bits of an exit status, so -1 comes back as 255.
        /// </summary>
        [Test]
        [Platform(Exclude = "Win")]
        public async Task When__ExitCodeIsNegative__Must_Return_ItTruncated_ToLowByte_On_Unix()
        {
            var result = await App.Run("--exit-code", "-1");

            Assert.AreEqual(255, result.ExitCode, result.StdErr);
        }

        [Test]
        public async Task When__ExitCodeIsZero__Must_Write_TheSuccessMessage_And_Not_TheErrorMessage()
        {
            var result = await App.Run(
                "--exit-code", "0",
                "--success-message", successMessage,
                "--error-message", errorMessage);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.That(result.StdErr, Does.Contain(successMessage));
                Assert.That(result.StdErr, Does.Not.Contain(errorMessage));
            });
        }

        [Test]
        public async Task When__ExitCodeIsNonZero__Must_Write_TheErrorMessage_And_Not_TheSuccessMessage()
        {
            var result = await App.Run(
                "--exit-code", "9",
                "--success-message", successMessage,
                "--error-message", errorMessage);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(9, result.ExitCode, result.StdErr);
                Assert.That(result.StdErr, Does.Contain(errorMessage));
                Assert.That(result.StdErr, Does.Not.Contain(successMessage));
            });
        }

        [Test]
        public async Task When__ThereIsNoSource__Must_Write_NothingToProcess_And_Return_TheGivenExitCode()
        {
            var result = await App.Run("--exit-code", "5");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(5, result.ExitCode, result.StdErr);
                Assert.That(result.StdErr, Does.Contain("Nothing to process"));
                Assert.IsEmpty(result.StdOut);
            });
        }

        /// <summary>
        /// A consumer that walks off mid-write breaks the pipe, and an unhandled write failure would hand the caller a
        /// runtime-chosen exit code instead of the requested one.
        /// Run through System.Diagnostics.Process rather than <see cref="App"/>: CliWrap owns the pipe it reads from
        /// and gives a test no way to let go of the read end while the program is still writing.
        /// </summary>
        [Test]
        public void When__TheConsumerAbandonsThePipe_MidWrite__Must_Still_Return_TheGivenExitCode()
        {
            const int exitCode = 6;

            var startInfo = new ProcessStartInfo(TestEnvironment.Executable.FullName)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            // A byte at a time with a pause before each: whatever the pipe's capacity, there are writes left to break
            foreach (var argument in new[]
                {
                    "--file", TestPayload.SourceFile.FullName,
                    "--read-chunk-size", "1",
                    "--output-chunk-delay", "50",
                    "--exit-code", exitCode.ToString()
                })
                startInfo.ArgumentList.Add(argument);

            using (var process = Process.Start(startInfo))
            {
                // It's a write that has to break, so there has to have been one before the read end goes
                Assert.AreNotEqual(-1, process.StandardOutput.BaseStream.ReadByte(), "The program has to be writing before the pipe is abandoned");

                process.StandardOutput.Close();

                Assert.IsTrue(process.WaitForExit(App.TimeoutMs), "The program has to exit rather than hang on a pipe nobody's reading");
                Assert.AreEqual(exitCode, process.ExitCode, "The exit code has to be the one that was asked for, not one the runtime picked");
            }
        }

        [Test]
        public async Task When__AProgressMessageIsGiven__Must_Write_It_OncePerChunk()
        {
            const string progressMessage = "CHUNK-WENT-OUT";
            const int chunkSize = 64;
            var expectedChunkCount = TestPayload.Bytes.Length / chunkSize;

            var result = await App.Run(
                "--file", TestPayload.SourceFile.FullName,
                "--read-chunk-size", chunkSize.ToString(),
                "--progress-message", progressMessage);

            var occurrences = result.StdErr.Split(progressMessage).Length - 1;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                Assert.AreEqual(expectedChunkCount, occurrences, "The progress message has to be written once per chunk");
            });
        }

        [Test]
        public async Task When__ProgressAndExitMessagesAreBothWritten__Must_Write_TheExitMessage_Last()
        {
            const string progressMessage = "CHUNK-WENT-OUT";

            var result = await App.Run(
                "--file", TestPayload.SourceFile.FullName,
                "--read-chunk-size", "64",
                "--progress-message", progressMessage,
                "--success-message", successMessage);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.ExitCode, result.StdErr);
                // Without this the comparison below would hold on a -1 from a progress message that was never written
                Assert.That(result.StdErr, Does.Contain(progressMessage));
                Assert.Greater(
                    result.StdErr.IndexOf(successMessage),
                    result.StdErr.LastIndexOf(progressMessage),
                    "The exit message has to come after the last of the progress");
            });
        }
    }
}
