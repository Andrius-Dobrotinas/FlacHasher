using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.IO.Audio
{
    public interface IAudioDecoder
    {
        Stream Read(Stream wavAudio, CancellationToken cancellation = default);
    }
}