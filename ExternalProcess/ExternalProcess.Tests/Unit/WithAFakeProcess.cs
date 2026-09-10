using NUnit.Framework;
using System.IO;
using System.Text;
using System.Threading;

namespace Andy.ExternalProcess.Unit
{
    /// <summary>
    /// The handful of behaviours a real process cannot be made to produce, and which therefore have to be
    /// staged with a fake one. Everything else about the runner is driven by a real process, on purpose:
    /// a fake can be made to do things no operating system would ever do, and tests written against one
    /// end up asserting the fake's habits rather than the runner's.
    /// </summary>
    [NonParallelizable]
    public class WithAFakeProcess
    {
        const string outputText = "Alright, partner, you know what time it is. Let's keep on rolling!";
        const string errorText = "Here's some error for ya!";

        /// <summary>
        /// Kill only asks a process to terminate; it is still alive when the call returns, and its exit code
        /// is unavailable until it actually dies. No real program can be held in that state on demand -
        /// SIGKILL cannot be refused and TerminateProcess cannot be declined.
        /// </summary>
        [Test]
        public void When_TheProcess_StaysAlive_AfterBeingKilled__Must_Not_Ask_ForItsExitCode()
        {
            var target = TestRunner.WithTimeoutInSeconds(-1);

            var process = new ExternalProcessFake(
                stdout: new MemoryStream(Encoding.UTF8.GetBytes(outputText)),
                stdin: null,
                respondToExitRequest: false,
                exitOnKillRequest: false);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process);

            // Asking a process that hasn't died yet for its exit code throws, so reporting anything else would mean having asked
            Assert.Throws<ProcessNotRespondingException>(() => Util.Read(outputStream));

            Assert.True(process.IsKillRequested, "The process has to be killed");
            Assert.False(process.HasExited, "The test has to keep the process alive for it to be meaningful");
            Assert.True(process.IsDisposedOf);
        }

        /// <summary>
        /// A failure of the reading rather than of the process, so it takes a stream that misbehaves rather than a program that does.
        /// </summary>
        [Test]
        public void When_ReadingStdErr_Throws__Must_Still_Report_TheProcess_Failure()
        {
            var target = TestRunner.WithTimeoutInSeconds(-1);

            var process = new ExternalProcessFake(
                stdout: new MemoryStream(Encoding.UTF8.GetBytes(outputText)),
                stdin: null,
                stderr: new ThrowingReadStream(Encoding.UTF8.GetBytes(errorText)),
                exitCode: -1);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: true);

            var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            // Stderr was redirected and would have been reported; that the reading of it came to nothing doesn't change what the flag says
            Assert.True(exception.IsProcessOutputCaptured);
        }

        /// <summary>
        /// On Unix, releasing the error stream while a read is in progress makes that read fail rather than end,
        /// which leaves the reading half-done. What the process said before that is the only explanation of the
        /// failure anyone gets, so it has to reach them.
        /// </summary>
        [Test]
        public void When_ReadingStdErr_Throws_PartWayThrough__TheException_Must_Carry_WhatWasRead_UpToThatPoint()
        {
            const int deliveredBytes = 8;
            var target = TestRunner.WithTimeoutInSeconds(-1, exitTimeoutMs: 2000);

            var process = new ExternalProcessFake(
                stdout: new MemoryStream(Encoding.UTF8.GetBytes(outputText)),
                stdin: null,
                stderr: new ThrowingReadStream(Encoding.UTF8.GetBytes(errorText), readsBeforeThrowing: 1, maxReadSize: deliveredBytes),
                exitCode: -1);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: true);

            var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            Assert.True(exception.IsProcessOutputCaptured);
            Assert.AreEqual(errorText.Substring(0, deliveredBytes), exception.ProcessErrorOutput);
        }

        /// <summary>
        /// A stream that never answers a read. A real process' error stream comes to an end when it exits,
        /// so nothing a real one does leaves the reading outstanding at the point of reporting.
        /// </summary>
        [TestCase(100)]
        [TestCase(300)]
        public void When_ReadingStdErr_NeverFinishes__Must_Still_Report_TheProcess_Failure(int exitTimeoutMs)
        {
            var target = TestRunner.WithTimeoutInSeconds(-1, exitTimeoutMs: exitTimeoutMs);

            using (var stderrReadSignal = new AutoResetEvent(false))
            {
                var stderr = new SignalWaitingMemoryStream(Encoding.UTF8.GetBytes(errorText), stderrReadSignal, maxReadSize: 8);
                var process = new ExternalProcessFake(
                    stdout: new MemoryStream(Encoding.UTF8.GetBytes(outputText)),
                    stdin: null,
                    stderr: stderr,
                    exitCode: -1);

                var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: true);

                stderrReadSignal.Set();

                Assert.Throws<ExecutionException>(() => Util.Read(outputStream));
                Assert.True(process.IsDisposedOf, "Still must dispose of the process");
            }
        }
    }
}
