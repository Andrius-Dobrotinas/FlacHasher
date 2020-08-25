using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.IO.Audio
{
    public interface IAudioFileEncoder
    {
        MemoryStream Encode(FileInfo sourceFile, int compressionLevel);
    }
}