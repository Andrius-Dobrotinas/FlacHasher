using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Win
{
    public class InputFileResolver
    {
        private readonly string sourceFileExtension;
        private readonly string hashFileExtension;

        public InputFileResolver(
            string sourceFileExtension,
            string hashFileExtension)
        {
            this.sourceFileExtension = sourceFileExtension;
            this.hashFileExtension = hashFileExtension;
        }

        public (FileInfo[], FileInfo[]) FindFiles(DirectoryInfo directory)
        {
            var files = FileSearch.FindFiles(directory, sourceFileExtension).ToArray();
            var hashFiles = FileSearch.FindFiles(directory, hashFileExtension).ToArray();

            return (files, hashFiles);
        }
    }
}