using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Andy.FlacHash
{
    public interface IMultipleFileHasher
    {
        /// <summary>
        /// Calculates hashes for each file upon enumeration.
        /// On cancellation, throws <see cref="OperationCanceledException"/> exception
        /// </summary>
        IEnumerable<FileHashResult> ComputeHashes(IEnumerable<FileInfo> files, CancellationToken cancellation = default);
    }

    public class MultipleFileHasher : IMultipleFileHasher
    {
        private readonly IFileHasher hasher;
        private readonly bool continueOnError;

        public MultipleFileHasher(IFileHasher hasher, bool continueOnError)
        {
            this.hasher = hasher;
            this.continueOnError = continueOnError;
        }

        public IEnumerable<FileHashResult> ComputeHashes(IEnumerable<FileInfo> files, CancellationToken cancellation = default)
        {
            return files.Select(
                file => {
                    try
                    {
                        cancellation.ThrowIfCancellationRequested();
                        
                        return new FileHashResult
                        {
                            File = file,
                            Hash = hasher.ComputeHash(file, cancellation)
                        };
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        if (!continueOnError)
                            throw;

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