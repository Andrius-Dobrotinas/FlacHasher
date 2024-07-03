﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Verification.Source
{
    public class TargetFileResolver
    {
        private readonly string sourceFileExtension;
        private readonly string hashFileExtension;

        public TargetFileResolver(
            string sourceFileExtension,
            string hashFileExtension)
        {
            this.sourceFileExtension = sourceFileExtension;
            this.hashFileExtension = hashFileExtension;
        }

        public (FileInfo[], FileInfo[]) GetFiles(DirectoryInfo directory)
        {
            var files = FileSearch.FindFiles(directory, sourceFileExtension).ToArray();
            var hashFiles = FileSearch.FindFiles(directory, hashFileExtension).ToArray();

            return (files, hashFiles);
        }
    }
}