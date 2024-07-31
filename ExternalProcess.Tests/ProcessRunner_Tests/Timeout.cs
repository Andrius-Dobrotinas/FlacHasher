using NUnit.Framework;
using Moq;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    [NonParallelizable]
    public class Timeout
    {
        [TestCase(1, 2, 400, true)]
        [TestCase(1, 2, 400, false)]
        [TestCase(2, 4, 580, true)]
        public void When_OutputReading_TakesLonger_ThanTimeoutValue__Must_TimeOut_By_Throwing_TimeoutException_And_Killing_And_DisposingOf_TheProcess(int timeoutSec, int readChunkSize, int delayBetweenReadsMs, bool redirectStderr)
        {
            var target = new ProcessRunner(timeoutSec, 0, 0, false);

            var voluntaryExitCompletion = new TaskCompletionSource<bool>();

            var stdout = new EndlessFakeReadStream(delayMs: delayBetweenReadsMs, maxReadSize: readChunkSize);
            var stderr = redirectStderr
                ? new EndlessFakeReadStream(delayMs: 100, maxReadSize: 1)
                : null;
            var process = new ExternalProcessFake(stdout: stdout, stdin: null, stderr, voluntaryExitCompletion: voluntaryExitCompletion);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process);

            Assert.Throws<TimeoutException>(
                // make sure the test assesses the situation right after the timeout is supposed to happen
                () => Run.WithAutoCancellation(
                    cancellation => Util.Read(outputStream, cancellation),
                    timeoutMs: timeoutSec * 1000 + 50));
            
            Assert.IsTrue(voluntaryExitCompletion.Task.Wait(1000), "Process has to exit either way");
            Assert.IsFalse(voluntaryExitCompletion.Task.Result, "Process has to have been killed");
            Assert.True(process.IsDisposedOf, "Process has to be disposed of");
        }

        [TestCase(1, 200, true)]
        [TestCase(2, 200, true)]
        [TestCase(2, 200, false)]
        public void WithInput__When_OutputReading_TakesLonger_ThanTimeoutValue__Must_TimeOut_By_Throwing_TimeoutException_And_Killing_And_DisposingOf_TheProcess(int timeoutSec, int delayBetweenReadsMs, bool redirectStderr)
        {
            var target = new ProcessRunner(timeoutSec, 0, 0, false);

            var voluntaryExitCompletion = new TaskCompletionSource<bool>();

            var input = new EndlessFakeReadStream(delayMs: delayBetweenReadsMs, maxReadSize: 1);
            var stderr = redirectStderr
                ? new EndlessFakeReadStream(delayMs: 100, maxReadSize: 1)
                : null;
            var process = new ExternalProcessPiped(voluntaryExitCompletion: voluntaryExitCompletion, stderr: stderr);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, input, readStderr: redirectStderr);

            Assert.Throws<TimeoutException>(
                () => Run.WithAutoCancellation(
                    cancellation => Util.Read(outputStream, cancellation),
                    timeoutMs: timeoutSec * 1000 + 1000)); // weirdly, when running many tests, this timeout has to be longer

            Assert.IsTrue(voluntaryExitCompletion.Task.Wait(1000), "Process has to exit either way");
            Assert.IsFalse(voluntaryExitCompletion.Task.Result, "Process has to have been killed");
            Assert.True(process.IsDisposedOf, "Process has to be disposed of");
        }

        [TestCase(1, 150)]
        [TestCase(2, 100)]
        public void WithInput_When_OutputReading_GetsStuckWaitingForStdOut_And_TimesOut__Must_Do_the_TimeoutRoutine(int timeoutSec, int delayBetweenReadsMs)
        {
            var target = new ProcessRunner(timeoutSec, 0, 0, false);

            var voluntaryExitCompletion = new TaskCompletionSource<bool>();

            var input = new EndlessFakeReadStream(delayMs: delayBetweenReadsMs, maxReadSize: 8);
            var process = new ExternalProcessPiped(voluntaryExitCompletion: voluntaryExitCompletion);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, input);

            Assert.Throws<TimeoutException>(() => Util.Read(outputStream));
            Assert.IsTrue(voluntaryExitCompletion.Task.Wait(1000), "Process has to exit either way");
            Assert.IsFalse(voluntaryExitCompletion.Task.Result, "Process has to have been killed");
            Assert.True(process.IsDisposedOf, "Process has to be disposed of");
        }
    }
}