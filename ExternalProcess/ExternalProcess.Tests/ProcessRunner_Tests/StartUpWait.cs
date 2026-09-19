using NUnit.Framework;
using System.Diagnostics;
using System.IO;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    /// <summary>
    /// The wait before touching the process' streams. It exists because writing to a process that isn't ready
    /// to receive yet fails, so it is the feeding that needs it - a process with nothing to receive has nothing
    /// to be too early for, and waiting on it would be dead time on every run.
    /// </summary>
    [Timeout(60000)]
    public class StartUpWait
    {
        const int longWaitMs = 3000;

        [Test]
        public void When_TheProcess_IsNot_BeingFed__Must_Not_Wait_AtAll()
        {
            var clock = Stopwatch.StartNew();

            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile),
                startWaitMs: longWaitMs);

            var elapsed = clock.ElapsedMilliseconds;

            Assert.AreEqual(TestPayload.Bytes, Util.Read(outputStream));
            Assert.Less(elapsed, longWaitMs / 3, "Nothing is being written to the process, so there is nothing to hold off");
        }

        [Test]
        public void When_TheProcess_IsBeingFed__Must_Wait_BeforeTouchingItsStreams()
        {
            var clock = Stopwatch.StartNew();

            var outputStream = Decoder.Run(
                DecoderArgs.ReadingStdin(),
                new MemoryStream(TestPayload.Bytes),
                startWaitMs: longWaitMs);

            var elapsed = clock.ElapsedMilliseconds;

            Assert.AreEqual(TestPayload.Bytes, Util.Read(outputStream));
            Assert.GreaterOrEqual(elapsed, longWaitMs * 0.8, "The wait is what a process too slow to be ready is given");
        }
    }
}
