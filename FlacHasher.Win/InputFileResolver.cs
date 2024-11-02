using Andy.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Application.Win
{
    public class InputFileResolver
    {
        private readonly ICollection<string> hashFileExtensions;
        private readonly IFileSearch fileSearch;

        public InputFileResolver(
            ICollection<string> hashFileExtensions,
            IFileSearch fileSearch)
        {
            this.hashFileExtensions = hashFileExtensions;
            this.fileSearch = fileSearch;
        }

        public FileInfo[] FindFiles(DirectoryInfo directory, string sourceFileExtension)
        {
            return fileSearch.FindFiles(directory, sourceFileExtension).ToArray();
        }

        public FileInfo[] GetHashfile(DirectoryInfo directory)
        {
            return fileSearch.FindFiles(directory, "*")
                .Where(file => hashFileExtensions.Contains(file.Extension))
                .ToArray();
        }
    }
}