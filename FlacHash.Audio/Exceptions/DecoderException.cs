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
            : base($"Couldn't decode audio. {exception.Message}. See {nameof(ActualException)} for more details", exception)
        {
            ActualException = exception;
        }
    }
}
