using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Win
{
    public static class IOUtil
    {
        public static void WriteToFile(FileInfo targetFile, IEnumerable<string> contents)
        {
            File.WriteAllLines(targetFile.FullName, contents);
        }
    }
}