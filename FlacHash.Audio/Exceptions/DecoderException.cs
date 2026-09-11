using Andy.ExternalProcess;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Audio
{
    /// <summary>
    /// Indicates that an error occurred while decoding audio, not necessarily originating from the decoder process.
    /// </summary>
    public class GenericDecoderException : IOException
    {
        public GenericDecoderException(Exception exception)
            : base($"Error when decoding audio: {exception.Message}", exception)
        {
        }

        protected GenericDecoderException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }

    /// <summary>
    /// Indicates that an audio decoder process exited with an error.
    /// </summary>
    public class DecoderException : GenericDecoderException
    {
        public ExecutionException ActualException { get; init; }

        public DecoderException(ExecutionException exception)
            : base(BuildMessage(exception), exception)
        {
            ActualException = exception;
        }

        /// <summary>
        /// Null when it wasn't captured (stderr wasn't redirected).
        /// </summary>
        public static string GetProcessOutput(DecoderException exception) =>
            exception.ActualException.IsProcessOutputCaptured ? exception.ActualException.ProcessErrorOutput : null;

        static string BuildMessage(ExecutionException exception)
        {
            var exitCodeText = exception is ExecutionWithExitCodeException withExitCode
                ? $" (exit code {withExitCode.ExitCode})"
                : "";

            return $"Couldn't decode audio. {exception.Message}{exitCodeText}";
        }
    }
}
