using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Andy.FlacHash.Hashing;

namespace Andy.FlacHash.Win
{
    public class InputFileResolver
    {
        private readonly string sourceFileExtension;
        private readonly ICollection<string> hashFileExtensions;
        private readonly FileSearch fileSearch;

        public InputFileResolver(
            string sourceFileExtension,
            ICollection<string> hashFileExtensions,
            FileSearch fileSearch)
        {
            this.sourceFileExtension = sourceFileExtension;
            this.hashFileExtensions = hashFileExtensions;
            this.fileSearch = fileSearch;
        }

        public (FileInfo[], FileInfo[]) FindFiles(DirectoryInfo directory)
        {
            var files = fileSearch.FindFiles(directory, sourceFileExtension).ToArray();
            var hashFiles = fileSearch.FindFiles(directory, "*")
                .Where(file => hashFileExtensions.Contains(file.Extension))
                .ToArray();

            return (files, hashFiles);
        }
    }
}