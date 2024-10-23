using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.CompressionLevel.Audio
{
    public interface IAudioEncoder
    {
        /// <summary>
        /// Disposes of the provided <paramref name="wavAudio"/> when it completes (regardless of whether it was successful)
        /// </summary>
        Stream Encode(Stream wavAudio, int compressionLevel);
    }
}