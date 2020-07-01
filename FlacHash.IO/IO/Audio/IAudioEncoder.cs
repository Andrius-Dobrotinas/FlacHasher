using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO.Audio
{
    public interface IAudioEncoder
    {
        MemoryStream Encode(Stream wavAudio, uint compressionLevel);
    }
}