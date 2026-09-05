using NUnit.Framework;
using System.Linq;
using System.Text;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    /// <summary>
    /// What's kept of the process' error output, and what it costs to keep it.
    /// A decoder reports progress there for the length of the run, so on a large file it amounts to far more
    /// than anyone would want held in memory - and the part worth having, the error, is written last.
    /// </summary>
    [Timeout(60000)]
    public class ErrorOutput
    {
        const string progressText = "decoding, decoding, decoding, still decoding away";
        const string errorText = "AND-HERE-IS-WHAT-WENT-WRONG";
        const int limitBytes = 2 * 1024;

        /// <summary>
        /// One line of progress per byte of a 256-byte source, which is far more than the limit allows.
        /// </summary>
        static DecoderArgs Chatty()
        {
            return DecoderArgs.Reading(TestPayload.SourceFile)
                .ReadChunkSize(1)
                .ProgressMessage(progressText)
                .ErrorMessage(errorText)
                .ExitCode(3);
        }

        [Test]
        public void When_TheProcess_WritesMoreErrorOutput_ThanTheLimitAllows__Must_Keep_TheMostRecent_UpToTheLimit()
        {
            var outputStream = Decoder.Run(Chatty(), maxErrorOutputBytes: limitBytes);

            var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            Assert.LessOrEqual(
                Encoding.UTF8.GetByteCount(exception.ProcessErrorOutput), limitBytes,
                "Nothing past the limit may be kept");

            // Written last, so keeping the tail is the only way it survives
            Assert.That(exception.ProcessErrorOutput, Does.Contain(errorText));
        }

        [Test]
        public void When_ThereIsNoLimit__Must_Keep_TheWholeOfTheErrorOutput()
        {
            var outputStream = Decoder.Run(Chatty(), maxErrorOutputBytes: ProcessRunner.UnboundedErrorOutput);

            var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            Assert.Greater(
                Encoding.UTF8.GetByteCount(exception.ProcessErrorOutput), limitBytes,
                "The whole of it is far more than the limit the other test imposes");

            Assert.AreEqual(
                TestPayload.Bytes.Length,
                CountOccurrences(exception.ProcessErrorOutput, progressText),
                "One progress line per read, and none of them dropped");

            Assert.That(exception.ProcessErrorOutput, Does.Contain(errorText));
        }

        /// <summary>
        /// A process whose error output isn't drained stops writing once the pipe fills up, and it stops
        /// writing to stdout right along with it. Reaching the limit mustn't turn into a stall.
        /// </summary>
        [Test]
        public void When_TheProcess_WritesFarMoreErrorOutput_ThanTheLimit__Must_Keep_DrainingIt__SoTheRunCanFinish()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.LargeSourceFile)
                    .ReadChunkSize(64)
                    .ProgressMessage(progressText),
                maxErrorOutputBytes: limitBytes);

            var result = Util.Read(outputStream);

            // Roughly 4000 progress lines against a 2 KiB limit: without draining past it, the process would still be waiting
            Assert.AreEqual(TestPayload.LargeBytes, result);
        }

        static int CountOccurrences(string text, string value)
        {
            return text.Split(value).Length - 1;
        }
    }
}
