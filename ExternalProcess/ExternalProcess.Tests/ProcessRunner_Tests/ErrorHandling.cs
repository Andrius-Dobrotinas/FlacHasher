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
        public void When_ProcessExits_With_ErrorExitCode__UponReadingTheWholeOutputStream__Must_Throw_ExecutionException_And_DisposeOfTheProcess(int exitCode)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"));
            using (var killProcessSignal = new ManualResetEventSlim(false))
            {
                var process = new ExternalProcessFake(stdout: stdout, stdin: null, exitCode: exitCode);

                var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process);

                Assert.Throws<ExecutionException>(() => Util.Read(outputStream));
                Assert.True(process.IsDisposedOf);
            }
        }

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

        [TestCase(-1)]
        [TestCase(224)]
        public void When_Process_HasToBeKilledOnExit_AndReturns_ErrorExitCode__UponReadingTheWholeOutputStream__Must_Throw_ExecutionException_And_DisposeOfTheProcess(int exitCode)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"));
            var process = new ExternalProcessFake(stdout: stdout, stdin: null, respondToExitRequest: false, exitCode: exitCode);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process);

            Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

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

        [Test]
        public void When_StdErrIsRedirected__And_ProcessExits_With_ErrorExitCode__UponReadingTheWholeOutputStream__Must_Throw_ExecutionException_With_IsProcessOutputCaptured_True_And_MessageIncludingErrorOutput()
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            using (var stderrReadFinishSignal = new ManualResetEventSlim(false))
            {
                var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"));
                var stderr = new ReadSignallingMemoryStream(Encoding.UTF8.GetBytes("Here's some error for ya!"), readFinishSignal: stderrReadFinishSignal);
                var process = new ExternalProcessFake(stdout: stdout, stdin: null, stderr: stderr, exitCode: -1);

                var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: true);

                // Wait for the whole of stderr to be captured before letting the process "exit", which closes the streams
                stderrReadFinishSignal.Wait();

                var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

                Assert.True(exception.IsProcessOutputCaptured);
                Assert.That(exception.Message, Does.Contain("Here's some error for ya!"));
            }
        }

        [Test]
        public void When_StdErrIsNotRedirected__And_ProcessExits_With_ErrorExitCode__UponReadingTheWholeOutputStream__Must_Throw_ExecutionException_With_IsProcessOutputCaptured_False_And_MessageStatingItWasNotCaptured()
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"));
            var process = new ExternalProcessFake(stdout: stdout, stdin: null, exitCode: -1);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: false);

            var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            Assert.False(exception.IsProcessOutputCaptured);
            Assert.That(exception.Message, Does.Contain("has not been captured"));
        }
    }
}