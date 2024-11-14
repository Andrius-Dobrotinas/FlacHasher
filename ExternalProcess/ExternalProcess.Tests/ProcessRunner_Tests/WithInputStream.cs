using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    [NonParallelizable]
    public class WithInputStream
    {
        [Test]
        public void Must_StartWriting_To_StdIn_RightAway__WithoutWaiting_For_RetunedStream_ToGetConsumed()
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var inputBytes = Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!");

            using (var writeSignal = new AutoResetEvent(false))
            {
                var stdin = new FakeWriteStream(writeSignal);
                var process = new ExternalProcessFake(stdout: new MemoryStream(), stdin: stdin);

                target.GetOutputStream_WaitProcessExitInParallel(process, input: new MemoryStream(inputBytes));
                var stdinBeenWrittenTo = writeSignal.WaitOne(1000);

                Assert.IsTrue(stdinBeenWrittenTo);
            }
        }

        [TestCaseSource(nameof(GetByteSequences))]
        public void Must_Write_TheWhole_InputStream_To_StdIn(byte[] sourceBytes)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            using (var readFinishSignal = new ManualResetEventSlim(false))
            {
                var input = new ReadSignallingMemoryStream(sourceBytes, readFinishSignal: readFinishSignal);
                var stdin = new MemoryStream();
                var process = new ExternalProcessFake(stdout: new MemoryStream(0), stdin: stdin);

                target.GetOutputStream_WaitProcessExitInParallel(process, input);

                var stdinBeenWrittenTo = readFinishSignal.Wait(5000); // for some reason, when running all tests, one case here takes longer than a second
                Assert.IsTrue(stdinBeenWrittenTo, "Must finish reading the whole stream into stdin");

                Assert.AreEqual(sourceBytes, stdin.ToArray());
            }
        }

        [TestCaseSource(nameof(GetByteSequences))]
        public void Must_Send_EndOfStream_To_StdIn_By_ClosingIt_When_Finishes_Writing_Input(byte[] sourceBytes)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            using (var readFinishSignal = new ManualResetEventSlim(false))
            using (var closeSignal = new ManualResetEventSlim(false))
            {
                var input = new ReadSignallingMemoryStream(sourceBytes, readFinishSignal: readFinishSignal);
                var stdin = new FakeWriteStream(closeSignal: closeSignal);
                var process = new ExternalProcessFake(stdout: new MemoryStream(), stdin: stdin);

                target.GetOutputStream_WaitProcessExitInParallel(process, input);

                readFinishSignal.Wait(); //this gives an idea of when input must be closed
                var hasClosed = closeSignal.Wait(500);

                Assert.IsTrue(hasClosed);
            }
        }

        [TestCaseSource(nameof(GetByteSequences))]
        public void Must_Close_InputStream_When_Finishes_WritingTo_StdIn(byte[] sourceBytes)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            using (var inputCloseSignal = new ManualResetEventSlim(false))
            using (var stdinCloseSignal = new ManualResetEventSlim(false))
            {
                var input = new ReadSignallingMemoryStream(sourceBytes, closeSignal: inputCloseSignal);

                var stdin = new FakeWriteStream(closeSignal: stdinCloseSignal);
                var process = new ExternalProcessFake(stdout: new MemoryStream(), stdin: stdin);

                target.GetOutputStream_WaitProcessExitInParallel(process, input);

                stdinCloseSignal.Wait(); //this gives an idea of when input must be closed
                var hasClosed = inputCloseSignal.Wait(500);

                Assert.IsTrue(hasClosed);
            }
        }

        [TestCaseSource(nameof(GetByteSequences))]
        public void When_InputIsNotAvailableYet_TheReturnedStream_Must_BeOpen(byte[] sourceBytes)
        {
            var target = new ProcessRunner(-1, 0, 0, false);

            var input = new DelayingMemoryStream(sourceBytes, 500);
            var process = new ExternalProcessPiped();

            var outputStream = target.GetOutputStream_WaitProcessExitInParallel(process, input);

            int byteRead = -1;
            Assert.DoesNotThrow(() => byteRead = outputStream.ReadByte());
            Assert.AreEqual(sourceBytes[0], byteRead);
        }

        static IEnumerable<TestCaseData> GetByteSequences()
        {
            yield return new TestCaseData(
                Encoding.UTF8.GetBytes("Alright, partner, you know what time it is. Let's keep on rolling!"));

            yield return new TestCaseData(
                Encoding.UTF8.GetBytes("It's so relieving\n to know that you're leaving\n as soon as you get paid\n"));
        }
    }
}