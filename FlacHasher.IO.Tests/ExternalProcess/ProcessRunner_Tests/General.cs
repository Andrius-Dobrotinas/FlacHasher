using Andy.FlacHash.ExternalProcess;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Andy.FlacHash.IO.ExternalProcess.ProcessRunner_Tests
{
    [NonParallelizable]
    public class General
    {
        [TestCase(true)]
        [TestCase(false)]
        public void Must_Return_OutputStream_RightAway_WithoutWaitingFor_StdOutToServeData__WhenUsingInput(bool redirectStderr)
        {
            var input = new DelayingMemoryStream(new byte[] { 1, 2, 3 }, delayMillis: 500);

            var target = new ProcessRunner(0, 0, 0, false);

            var stderr = redirectStderr ? new DelayingMemoryStream(new byte[] { 1, 2, 3 }, delayMillis: 500) : null;
            using (var timeout = new CancellationTokenSource())
            {
                var resultTask = Task.Run(() =>
                {
                    // The process' stdout will be waiting for some data to come in through stdin
                    // This way, both stdout and stdin will be waiting
                    var process = new ExternalProcessPiped(stderr: stderr);

                    // the input stream will start returning data way after stdout is returned
                    return target.GetOutputStream_WaitProcessExitInParallel(process, input, readStderr: true);
                });

                // just to account for variations in system speed
                timeout.CancelAfter(100);

                Assert.DoesNotThrow(() => resultTask.Wait(timeout.Token));

                Assert.NotNull(resultTask.Result);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Must_Return_OutputStream_RightAway_WithoutWaitingFor_StdOutToServeData(bool redirectStderr)
        {
            var input = new DelayingMemoryStream(new byte[] { 1, 2, 3 }, delayMillis: 500);

            var target = new ProcessRunner(0, 0, 0, false);

            var stderr = redirectStderr ? new DelayingMemoryStream(new byte[] { 1, 2, 3 }, delayMillis: 500) : null;
            
            // Waits for data and doesn't release whoever's trying to read until the stream is closed
            using (var stdoutSourcePipe = new AnonymousPipeServerStream(PipeDirection.Out))
            using (var timeout = new CancellationTokenSource())
            {
                var resultTask = Task.Run(() =>
                {
                    // The process' stdout will be waiting for some data to be sent by stdinPipeServer
                    var process = new ExternalProcessPiped(stdoutSourcePipe, stderr: stderr);

                    return target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: true);
                });

                // just to account for variations in system speed
                timeout.CancelAfter(100);

                Assert.DoesNotThrow(() => resultTask.Wait(timeout.Token));

                Assert.NotNull(resultTask.Result);
            }
        }

        [TestCaseSource(nameof(GetByteSequences_WithStdErrRedirectFlag))]
        public void TheStream_Must_ServeFrom_StdOut_OfTheProcess(byte[] sourceBytes, bool redirectStderr)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var stdout = new MemoryStream(sourceBytes);
            var stderr = redirectStderr ? new MemoryStream(Encoding.UTF8.GetBytes("E.r.r.o.r.")) : null;
            var process = new ExternalProcessFake(stdout: stdout, stdin: null, stderr: stderr);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, readStderr: redirectStderr);
            var resultBytes = Util.Read(outputStream);

            Assert.AreEqual(sourceBytes, resultBytes);
        }


        [TestCaseSource(nameof(GetByteSequences_WithStdErrRedirectFlag))]
        public void TheStream_Must_ServeDataFrom_StdOut_OfTheProcess__WhenUsingInput(byte[] sourceBytes, bool redirectStderr)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var stdout = new MemoryStream(sourceBytes);
            var stderr = redirectStderr ? new MemoryStream(Encoding.UTF8.GetBytes("E.r.r.o.r.")) : null;
            var process = new ExternalProcessFake(stdout: stdout, stdin: new FakeWriteStream(), stderr: stderr);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, input: new MemoryStream(0), readStderr: redirectStderr);
            var resultBytes = Util.Read(outputStream);

            Assert.AreEqual(sourceBytes, resultBytes);
        }

        [TestCase(200, true)]
        [TestCase(200, false)]
        [TestCase(1000, true)]
        [TestCase(1000, false)]
        public void TheStream_UponReading_Must_WaitFor_StdOut_ToReturnSomeData(int stdoutResponseDelayMs, bool withInput)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            using (var stdoutReadSignal = new AutoResetEvent(false))
            {
                var stdout = new SignalWaitingMemoryStream(new byte[] { 1, 2 }, stdoutReadSignal);
                var process = new ExternalProcessFake(stdout: stdout, stdin: withInput ? new FakeWriteStream() : null);

                var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, input: withInput ? new MemoryStream(0) : null);

                var readTask = Task.Run(outputStream.ReadByte);

                var hasReturned = readTask.Wait(stdoutResponseDelayMs);
                Assert.IsFalse(hasReturned, "Has to keep waiting for data from stdout");

                stdoutReadSignal.Set();

                hasReturned = readTask.Wait(1000);
                Assert.IsTrue(hasReturned, "Has to return once stdout returns some data");
            }
        }

        [TestCaseSource(nameof(GetByteSequences_WithStdErrRedirectFlag))]
        public void Once_StdOut_IsFullyRead_And_ProcessExitedSuccessfully__Must_DisposeOf_TheProcess(byte[] sourceBytes, bool withInput)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var stdout = new MemoryStream(sourceBytes);
            var process = new ExternalProcessFake(stdout: stdout, stdin: withInput ? new FakeWriteStream() : null);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, input: withInput ? new MemoryStream(0) : null);

            Assert.False(process.IsDisposedOf, "Must not dispose of the process until the stream is read");

            process.ExitCode = 0;

            Util.Read(outputStream);

            Assert.True(process.IsDisposedOf);
        }

        [Test]
        public void Once_StdOut_IsFullyRead_But_ProcessDoesNotExit__Must_Quietly_Kill_TheProcess_DisposeOfIt_And_Return()
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var voluntaryExitCompletion = new TaskCompletionSource<bool>();

            var stdout = new MemoryStream(Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"));
            var process = new ExternalProcessFake(stdout: stdout, stdin: null, voluntaryExitCompletion: voluntaryExitCompletion, respondToExitRequest: false);

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process);

            process.ExitCode = 0;

            Assert.DoesNotThrow(() => Util.Read(outputStream));

            Assert.IsTrue(voluntaryExitCompletion.Task.Wait(1000), "Process has to exit either way");
            Assert.IsFalse(voluntaryExitCompletion.Task.Result, "Process has to have been killed");
            Assert.True(process.IsDisposedOf, "Process has to be disposed of");
        }

        // If a stream is redirected, once the process' buffer fills up, it stops and waits, so stdout freezes too
        [TestCaseSource(nameof(GetByteSequences_WithStdErrRedirectFlag))]
        public void When_Stderr_IsRedirected__Must_Start_ReadingIt_RightAway__SoAsNotToBlockTheProcess(byte[] sourceBytes, bool withInput)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            using (var readSignal = new AutoResetEvent(false))
            {
                var stdout = new MemoryStream(sourceBytes);
                var stderr = new ReadSignallingMemoryStream(Encoding.UTF8.GetBytes("E.r.r.o.r."), readSignal: readSignal);
                var process = new ExternalProcessFake(
                    stdout: new MemoryStream(0),
                    stdin: withInput ? new FakeWriteStream() : null,
                    stderr: stderr);

                target.GetOutputStream_WaitProcessExitInParallel(
                    process,
                    input: withInput ? new MemoryStream(0) : null,
                    readStderr: true);

                var hasRead = readSignal.WaitOne(1000);
                Assert.IsTrue(hasRead);
            }
        }

        static IEnumerable<TestCaseData> GetByteSequences_WithStdErrRedirectFlag()
        {
            yield return new TestCaseData(
                Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"),
                false);

            yield return new TestCaseData(
                Encoding.UTF8.GetBytes("It's so relieving\n to know that you're leaving\n as soon as you get paid\n"),
                true);
        }
    }
}