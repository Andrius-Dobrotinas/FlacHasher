using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash
{
    public static class FileSearch
    {
        public static IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, string fileExtension)
        {
            return directory
                .EnumerateFiles(
                    $"*.{fileExtension}",
                    new EnumerationOptions
                    {
                        MatchType = MatchType.Simple,
                        RecurseSubdirectories = false,
                        AttributesToSkip = FileAttributes.Hidden, // TODO: make configurable
                        MatchCasing = MatchCasing.CaseInsensitive, // TODO: make configurable
                    });
        }
    }
}