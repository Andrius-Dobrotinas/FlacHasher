using System;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.IO
{
    public interface IFileReader
    {
        /// <summary>
        /// Returns a specified file as a stream.
        /// </summary>
        Stream Read(FileInfo sourceFile, CancellationToken cancellation = default);
    }
}