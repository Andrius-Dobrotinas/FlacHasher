using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash
{
    public class DirectoryHasher
    {
        private readonly MultipleFileHasher multiHasher;

        public DirectoryHasher(MultipleFileHasher multiHasher)
        {
            this.multiHasher = multiHasher;
        }

        public IEnumerable<FileHashResult> ComputeHashes(DirectoryInfo directory, string fileSearchPattern)
        {
            var files = directory.GetFiles(fileSearchPattern, SearchOption.TopDirectoryOnly); // TODO: use EnumerationOptions instead and ignore hidden files

            return multiHasher.ComputeHashes(files);
        }
    }
}