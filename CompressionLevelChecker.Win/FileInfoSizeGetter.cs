using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public interface IFileInfoSizeGetter
    {
        long GetSize(FileInfo fileInfo);
    }

    public class FileInfoSizeGetter : IFileInfoSizeGetter
    {
        public long GetSize(FileInfo fileInfo)
        {
            return fileInfo.Length;
        }
    }
}