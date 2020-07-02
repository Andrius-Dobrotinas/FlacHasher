using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO.Audio
{
    public interface IAudioDecoder
    {
        Stream Read(Stream wavAudio);
    }
}