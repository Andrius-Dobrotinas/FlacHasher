using NUnit.Framework;
using System.Diagnostics;
using System.IO;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    /// <summary>
    /// What the consumer gets out of the returned stream, driven by a real process.
    /// Expansion is on by default: a decoder's output outgrows its source, and output that merely echoes the
    /// input can't tell a stream that was served from one that was handed back.
    /// </summary>
    [Timeout(30000)]
    public class Output
    {
        const byte xor = 0x5A;
        const int expand = 3;

        [Test]
        public void TheStream_Must_Serve_WhatTheProcessWroteTo_StdOut()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).Expand(expand).Xor(xor));

            var result = Util.Read(outputStream);

            Assert.AreEqual(TestPayload.Expected(TestPayload.Bytes, expand, xor), result);
        }

        [Test]
        public void TheStream_Must_Serve_WhatTheProcessWroteTo_StdOut__WhenFedThroughStdIn()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.ReadingStdin().Expand(expand).Xor(xor),
                new MemoryStream(TestPayload.Bytes));

            var result = Util.Read(outputStream);

            Assert.AreEqual(TestPayload.Expected(TestPayload.Bytes, expand, xor), result);
        }

        /// <summary>
        /// Spread over many small writes, so that the stream is served in pieces rather than in one go.
        /// </summary>
        [TestCase(1)]
        [TestCase(7)]
        public void TheStream_Must_Serve_TheWholeOutput__HoweverItIsChunked(int readChunkSize)
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).Expand(expand).ReadChunkSize(readChunkSize));

            var result = Util.Read(outputStream);

            Assert.AreEqual(TestPayload.Expected(TestPayload.Bytes, expand), result);
        }

        [Test]
        public void When_TheProcess_WritesNothing__TheStream_Must_BeEmpty()
        {
            var outputStream = Decoder.Run(DecoderArgs.Reading(TestPayload.EmptySourceFile));

            var result = Util.Read(outputStream);

            Assert.IsEmpty(result);
        }

        /// <summary>
        /// Stream's contract: once a read returns 0, further reads keep returning 0 rather than throwing.
        /// </summary>
        [Test]
        public void TheStream_Must_KeepReturning_Zero_OnReads_AfterTheEnd()
        {
            var outputStream = Decoder.Run(DecoderArgs.Reading(TestPayload.SourceFile));

            Util.Read(outputStream);

            Assert.AreEqual(0, outputStream.Read(new byte[1], 0, 1));
            Assert.AreEqual(0, outputStream.Read(new byte[1], 0, 1));
        }

        [Test]
        public void Flush_MustNot_Throw()
        {
            var outputStream = Decoder.Run(DecoderArgs.Reading(TestPayload.SourceFile));

            Util.Read(outputStream);

            Assert.DoesNotThrow(() => outputStream.Flush());
        }

        /// <summary>
        /// The stream stands for output still to come, so it has to be handed over before there is any.
        /// Reading it does wait for data - that's what the content tests above rely on.
        /// </summary>
        [Test]
        public void Must_Return_TheStream_WithoutWaitingFor_TheProcess_ToWrite_Anything()
        {
            const int writeDelayMs = 4000;

            var start = Stopwatch.StartNew();

            Decoder.Run(DecoderArgs.Reading(TestPayload.SourceFile).WriteDelay(writeDelayMs));

            Assert.Less(start.ElapsedMilliseconds, writeDelayMs / 4);
        }

        /// <summary>
        /// A decoder's parameters come from user configuration and can contain spaces, so they have to reach
        /// the process as the single arguments they were given as.
        /// </summary>
        [Test]
        public void Must_Pass_AnArgument_ContainingSpaces_AsOneArgument()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).ProgressMessage("two words"));

            var result = Util.Read(outputStream);

            // A mangled argument is a usage error, which leaves stdout empty
            Assert.AreEqual(TestPayload.Bytes, result);
        }
    }
}
