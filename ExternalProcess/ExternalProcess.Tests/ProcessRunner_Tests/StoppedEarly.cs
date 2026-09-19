using NUnit.Framework;
using System;
using System.IO;
using System.Threading;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    /// <summary>
    /// A run that has to be stopped part way, either because it was asked for or because it overran its allowance.
    /// Both end the same way - the process is killed while the consumer is still reading it - and the killing is
    /// what unblocks that read.
    /// </summary>
    [Timeout(60000)]
    public class StoppedEarly
    {
        const string progressText = "GOT-THIS-FAR";

        /// <summary>
        /// Stalls before it has written everything, so a consumer is left waiting mid-stream with stdout still open.
        /// </summary>
        static DecoderArgs StallsPartWayThrough()
        {
            return DecoderArgs.Reading(TestPayload.LargeSourceFile)
                .ReadChunkSize(64)
                .WriteDelay(DecoderArgs.WaitForever);
        }

        /// <summary>
        /// Has written everything but holds stdout open, so the consumer waits for an end of stream that never comes.
        /// </summary>
        static DecoderArgs NeverSignalsTheEnd()
        {
            return DecoderArgs.Reading(TestPayload.SourceFile)
                .KeepStdoutOpen(DecoderArgs.WaitForever);
        }

        [Test]
        public void When_TheProcess_StallsPartWayThrough__Must_TimeOut_And_Throw_ProcessTimeoutException()
        {
            var outputStream = Decoder.Run(StallsPartWayThrough(), timeoutSec: 1);

            var exception = Assert.Throws<ProcessTimeoutException>(() => Util.Read(outputStream));

            Assert.IsNotInstanceOf<ExecutionWithExitCodeException>(exception, "A terminated process has no exit code of its own to report");
        }

        [Test]
        public void When_TheProcess_NeverSignalsTheEndOfItsOutput__Must_TimeOut_And_Throw_ProcessTimeoutException()
        {
            var outputStream = Decoder.Run(NeverSignalsTheEnd(), timeoutSec: 1);

            Assert.Throws<ProcessTimeoutException>(() => Util.Read(outputStream));
        }

        [Test]
        public void When_TimedOut__TheException_Must_Carry_WhatTheProcessWroteToStdErr()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.LargeSourceFile)
                    .ReadChunkSize(64)
                    .ProgressMessage(progressText)
                    .KeepStdoutOpen(DecoderArgs.WaitForever),
                timeoutSec: 1);

            var exception = Assert.Throws<ProcessTimeoutException>(() => Util.Read(outputStream));

            Assert.True(exception.IsProcessOutputCaptured);
            Assert.That(exception.ProcessErrorOutput, Does.Contain(progressText));
        }

        /// <summary>
        /// Showing the process' output means not redirecting stderr, which leaves nothing to put in the exception.
        /// </summary>
        [Test]
        public void When_TimedOut__And_TheOutput_IsOnShow__Must_Admit_ToHavingCapturedNothing()
        {
            var outputStream = Decoder.Run(NeverSignalsTheEnd(), timeoutSec: 1, showProcessOutput: true);

            var exception = Assert.Throws<ProcessTimeoutException>(() => Util.Read(outputStream));

            Assert.False(exception.IsProcessOutputCaptured);
            Assert.IsNull(exception.ProcessErrorOutput);
        }

        [Test]
        public void When_TheProcess_FinishesWithinItsAllowance__Must_Not_TimeOut()
        {
            var outputStream = Decoder.Run(DecoderArgs.Reading(TestPayload.SourceFile).Expand(3), timeoutSec: 30);

            Assert.DoesNotThrow(() => Util.Read(outputStream));
        }

        [Test]
        public void When_CancellationIsRequested_WhileTheProcess_IsStalled__Must_Throw_OperationCanceledException()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var outputStream = Decoder.Run(StallsPartWayThrough(), cancellation: cancellation.Token);

                cancellation.CancelAfter(300);

                Assert.Throws<OperationCanceledException>(() => Util.Read(outputStream));
            }
        }

        [Test]
        public void When_CancellationIsRequested_WhileTheProcess_HoldsStdOutOpen__Must_Throw_OperationCanceledException()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var outputStream = Decoder.Run(NeverSignalsTheEnd(), cancellation: cancellation.Token);

                cancellation.CancelAfter(300);

                Assert.Throws<OperationCanceledException>(() => Util.Read(outputStream));
            }
        }

        [Test]
        public void When_CancellationIsRequested_WhileFeedingTheProcess__Must_Throw_OperationCanceledException()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var outputStream = Decoder.Run(
                    DecoderArgs.ReadingStdin().ReadChunkSize(64).WriteDelay(DecoderArgs.WaitForever),
                    new MemoryStream(TestPayload.LargeBytes),
                    cancellation: cancellation.Token);

                cancellation.CancelAfter(300);

                Assert.Throws<OperationCanceledException>(() => Util.Read(outputStream));
            }
        }

        /// <summary>
        /// Cancelling before the process has produced anything leaves the consumer blocked on a first read
        /// that no data will ever satisfy, so only the killing can end it.
        /// </summary>
        [Test]
        public void When_CancellationIsRequested_BeforeAnyOutput__Must_Throw_OperationCanceledException()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var outputStream = Decoder.Run(
                    DecoderArgs.Reading(TestPayload.SourceFile).WriteDelay(DecoderArgs.WaitForever),
                    cancellation: cancellation.Token);

                cancellation.CancelAfter(300);

                Assert.Throws<OperationCanceledException>(() => Util.Read(outputStream));
            }
        }

        /// <summary>
        /// Cancelling a run that has already finished has nothing left to stop.
        /// </summary>
        [Test]
        public void When_CancellationIsRequested_AfterTheRunIsOver__Must_Not_Throw()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var outputStream = Decoder.Run(DecoderArgs.Reading(TestPayload.SourceFile), cancellation: cancellation.Token);

                Util.Read(outputStream);

                Assert.DoesNotThrow(() => cancellation.Cancel());
            }
        }
    }
}
