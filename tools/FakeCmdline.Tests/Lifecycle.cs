using NUnit.Framework;
using System.Diagnostics;

namespace Andy.FakeCmdline
{
    /// <summary>
    /// Everything here is about ordering and timing rather than content, so the runs must not compete for the machine.
    /// </summary>
    [NonParallelizable]
    public class Lifecycle
    {
        /// <summary>
        /// Long enough for the process to start and do its work on a busy machine.
        /// </summary>
        const int generousWaitMs = 5000;

        /// <summary>
        /// The window within which something that is meant NOT to happen must not happen.
        /// Kept well below the waits the program is given, so that neither side of the comparison is a close call.
        /// </summary>
        const int shortWaitMs = 300;

        const string holdMs = "800";

        /// <summary>
        /// The one that matters most: if the program stopped closing stdout before lingering, tests of a process
        /// runner would quietly exercise its read-timeout path instead of its exit-timeout path.
        /// </summary>
        [Test]
        public void When__Lingering__Must_Send_EOF_BeforeExiting()
        {
            using (var run = LiveRun.Start("--file", TestPayload.SourceFile.FullName, "--linger", holdMs))
            {
                Assert.IsTrue(run.WaitForStdoutEof(generousWaitMs), "Standard output has to reach an EOF");

                // Waiting rather than just looking at the task: an EOF handed over by the exit itself would look the same at that instant
                Assert.IsFalse(run.ProcessTask.Wait(shortWaitMs), "The EOF has to arrive well before the process exits - an EOF that comes with the exit proves nothing");

                Assert.IsTrue(run.ProcessTask.Wait(generousWaitMs), "The process has to exit once it's done lingering");
            }
        }

        [Test]
        public void When__KeepingStdoutOpen__Must_Not_Send_EOF_UntilThatTimeElapses()
        {
            using (var run = LiveRun.Start("--file", TestPayload.SourceFile.FullName, "--keep-stdout-open", holdMs))
            {
                // Waiting for the payload first, so that the window below measures the holding rather than the start-up
                Assert.IsTrue(run.WaitForOutput(generousWaitMs), "The payload has to be written before stdout is held open");

                Assert.IsFalse(run.WaitForStdoutEof(shortWaitMs), "Standard output has to stay open for the time it was given");
                Assert.IsTrue(run.WaitForStdoutEof(generousWaitMs), "Standard output has to be let go of once that time has elapsed");
            }
        }

        [Test]
        public void When__KeepingStdoutOpen_Forever__Must_Not_Send_EOF()
        {
            using (var run = LiveRun.Start("--file", TestPayload.SourceFile.FullName, "--keep-stdout-open", "-1"))
            {
                Assert.IsTrue(run.WaitForOutput(generousWaitMs), "The payload has to be written before stdout is held open");

                // "Forever" can only be asserted within a window; the run is killed on the way out
                Assert.IsFalse(run.WaitForStdoutEof(shortWaitMs), "Standard output has to stay open indefinitely");
            }
        }

        /// <summary>
        /// The lingering sits outside the writing, so it has to be honoured on the sourceless path too - a test that
        /// wants a process which starts, does nothing and takes its time about quitting has no source to give it.
        /// </summary>
        [Test]
        public void When__LingeringWithoutASource__Must_Wait_BeforeExiting()
        {
            const int exitCode = 4;

            using (var run = LiveRun.Start("--linger", holdMs, "--exit-code", exitCode.ToString()))
            {
                // Syncing on the program's first word, so that the window below measures the lingering rather than the start-up
                Assert.IsTrue(run.WaitForStdErr(generousWaitMs), "The program has to say it has nothing to process before it starts lingering");

                Assert.IsFalse(run.ProcessTask.Wait(shortWaitMs), "The process has to wait even though it had nothing to write");

                Assert.IsTrue(run.ProcessTask.Wait(generousWaitMs), "The process has to exit once it's done lingering");
                Assert.AreEqual(exitCode, run.ProcessTask.Result.ExitCode, run.StdErr);
            }
        }

        [Test]
        public void When__LingeringForever__Must_Not_Exit_AfterSendingEOF()
        {
            using (var run = LiveRun.Start("--file", TestPayload.SourceFile.FullName, "--linger", "-1"))
            {
                Assert.IsTrue(run.WaitForStdoutEof(generousWaitMs), "Standard output has to be closed before the lingering starts");

                Assert.IsFalse(run.ProcessTask.Wait(shortWaitMs), "The process has to keep running indefinitely");
            }
        }

        [Test]
        public void When__TheChunkDelayIsForever__Must_Not_Write_Anything()
        {
            using (var run = LiveRun.Start("--file", TestPayload.SourceFile.FullName, "--output-chunk-delay", "-1"))
            {
                Assert.IsFalse(run.WaitForOutput(shortWaitMs), "Nothing has to come out of a write that never happens");
                Assert.IsFalse(run.StdoutHasReachedEof, "Standard output has to stay open");
            }
        }

        /// <summary>
        /// Coarse on purpose: the point is that the delay is paid for every chunk rather than once.
        /// </summary>
        [Test]
        public void When__AChunkDelayIsGiven__Must_Apply_It_ToEveryChunk()
        {
            const int delayMs = 100;
            const int chunkSize = 52; // 256 bytes make 5 chunks of this size, the last one short
            const int chunkCount = 5;

            var stopwatch = Stopwatch.StartNew();

            using (var run = LiveRun.Start(
                "--file", TestPayload.SourceFile.FullName,
                "--output-chunk-size", chunkSize.ToString(),
                "--output-chunk-delay", delayMs.ToString()))
            {
                Assert.IsTrue(run.WaitForOutput(generousWaitMs), "The first chunk has to come out");
                var firstChunkAtMs = stopwatch.ElapsedMilliseconds;

                Assert.IsTrue(run.WaitForStdoutEof(generousWaitMs), "Every chunk has to come out eventually");
                var lastChunkAtMs = stopwatch.ElapsedMilliseconds;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(TestPayload.Bytes, run.Bytes, "Delaying must not cost any bytes");

                    Assert.Greater(lastChunkAtMs, chunkCount * delayMs - delayMs,
                        "The whole run has to take about one delay per chunk");

                    // Measured from the first chunk, so that a slow start-up can't be mistaken for a delay
                    Assert.Greater(lastChunkAtMs - firstChunkAtMs, (chunkCount - 1) * delayMs / 2,
                        "The chunks after the first one have to be delayed too, not just the first");
                });
            }
        }
    }
}
