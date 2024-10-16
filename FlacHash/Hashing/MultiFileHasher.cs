using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Andy.FlacHash.Hashing
{
    public interface IMultiFileHasher
    {
        /// <summary>
        /// Calculates hashes for each file upon enumeration.
        /// On cancellation, throws <see cref="OperationCanceledException"/> exception
        /// </summary>
        IEnumerable<FileHashResult> ComputeHashes(IEnumerable<FileInfo> files, CancellationToken cancellation = default);
    }

    public class MultiFileHasher : IMultiFileHasher
    {
        private readonly IFileHasher hasher;
        private readonly bool continueOnError;

        public MultiFileHasher(IFileHasher hasher, bool continueOnError)
        {
            this.hasher = hasher;
            this.continueOnError = continueOnError;
        }

        public IEnumerable<FileHashResult> ComputeHashes(IEnumerable<FileInfo> files, CancellationToken cancellation = default)
        {
            return files.Select(
                file =>
                {
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
                    // If it's purely a problem decoding the file, then it can move on to the next one (if configured); otherwise, something bigger is up
                    catch (Audio.IOException e)
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