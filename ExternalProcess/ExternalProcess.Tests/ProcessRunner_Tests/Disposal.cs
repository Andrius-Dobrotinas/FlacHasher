using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    /// <summary>
    /// Letting go of the stream, whether the run is over or not. Disposing of it part way is how a caller
    /// says it wants no more, and it has to be as quiet as closing a file - the caller is the one who asked.
    /// </summary>
    [Timeout(60000)]
    public class Disposal
    {
        /// <summary>
        /// Trickles its output out slowly enough that the run is certain to still be going when the stream is let go of.
        /// </summary>
        static DecoderArgs StillGoing()
        {
            return DecoderArgs.Reading(TestPayload.LargeSourceFile)
                .ReadChunkSize(64)
                .WriteDelay(20);
        }

        [Test]
        public void When_TheStream_IsDisposedOf_WhileTheProcess_IsStillWriting__Must_Be_Quiet()
        {
            var outputStream = Decoder.Run(StillGoing());

            outputStream.ReadByte();

            Assert.DoesNotThrow(() => outputStream.Dispose());
        }

        [Test]
        public void When_TheStream_IsDisposedOf_WhileTheProcess_IsStillWriting__A_ReadInFlight_Must_Report_Cancellation()
        {
            var outputStream = Decoder.Run(StillGoing());

            outputStream.ReadByte();

            var read = Task.Run(() => Util.Read(outputStream));

            outputStream.Dispose();

            var exception = Assert.Throws<AggregateException>(() => read.Wait(10000));
            Assert.IsInstanceOf<OperationCanceledException>(exception.InnerException);
        }

        [Test]
        public void When_TheStream_IsDisposedOf_AfterTheRun_IsOver__Must_Be_Quiet()
        {
            var outputStream = Decoder.Run(DecoderArgs.Reading(TestPayload.SourceFile));

            Util.Read(outputStream);

            Assert.DoesNotThrow(() => outputStream.Dispose());
        }

        [Test]
        public void When_TheStream_IsDisposedOf_AfterTheRun_IsOver__Must_Be_Quiet__WhenFedThroughStdIn()
        {
            var outputStream = Decoder.Run(DecoderArgs.ReadingStdin(), new MemoryStream(TestPayload.Bytes));

            Util.Read(outputStream);

            Assert.DoesNotThrow(() => outputStream.Dispose());
        }

        [Test]
        public void When_TheStream_IsDisposedOf_Twice__Must_Be_Quiet()
        {
            var outputStream = Decoder.Run(DecoderArgs.Reading(TestPayload.SourceFile));

            Util.Read(outputStream);
            outputStream.Dispose();

            Assert.DoesNotThrow(() => outputStream.Dispose());
        }

        /// <summary>
        /// Disposing of it is the caller saying it wants no more, so the process has no reason to be left running.
        /// </summary>
        [Test]
        public void When_TheStream_IsDisposedOf_WhileTheProcess_IsStillWriting__Must_Not_Leave_ItRunning()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).KeepStdoutOpen(DecoderArgs.WaitForever));

            // Returning at all means the process is gone: closing waits for it, and this one would never end on its own
            Assert.DoesNotThrow(() => outputStream.Dispose());
        }
    }
}
