using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash
{
    public interface IMultipleFileHasher
    {
        /// <summary>
        /// Calculates hashes for each file upon enumeration
        /// </summary>
        IEnumerable<FileHashResult> ComputeHashes(IEnumerable<FileInfo> files);
    }

    public class MultipleFileHasher : IMultipleFileHasher
    {
        private readonly IFileHasher hasher;

        public MultipleFileHasher(IFileHasher hasher)
        {
            this.hasher = hasher;
        }

        public IEnumerable<FileHashResult> ComputeHashes(IEnumerable<FileInfo> files)
        {
            return files.Select(
                file => {
                    try
                    {
                        return new FileHashResult
                        {
                            File = file,
                            Hash = hasher.ComputerHash(file)
                        };
                    }
                    catch (Exception e)
                    {
                        return new FileHashResult
                        {
                            File = file,
                            Exception = e
                        };
                    }
                });
        }
    }
}