﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Audio.Compression
{
    public interface IAudioFileEncoder
    {
        MemoryStream Encode(FileInfo sourceFile);
    }
}