using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Audio
{
    public class DecoderException : IOException
    {
        public DecoderException(Exception exception)
            : base($"Error decoding audio: {exception.Message}", exception)
        {
        }
    }
}
