using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Application.Win
{
    public class InputFileResolver
    {
        private readonly ICollection<string> hashFileExtensions;
        private readonly FileSearch fileSearch;

        public InputFileResolver(
            ICollection<string> hashFileExtensions,
            FileSearch fileSearch)
        {
            this.hashFileExtensions = hashFileExtensions;
            this.fileSearch = fileSearch;
        }

        public (FileInfo[], FileInfo[]) FindFiles(DirectoryInfo directory, string sourceFileExtension)
        {
            var files = fileSearch.FindFiles(directory, sourceFileExtension).ToArray();
            var hashFiles = fileSearch.FindFiles(directory, "*")
                .Where(file => hashFileExtensions.Contains(file.Extension))
                .ToArray();

            return (files, hashFiles);
        }
    }
}