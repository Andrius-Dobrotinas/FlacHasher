using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Win
{
    public static class IOUtil
    {
        public static IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, string fileFilter)
        {
            return directory
                .GetFiles(fileFilter, SearchOption.TopDirectoryOnly);
        }

        public static void WriteToFile(FileInfo targetFile, IEnumerable<string> contents)
        {
            File.WriteAllLines(targetFile.FullName, contents);
        }
    }
}