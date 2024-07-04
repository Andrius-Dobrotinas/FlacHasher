using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Win
{
    public class InputFileResolver
    {
        private readonly string sourceFileExtension;
        private readonly ICollection<string> hashFileExtensions;

        public InputFileResolver(
            string sourceFileExtension,
            ICollection<string> hashFileExtensions)
        {
            this.sourceFileExtension = sourceFileExtension;
            this.hashFileExtensions = hashFileExtensions;
        }

        public (FileInfo[], FileInfo[]) FindFiles(DirectoryInfo directory)
        {
            var files = FileSearch.FindFiles(directory, sourceFileExtension).ToArray();
            var hashFiles = FileSearch.FindFiles(directory, "*")
                .Where(file => hashFileExtensions.Contains(file.Extension))
                .ToArray();

            return (files, hashFiles);
        }
    }
}