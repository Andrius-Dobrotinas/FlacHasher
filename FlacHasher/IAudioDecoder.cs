using System;
using System.IO;

namespace FlacHasher
{
    public interface IAudioDecoder
    {
        Stream Decode(string sourceFilePath);
    }
}