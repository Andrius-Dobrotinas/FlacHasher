using System;
using System.IO;

namespace Andy.FlacHash
{
    public interface IFileHasher
    {
        byte[] ComputerHash(FileInfo sourceFile);
    }
}