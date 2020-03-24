using System;
using System.IO;

namespace FlacHasher
{
    public interface IAudioDecoder
    {
        Stream Decode(FileInfo sourceFile);
    }
}