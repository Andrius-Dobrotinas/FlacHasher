using NUnit.Framework;
using Moq;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    [NonParallelizable]
    public class ErrorHandling
    {
        [TestCase(100)]
        [TestCase(300)]
        public void When_StdErrIsRedirected_But_NotResponding__And_ProcessExits_With_ErrorExitCode__UponReadingTheWholeOutputStream__Must_AbortReadingStdErr_AfterTimeout_And_DisposeOfTheProcess(int timoeut)
        {
            var target = new ProcessRunner(-1, timoeut, 0, false);

            using (var stderrReadSignal = new AutoResetEvent(false))
            {
                var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner"));
                var stderr = new SignalWaitingMemoryStream(Encoding.UTF8.GetBytes("Here's some error for ya!"), stderrReadSignal, maxReadSize: 8);
                var process = new ExternalProcessFake(stdout: stdout, stdin: null, stderr: stderr, exitCode: -1);

                var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: true);

                stderrReadSignal.Set();

                Assert.Throws<ExecutionException>(() => Util.Read(outputStream));
                Assert.True(process.IsDisposedOf, "Still must dispose of the process");
            }
        }

        /* A process that has to be killed reports the code the OS terminated it with, never the one it would have exited with,
         * so there is no error to report on its behalf */
        /* Kill only requests termination; the process is still alive when it returns, and its exit code is unavailable until it actually dies.
         * No real program can refuse to die, which is why this one needs a fake process. */
        [Test]
        public void When_Process_HasToBeKilledOnExit_But_StaysAlive_AfterBeingKilled__Must_Not_Ask_ForItsExitCode()
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"));
            var process = new ExternalProcessFake(stdout: stdout, stdin: null, respondToExitRequest: false, exitOnKillRequest: false);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process);

            // Asking a process that hasn't died yet for its exit code throws, so reporting anything else would mean having asked
            Assert.Throws<ProcessNotRespondingException>(() => Util.Read(outputStream));

            Assert.True(process.IsKillRequested, "The process has to be killed");
            Assert.False(process.HasExited, "The test has to keep the process alive for it to be meaningful");
            Assert.True(process.IsDisposedOf);
        }

        [Test]
        public void When_StdErrIsRedirected_But_ReadingItThrows__And_ProcessExits_With_ErrorExitCode__UponReadingTheWholeOutputStream__Must_Throw_ExecutionException_With_IsProcessOutputCaptured_True()
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"));
            var stderr = new ThrowingReadStream(Encoding.UTF8.GetBytes("Here's some error for ya!"));
            var process = new ExternalProcessFake(stdout: stdout, stdin: null, stderr: stderr, exitCode: -1);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: true);

            var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            // Stderr was redirected (and would've been used), even though reading it failed
            Assert.True(exception.IsProcessOutputCaptured);
        }
    }
}