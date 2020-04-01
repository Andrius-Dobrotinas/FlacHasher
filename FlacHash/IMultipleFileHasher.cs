using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash
{
    public interface IMultipleFileHasher
    {
        IEnumerable<FileHashResult> ComputeHashes(IEnumerable<FileInfo> files);
    }
}