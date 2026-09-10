using NUnit.Framework;
using System;
using System.IO;

namespace Andy.ExternalProcess.ProcessRunner_Tests
{
    /// <summary>
    /// How the process' exit is reported to whoever is reading the stream.
    /// </summary>
    [Timeout(30000)]
    public class Exit
    {
        const string errorText = "The decoder didn't like that one bit";

        /// <summary>
        /// A Ctrl+C'd child reports the console-interrupt status on Windows and SIGINT's 128 + 2 on Unix.
        /// Unix keeps only the low byte of an exit status, so the Windows value can't occur there at all.
        /// </summary>
        static int CtrlCExitCode => OperatingSystem.IsWindows() ? -1073741510 : 130;

        [TestCase(1)]
        [TestCase(42)]
        public void When_TheProcess_Exits_WithAnErrorCode__Reading_TheWholeStream_Must_Throw_ExecutionException_CarryingTheCode(int exitCode)
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).ExitCode(exitCode));

            var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            Assert.AreEqual(exitCode, exception.ExitCode);
        }

        [Test]
        public void When_TheProcess_Exits_Successfully__Reading_TheWholeStream_Must_Not_Throw()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).ExitCode(0).SuccessMessage("All good"));

            Assert.DoesNotThrow(() => Util.Read(outputStream));
        }

        [Test]
        public void When_StdErr_IsCaptured__TheException_Must_Carry_WhatTheProcessWroteToIt()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).ExitCode(3).ErrorMessage(errorText),
                showProcessOutput: false);

            var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            Assert.True(exception.IsProcessOutputCaptured);
            Assert.That(exception.ProcessErrorOutput, Does.Contain(errorText));
        }

        /// <summary>
        /// Showing the process' output means not redirecting stderr, which leaves nothing to put in the exception.
        /// </summary>
        [Test]
        public void When_TheProcessOutput_IsOnShow__TheException_Must_Admit_ToHavingCapturedNothing()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).ExitCode(3).ErrorMessage(errorText),
                showProcessOutput: true);

            var exception = Assert.Throws<ExecutionException>(() => Util.Read(outputStream));

            Assert.False(exception.IsProcessOutputCaptured);
            Assert.IsNull(exception.ProcessErrorOutput);
        }

        /// <summary>
        /// A cmd-line application relays a Ctrl+C to the process it spawned, which is a cancellation rather than a failure.
        /// </summary>
        [Test]
        public void When_TheProcess_Exits_WithTheCtrlCCode__Reading_TheWholeStream_Must_Throw_OperationCanceledException()
        {
            var outputStream = Decoder.Run(
                DecoderArgs.Reading(TestPayload.SourceFile).ExitCode(CtrlCExitCode));

            Assert.Throws<OperationCanceledException>(() => Util.Read(outputStream));
        }

        /// <summary>
        /// There's no stream to fail on later: starting is the first thing that happens, and it throws into the caller's face.
        /// </summary>
        [Test]
        public void When_TheExecutable_CannotBeRun__Must_Throw_RightAway_FromTheCall_Itself()
        {
            var missing = new FileInfo(
                Path.Combine(Path.GetTempPath(), $"no-such-decoder-{Guid.NewGuid():N}.exe"));

            var target = new ProcessRunner(Decoder.NoTimeout, Decoder.ExitTimeoutMs, Decoder.StartWaitMs, ProcessRunner.DefaultMaxErrorOutputBytes, showProcessOutput: false);

            Assert.Throws<System.ComponentModel.Win32Exception>(
                () => target.RunAndReadOutput(missing, Array.Empty<string>()));
        }
    }
}
