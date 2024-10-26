﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.IO
{
    public interface IFileSearch
    {
        IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, string fileExtension);
    }

    public class FileSearch : IFileSearch
    {
        private bool includeHidden;

        public FileSearch(bool includeHidden)
        {
            this.includeHidden = includeHidden;
        }

        public IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, string fileExtension)
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

            return FindFiles(directory, fileExtension, settings);
        }


        public static IEnumerable<FileInfo> FindFiles(DirectoryInfo directory, string fileExtension, EnumerationOptions settings)
        {
            return directory
                .EnumerateFiles(
                    $"*.{fileExtension}",
                    settings);
        }
    }
}