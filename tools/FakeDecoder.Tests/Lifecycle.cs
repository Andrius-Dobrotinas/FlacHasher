using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Andy.FakeDecoder
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

        /// <summary>
        /// Reading a pipe takes a different path through the program than reading a file, and the EOF has to come
        /// ahead of the exit on that path too.
        /// </summary>
        [Test]
        public void When__LingeringWhileReadingStdin__Must_Send_EOF_BeforeExiting()
        {
            using (var run = LiveRun.Start(TestPayload.Bytes, "--stdin", "--linger", holdMs))
            {
                Assert.IsTrue(run.WaitForStdoutEof(generousWaitMs), "Standard output has to reach an EOF");

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
        public void When__TheWriteDelayIsForever__Must_Not_Write_Anything()
        {
            using (var run = LiveRun.Start("--file", TestPayload.SourceFile.FullName, "--write-delay", "-1"))
            {
                Assert.IsFalse(run.WaitForOutput(shortWaitMs), "Nothing has to come out of a write that never happens");
                Assert.IsFalse(run.StdoutHasReachedEof, "Standard output has to stay open");
            }
        }

        /// <summary>
        /// Coarse on purpose: the point is that the delay is paid for every write rather than once.
        /// </summary>
        [Test]
        public void When__AWriteDelayIsGiven__Must_Apply_It_ToEveryWrite()
        {
            const int delayMs = 100;
            const int readChunkSize = 52;
            // 256 bytes make 5 reads of that size, the last one short, and every read makes a write
            const int writeCount = 5;

            var stopwatch = Stopwatch.StartNew();

            using (var run = LiveRun.Start(
                "--file", TestPayload.SourceFile.FullName,
                "--read-chunk-size", readChunkSize.ToString(),
                "--write-delay", delayMs.ToString()))
            {
                Assert.IsTrue(run.WaitForOutput(generousWaitMs), "The first write has to come out");
                var firstWriteAtMs = stopwatch.ElapsedMilliseconds;

                Assert.IsTrue(run.WaitForStdoutEof(generousWaitMs), "Every write has to come out eventually");
                var lastWriteAtMs = stopwatch.ElapsedMilliseconds;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(TestPayload.Bytes, run.Bytes, "Delaying must not cost any bytes");

                    Assert.Greater(lastWriteAtMs, writeCount * delayMs - delayMs,
                        "The whole run has to take about one delay per write");

                    // Measured from the first write, so that a slow start-up can't be mistaken for a delay
                    Assert.Greater(lastWriteAtMs - firstWriteAtMs, (writeCount - 1) * delayMs / 2,
                        "The writes after the first one have to be delayed too, not just the first");
                });
            }
        }

        /// <summary>
        /// The rest of the source is deliberately left unread on an early finish, so that whoever is feeding stdin
        /// is left writing into a pipe nobody holds - the broken pipe a real `head` hands the command feeding it.
        /// Draining the remainder instead would take that away and nothing else would notice.
        /// Run through System.Diagnostics.Process rather than <see cref="App"/>: this test has to own the write end
        /// of the pipe and keep pushing at it, which CliWrap gives it no way to do.
        /// </summary>
        [Test]
        public void When__FinishingEarly_WithStdin__Must_Break_TheProducersNextWrite()
        {
            var startInfo = new ProcessStartInfo(TestEnvironment.Executable.FullName)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            foreach (var argument in new[] { "--stdin", "--read-chunk-size", "1", "--finish-after-reads", "1" })
                startInfo.ArgumentList.Add(argument);

            using (var process = Process.Start(startInfo))
            {
                var producer = Task.Run(() => WriteUntilItBreaks(process.StandardInput.BaseStream));

                // A write into a full pipe blocks instead of returning, so the byte cap inside the loop can't be the only guard
                if (!producer.Wait(generousWaitMs))
                {
                    process.Kill();
                    Assert.Fail("The program has to finish and let go of stdin rather than leave the producer blocked on a write");
                }

                Assert.IsInstanceOf<IOException>(
                    producer.Result,
                    "Writing to a program that has finished early and gone has to fail");            }
        }

        /// <summary>
        /// The pipe swallows a bufferful before a write can fail, so it takes far more than one write to reach one.
        /// The failure is handed back rather than thrown, so that waiting for the writing to end can't be tripped up by it.
        /// </summary>
        static IOException WriteUntilItBreaks(Stream producer)
        {
            // Well past any pipe's capacity, so that a program which drains its source runs out of rope rather than out of time
            const int capBytes = 8 * 1024 * 1024;
            var buffer = new byte[4096];

            try
            {
                for (int written = 0; written < capBytes; written += buffer.Length)
                {
                    producer.Write(buffer, 0, buffer.Length);
                    producer.Flush();
                }
            }
            catch (IOException exception)
            {
                return exception;
            }

            return null;
        }
    }
}
