using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    /// <summary>
    /// Hashing an album means running the decoder once per track, one after another, so anything the runner
    /// fails to let go of is paid for again on every file. Nothing above shows that: a single run leaks just
    /// as quietly as it succeeds.
    /// </summary>
    [Timeout(120000)]
    public class RepeatedRuns
    {
        const int runs = 30;
        const int expand = 2;

        [Test]
        public void Must_Keep_ServingCorrectly__RunAfterRun()
        {
            var expected = TestPayload.Expected(TestPayload.Bytes, expand);

            for (int run = 0; run < runs; run++)
            {
                var outputStream = Decoder.Run(DecoderArgs.Reading(TestPayload.SourceFile).Expand(expand));

                Assert.AreEqual(expected, Util.Read(outputStream), $"Run {run}");
            }
        }

        [Test]
        public void Must_Keep_ServingCorrectly__RunAfterRun__WhenFedThroughStdIn()
        {
            var expected = TestPayload.Expected(TestPayload.Bytes, expand);

            for (int run = 0; run < runs; run++)
            {
                var outputStream = Decoder.Run(
                    DecoderArgs.ReadingStdin().Expand(expand),
                    new MemoryStream(TestPayload.Bytes));

                Assert.AreEqual(expected, Util.Read(outputStream), $"Run {run}");
            }
        }
    }
}
