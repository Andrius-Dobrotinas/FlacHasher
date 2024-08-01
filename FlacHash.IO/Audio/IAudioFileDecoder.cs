using System;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.Audio
{
    public interface IAudioFileDecoder
    {
        /// <summary>
        /// Returns a specified file as a decoded audio byte stream.
        /// </summary>
        Stream Read(FileInfo sourceFile, CancellationToken cancellation = default);
    }
}