using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Runs the fake decoder through the only public way in.
    /// </summary>
    static class Decoder
    {
        public const int NoTimeout = ProcessRunner.NoTimeoutValue;

        /// <summary>
        /// Long enough that a healthy process is reaped well within it, which is what production configures (1000 ms).
        /// </summary>
        public const int ExitTimeoutMs = 1000;

        /// <summary>
        /// Production waits 100 ms here before touching the child's streams. Whether that's needed at all is its own
        /// question; until it's answered, tests don't pay for it.
        /// </summary>
        public const int StartWaitMs = 0;

        public static ProcessOutputStream Run(
            DecoderArgs arguments,
            int timeoutSec = NoTimeout,
            int exitTimeoutMs = ExitTimeoutMs,
            bool showProcessOutput = false,
            int maxErrorOutputBytes = ProcessRunner.DefaultMaxErrorOutputBytes,
            CancellationToken cancellation = default)
        {
            return Build(timeoutSec, exitTimeoutMs, showProcessOutput, maxErrorOutputBytes)
                .RunAndReadOutput(TestEnvironment.DecoderExecutable, arguments.Build(), cancellation);
        }

        public static ProcessOutputStream Run(
            DecoderArgs arguments,
            Stream input,
            int timeoutSec = NoTimeout,
            int exitTimeoutMs = ExitTimeoutMs,
            bool showProcessOutput = false,
            int maxErrorOutputBytes = ProcessRunner.DefaultMaxErrorOutputBytes,
            CancellationToken cancellation = default)
        {
            return Build(timeoutSec, exitTimeoutMs, showProcessOutput, maxErrorOutputBytes)
                .RunAndReadOutput(TestEnvironment.DecoderExecutable, arguments.Build(), input, cancellation);
        }

        static ProcessRunner Build(int timeoutSec, int exitTimeoutMs, bool showProcessOutput, int maxErrorOutputBytes)
        {
            return new ProcessRunner(
                ProcessRunner.TimeoutFromSeconds(timeoutSec),
                exitTimeoutMs,
                StartWaitMs,
                maxErrorOutputBytes,
                showProcessOutput);
        }
    }
}
