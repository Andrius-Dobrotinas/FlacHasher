using System;

namespace Andy.FlacHash.IO.Audio
{
    public class AudioCompressionException : IOException
    {
        public AudioCompressionException(string msg) : base(msg)
        {

        }
    }
}