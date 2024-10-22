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
        private readonly IProgress<int> inputStreamReadProgressReporter;

        public MultiFileHasher(IFileHasher hasher, bool continueOnError) : this(hasher, continueOnError, null)
        {
        }

        /// <summary>
        /// Wires it up to report bytes processed after each file via <paramref name="inputStreamReadProgressReporter"/>
        /// </summary>
        /// <param name="inputStreamReadProgressReporter"></param>
        public MultiFileHasher(IFileHasher hasher, bool continueOnError, IProgress<int> inputStreamReadProgressReporter)
        {
            this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.continueOnError = continueOnError;
            this.inputStreamReadProgressReporter = inputStreamReadProgressReporter;
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
                    finally
                    {
                        if (inputStreamReadProgressReporter != null)
                            inputStreamReadProgressReporter.Report(
                                file.Exists 
                                ? (int)file.Length // int.Max is big enough for normal audio files
                                : 0);
                    }
                });
        }
    }
}