using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Verification.Source
{
    public static class FileUtil
    {
        public static IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, string fileFilter)
        {
            return directory
                .GetFiles(fileFilter, SearchOption.TopDirectoryOnly);
        }

        public static IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, IList<string> fileExtensions)
        {
            return directory
                .EnumerateFiles()
                .Where(file => fileExtensions.Contains(file.Extension));
        }
    }
}