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
        [TestCase(-1)]
        [TestCase(368)]
        public void When_StdOutFinishes_And_ProcessExits_WithErrorExitCode__Must_ThrowExecutionException_And_DisposeOfTheProcess(int exitCode)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"));
            using (var killProcessSignal = new ManualResetEventSlim(false))
            {
                var process = new ExternalProcessFake(stdout: stdout, stdin: null);

                var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process);

                process.ExitCode = exitCode;

                Assert.Throws<ExecutionException>(() => Util.Read(outputStream));
                Assert.True(process.IsDisposedOf);
            }
        }

        [TestCase(100)]
        [TestCase(300)]
        public void When_StdErrIsRedirected__When_StdOutCloses_And_ProcessExits_With_ErrorExitCode_But_StdErr_IsNotResponding__Must_AbortReadingIt_AfterTimeout(int timoeut)
        {
            var target = new ProcessRunner(-1, timoeut, 0, false);

            using (var stderrReadSignal = new AutoResetEvent(false))
            {
                var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner"));
                var stderr = new SignalWaitingMemoryStream(Encoding.UTF8.GetBytes("Here's some error for ya!"), stderrReadSignal, maxReadSize: 8);
                var process = new ExternalProcessFake(stdout: stdout, stdin: null, stderr: stderr);

                var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: true);

                process.ExitCode = -1;
                stderrReadSignal.Set();

                Assert.Throws<ExecutionException>(() => Util.Read(outputStream));
                Assert.True(process.IsDisposedOf, "Still must dispose of the process");
            }
        }

        [TestCase(-1)]
        [TestCase(224)]
        public void When_StdOutFinishes_But_ProcessHasToBeKilledOnExit_And_Returns_ErrorExitCode__Must_ThrowException_And_DisposeOfTheProcess(int exitCode)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"));
            var process = new ExternalProcessFake(stdout: stdout, stdin: null, respondToExitRequest: false);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process);

            process.ExitCode = exitCode;

            Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            Assert.True(process.IsDisposedOf);
        }
    }
}