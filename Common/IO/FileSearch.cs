using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.IO
{
    public interface IFileSearch
    {
        IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, params string[] fileExtensions);
    }

    public class FileSearch : IFileSearch
    {
        private bool includeHidden;

        public FileSearch(bool includeHidden)
        {
            this.includeHidden = includeHidden;
        }

        public IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, params string[] fileExtensions)
        {
            var settings = new EnumerationOptions
            {
                MatchType = MatchType.Simple,
                RecurseSubdirectories = false,
                MatchCasing = MatchCasing.CaseInsensitive,
                AttributesToSkip = FileAttributes.System | FileAttributes.Directory
            };
            if (!includeHidden)
                settings.AttributesToSkip = settings.AttributesToSkip | FileAttributes.Hidden;

            return FindFiles(directory, fileExtensions, settings);
        }


        public static IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, string fileExtension, EnumerationOptions settings)
        {
            return directory
                .EnumerateFiles(
                    $"*.{fileExtension}",
                    settings);
        }

        public static IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, string[] fileExtensions, EnumerationOptions settings)
        {
            if (fileExtensions.Length == 1)
                return FindFiles(directory, fileExtensions.First(), settings);
            else
            {
                var extensions = fileExtensions.Select(ext => $".{ext}").ToArray();

                return FindFiles(directory, "*", settings)
                    .Where(file => extensions.Contains(file.Extension));
            }
        }
    }
}