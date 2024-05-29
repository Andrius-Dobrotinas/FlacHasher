using Andy.FlacHash.ExternalProcess;
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

namespace Andy.FlacHash.IO.ExternalProcess.ProcessRunner_Tests
{
    [NonParallelizable]
    public class OnCancellation
    {
        [TestCase(300, 100, true)]
        [TestCase(500, 50, false)]
        public void When_CancellationIsRequested_WhileReadingProcessOutput__MustAbort_RightAway_KillTheProcess_And_ThrowCancellationException(int timeoutMs, int delayBetweenReads, bool redirectStdErr)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var sourceBytes = Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!");
            using (var cancellation = new CancellationTokenSource())
            {
                var stdout = new EndlessFakeReadStream(delayMs: delayBetweenReads, maxReadSize: 1);
                var stderr = redirectStdErr
                    ? new EndlessFakeReadStream(delayMs: 100, maxReadSize: 1) // just to slow it down so the reading stream doesn't fill up too much memory
                    : null;
                var process = new ExternalProcessFake(stdout, stdin: null, stderr: stderr, respondToExitRequest: true);

                cancellation.CancelAfter(timeoutMs);
                var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: redirectStdErr, cancellation: cancellation.Token);

                Assert.Throws<OperationCanceledException>(() => Util.Read(outputStream));
                Assert.True(process.IsDisposedOf, "Process has to be disposed of");
            }
        }

        [TestCase(200, 100, true)]
        [TestCase(500, 50, false)]
        public void When_CancellationIsRequested_WhileReadingProcessOutput__MustAbort_RightAway_KillTheProcess_And_ThrowCancellationException__WhenUsingInput(int timeout, int delayBetweenReads, bool redirectStdErr)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var sourceBytes = Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!");
            using (var cancellation = new CancellationTokenSource())
            {
                var input = new EndlessFakeReadStream(delayMs: delayBetweenReads, maxReadSize: 64);
                var stderr = redirectStdErr
                        ? new EndlessFakeReadStream(delayMs: 100, maxReadSize: 1) // just to slow it down so the reading stream doesn't fill up too much memory
                        : null;
                var process = new ExternalProcessPiped(stderr: stderr);

                cancellation.CancelAfter(timeout);
                var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, input, readStderr: redirectStdErr, cancellation: cancellation.Token);

                Assert.Throws<OperationCanceledException>(() => Util.Read(outputStream));
                Assert.True(process.IsDisposedOf, "Process has to be disposed of");
            }
        }

        [TestCase(200, 100, true)]
        [TestCase(500, 50, false)]
        public void When_CancellationIsRequested_WhileStuckWaitingForMoreStdOut__Must_Do_the_CancellationRoutine(int timeout, int delayBetweenReads, bool redirectStdErr)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var sourceBytes = Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!");
            using (var cancellation = new CancellationTokenSource())
            {
                var stdout = new EndlessFakeReadStream(delayMs: delayBetweenReads, maxReadSize: 64);
                var stderr = redirectStdErr
                    ? new EndlessFakeReadStream(delayMs: 100, maxReadSize: 1) // just to slow it down so the reading stream doesn't fill up too much memory
                    : null;
                var process = new ExternalProcessFake(stdout: stdout, stdin: null, stderr: stderr);

                cancellation.CancelAfter(timeout);
                var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: redirectStdErr, cancellation: cancellation.Token);

                Assert.Throws<OperationCanceledException>(() => Util.Read(outputStream));
                Assert.True(process.IsDisposedOf, "Process has to be disposed of");
            }
        }
    }
}