using Andy.ExternalProcess;

namespace Andy.FlacHash.Application.Audio
{
    /// <summary>
    /// Turns the settings a user writes into the numbers the process runner works in.
    /// Both applications go through here, so seconds-to-milliseconds and kibibytes-to-bytes are each written once.
    /// </summary>
    public static class ProcessRunnerFactory
    {
        /// <summary>
        /// Written by a user, so it's in kibibytes; <see cref="NoLimit"/> keeps everything, at the cost of a buffer that grows with the run.
        /// </summary>
        public const int NoLimit = -1;

        /// <summary>
        /// Nothing worth capturing is measured in fractions of a kibibyte, so 0 means "I didn't choose" rather than "keep nothing"
        /// </summary>
        public const int UseDefaultSize = 0;

        public static ProcessRunner Build(ApplicationSettings settings, bool showProcessOutput)
        {
            return new ProcessRunner(
                ProcessRunner.TimeoutFromSeconds(settings.ProcessTimeoutSec),
                settings.ProcessExitTimeoutMs,
                settings.ProcessStartDelayMs,
                ToErrorOutputBytes(settings.DecoderInfoOutputMaxSizeKb),
                showProcessOutput);
        }

        public static int ToErrorOutputBytes(int maxSizeKb)
        {
            if (maxSizeKb == NoLimit)
                return ProcessRunner.UnboundedErrorOutput;

            if (maxSizeKb == UseDefaultSize)
                return ProcessRunner.DefaultMaxErrorOutputBytes;

            if (maxSizeKb < 0)
                throw new ConfigurationException($"{nameof(ApplicationSettings.DecoderInfoOutputMaxSizeKb)} has to be a size in kibibytes, or {NoLimit} for no limit. Given: {maxSizeKb}");

            return maxSizeKb * 1024;
        }
    }
}
