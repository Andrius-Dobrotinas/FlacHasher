using System;
using System.IO;

namespace Andy.FlacHash
{
    public interface IFileReader
    {
        /// <summary>
        /// Returns the specified file as a stream.
        /// </summary>
        Stream Read(FileInfo sourceFile);
    }
}