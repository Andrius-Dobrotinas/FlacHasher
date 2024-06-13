using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Verification.Source
{
    public class TargetFileResolver
    {
        private readonly string sourceFileFilter;
        private readonly string hashFileFilter;

        public TargetFileResolver(
            string sourceFileFilter,
            string hashFileFilter)
        {
            this.sourceFileFilter = sourceFileFilter;
            this.hashFileFilter = hashFileFilter;
        }

        public (FileInfo[], FileInfo[]) GetFiles(DirectoryInfo directory)
        {
            var allFiles = FileUtil.FindFiles(directory, new string[] { sourceFileFilter, hashFileFilter })
                .GroupBy(x => x.Extension)
                .ToArray()
                .ToDictionary(x => x.Key, x => x.ToArray());

            var files = allFiles.ContainsKey(sourceFileFilter)
                ? allFiles[sourceFileFilter]
                : new FileInfo[0];

            var hashFiles = allFiles.ContainsKey(hashFileFilter)
                ? allFiles[hashFileFilter]
                : null;

            return (files, hashFiles);
        }
    }
}