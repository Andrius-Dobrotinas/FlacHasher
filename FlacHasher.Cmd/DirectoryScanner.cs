using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    public static class DirectoryScanner
    {
        public static IEnumerable<FileInfo> GetFiles(DirectoryInfo directory, string fileSearchPattern)
        {
            return directory.GetFiles($"*.{fileSearchPattern}", SearchOption.TopDirectoryOnly); // TODO: use EnumerationOptions instead and ignore hidden files
        }
    }
}