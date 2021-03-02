﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Win
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

        public (FileInfo[], FileInfo)? GetFiles(DirectoryInfo directory)
        {
            var allFiles = IOUtil.FindFiles(directory, new string[] { sourceFileFilter, hashFileFilter })
                .GroupBy(x => x.Extension)
                .ToArray()
                .ToDictionary(x => x.Key, x => x.ToArray());

            var files = allFiles.ContainsKey(sourceFileFilter)
                ? allFiles[sourceFileFilter]
                : new FileInfo[0];

            var hashFile = allFiles.ContainsKey(hashFileFilter)
                ? allFiles[hashFileFilter].First()
                : null;

            return (files, hashFile);
        }
    }
}