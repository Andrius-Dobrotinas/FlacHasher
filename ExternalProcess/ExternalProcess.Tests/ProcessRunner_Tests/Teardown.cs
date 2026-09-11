using NUnit.Framework;
using System;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    /// <summary>
    /// What happens between the last byte of output and the process actually being gone.
    /// A process closes its stdout as part of winding down, and the operating system doesn't reap it until
    /// slightly afterwards, so the two aren't simultaneous and a short wait can miss a perfectly healthy exit.
    /// </summary>
    [Timeout(30000)]
    public class Teardown
    {
        const string progressText = "still-going";

        /// <summary>
        /// The gap between EOF and the process being reaped is real, and a process that takes a moment
        /// over it has still done its job.
        /// </summary>
        [TestCase(0)]
        [TestCase(300)]
        public void When_TheProcess_TakesAMoment_ToExit_AfterClosingStdOut__Must_Treat_TheRun_AsSuccessful(int lingerMs)
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).Linger(lingerMs),
                exitTimeoutMs: 2000);

            Assert.DoesNotThrow(() => Util.Read(outputStream));
        }

        /// <summary>
        /// The output is all there, but nothing has vouched for it: the process was terminated rather than
        /// having finished, so its exit code is the OS' and says nothing about how the program would have ended.
        /// </summary>
        [Test]
        public void When_TheProcess_WillNotExit_AfterClosingStdOut__Reading_TheWholeStream_Must_Throw_ProcessNotRespondingException()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).Linger(DecoderArgs.WaitForever),
                exitTimeoutMs: 300);

            var exception = Assert.Throws<ProcessNotRespondingException>(() => Util.Read(outputStream));

            Assert.IsNotInstanceOf<ExecutionWithExitCodeException>(exception, "A terminated process has no exit code of its own to report");
        }

        [Test]
        public void When_TheProcess_WillNotExit__TheException_Must_Carry_WhatItWroteToStdErr()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile)
                    .ReadChunkSize(16)
                    .ProgressMessage(progressText)
                    .Linger(DecoderArgs.WaitForever),
                exitTimeoutMs: 300);

            var exception = Assert.Throws<ProcessNotRespondingException>(() => Util.Read(outputStream));

            Assert.True(exception.IsProcessOutputCaptured);
            Assert.That(exception.ProcessErrorOutput, Does.Contain(progressText));
        }

        /// <summary>
        /// Showing the process' output means not redirecting stderr, which leaves nothing to put in the exception.
        /// </summary>
        [Test]
        public void When_TheProcess_WillNotExit__And_TheOutput_IsOnShow__Must_Admit_ToHavingCapturedNothing()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).Linger(DecoderArgs.WaitForever),
                exitTimeoutMs: 300,
                showProcessOutput: true);

            var exception = Assert.Throws<ProcessNotRespondingException>(() => Util.Read(outputStream));

            Assert.False(exception.IsProcessOutputCaptured);
            Assert.IsNull(exception.ProcessErrorOutput);
        }

        /// <summary>
        /// Holding stdout open is not the same as refusing to exit: here the consumer has no EOF yet, so it's
        /// the run that's overdue rather than the teardown, and that's the timeout's business, not this one's.
        /// </summary>
        [Test]
        public void When_TheProcess_HoldsStdOutOpen__Must_Not_Report_It_As_NotResponding()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).KeepStdoutOpen(DecoderArgs.WaitForever),
                timeoutSec: 1,
                exitTimeoutMs: 300);

            Assert.Throws<ProcessTimeoutException>(() => Util.Read(outputStream));
        }
    }
}
