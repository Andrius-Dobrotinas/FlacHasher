using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash
{
    public class MultipleFileHasher
    {
        private readonly IFileHasher hasher;

        public MultipleFileHasher(IFileHasher hasher)
        {
            this.hasher = hasher;
        }

        public IEnumerable<FileHashResult> ComputeHashes(IEnumerable<FileInfo> files)
        {
            return files.Select(
                file => new FileHashResult
                {
                    File = file,
                    Hash = hasher.ComputerHash(file)
                });
        }
    }
}