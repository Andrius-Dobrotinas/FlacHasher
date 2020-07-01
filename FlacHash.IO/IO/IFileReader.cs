using System;
using System.IO;

namespace Andy.FlacHash.IO
{
    public interface IFileReader
    {
        /// <summary>
        /// Returns a specified file as a stream.
        /// </summary>
        Stream Read(FileInfo sourceFile);
    }
}