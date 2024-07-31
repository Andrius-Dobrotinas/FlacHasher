using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Audio
{
    public interface IAudioFileEncoder
    {
        Stream Encode(FileInfo sourceFile, int compressionLevel);
    }
}