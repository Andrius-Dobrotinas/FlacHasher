using NUnit.Framework;
using System.IO;
using System.Threading;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    /// <summary>
    /// Feeding a process through its standard input, and what it means when it stops listening.
    /// A pipe holds only so much, so a process that gives up on its input part way leaves the writing
    /// blocked on a full pipe and then broken - the same thing `head` does to whatever feeds it.
    /// </summary>
    [Timeout(60000)]
    public class Feeding
    {
        const string errorText = "THAT-WILL-DO";

        /// <summary>
        /// Takes one chunk and finishes, leaving the rest of a payload far bigger than the pipe unread.
        /// No expansion: the point is the input pipe filling up, and a bigger output would stall this on the other one first.
        /// </summary>
        static DecoderArgs GivesUpOnItsInput()
        {
            return DecoderArgs.ReadingStdin().ReadChunkSize(64).FinishAfterReads(1);
        }

        [Test]
        public void When_TheProcess_StopsReading_BeforeItHasEverything__And_Exits_Successfully__Must_Throw_PrematureExitException()
        {
            var outputStream = Decoder.Run(GivesUpOnItsInput(), new MemoryStream(TestPayload.LargeBytes));

            var exception = Assert.Throws<PrematureExitException>(() => Util.Read(outputStream));

            Assert.AreEqual(0, exception.ExitCode, "The process really did exit, and chose this code itself");
        }

        /// <summary>
        /// Some callers show nothing but the message, so it has to name the failure on its own.
        /// </summary>
        [Test]
        public void When_TheProcess_StopsReading_BeforeItHasEverything__TheMessage_Must_Say_So_WithoutHelp()
        {
            var outputStream = Decoder.Run(GivesUpOnItsInput(), new MemoryStream(TestPayload.LargeBytes));

            var exception = Assert.Throws<PrematureExitException>(() => Util.Read(outputStream));

            Assert.That(exception.Message, Does.Contain("stopped reading its input"));
        }

        /// <summary>
        /// A code the process chose for itself says more than the fact that it stopped listening.
        /// </summary>
        [Test]
        public void When_TheProcess_StopsReading_BeforeItHasEverything__But_Exits_WithAnErrorCode__Must_Report_TheErrorCode_Instead()
        {
            var outputStream = Decoder.Run(
                GivesUpOnItsInput().ExitCode(4).ErrorMessage(errorText),
                new MemoryStream(TestPayload.LargeBytes));

            var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            Assert.IsNotInstanceOf<PrematureExitException>(exception);
            Assert.AreEqual(4, exception.ExitCode);
            Assert.That(exception.ProcessErrorOutput, Does.Contain(errorText));
        }

        [Test]
        public void When_TheProcess_Reads_TheWholeInput__Must_Not_Throw()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.ReadingStdin().ReadChunkSize(64),
                new MemoryStream(TestPayload.LargeBytes));

            var result = Util.Read(outputStream);

            Assert.AreEqual(TestPayload.LargeBytes, result);
        }

        /// <summary>
        /// A decoder's output outgrows its input, so the process is writing far more than it is reading while
        /// the feeding is still going on. Both pipes are in play at once and neither may be allowed to wedge the other.
        /// </summary>
        [Test]
        public void When_TheProcess_Produces_FarMore_ThanItConsumes__Must_Feed_And_Serve_Without_Deadlocking()
        {
            const int expand = 4;

            var outputStream = Decoder.Run(
                DecoderArgs.ReadingStdin().ReadChunkSize(1024).Expand(expand),
                new MemoryStream(TestPayload.LargeBytes));

            var result = Util.Read(outputStream);

            Assert.AreEqual(TestPayload.Expected(TestPayload.LargeBytes, expand), result);
        }

        /// <summary>
        /// The runner takes ownership of the stream it is handed: the caller has no way of knowing when the
        /// feeding is done, so it can't be the one to close it.
        /// </summary>
        [Test]
        public void Must_DisposeOf_TheInputStream_ItWasGiven()
        {
            using (var inputClosed = new ManualResetEventSlim(false))
            {
                var input = new ReadSignallingMemoryStream(TestPayload.Bytes, closeSignal: inputClosed);

                var outputStream = Decoder.Run(DecoderArgs.ReadingStdin(), input);
                Util.Read(outputStream);

                Assert.IsTrue(inputClosed.Wait(2000));
            }
        }
    }
}
