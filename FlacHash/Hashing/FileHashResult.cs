using System;
using System.IO;

namespace Andy.FlacHash.Hashing
{
    public class FileHashResult
    {
        public FileInfo File { get; set; }
        public byte[] Hash { get; set; }
        public Exception Exception { get; set; }
    }
}