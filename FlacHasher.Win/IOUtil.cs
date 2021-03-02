using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Win
{
    public static class IOUtil
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

        public static void WriteToFile(FileInfo targetFile, IEnumerable<string> contents)
        {
            File.WriteAllLines(targetFile.FullName, contents);
        }
    }
}