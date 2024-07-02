using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash
{
    public interface IReportingMultiFileHasher
    {
        /// <summary>
        /// Calculates hashes for each file and reports the hash as soon as it's calculated.
        /// On cancellation, returns quietly without exceptions.
        /// </summary>
        void ComputeHashes(
            IEnumerable<FileInfo> files,
            Action<FileHashResult> reportHash,
            CancellationToken cancellationToken);
    }

    public class ReportingMultiFileHasher : IReportingMultiFileHasher
    {
        private readonly IMultipleFileHasher hasher;

        public ReportingMultiFileHasher(IMultipleFileHasher hasher)
        {
            this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public void ComputeHashes(
            IEnumerable<FileInfo> files,
            Action<FileHashResult> reportHash,
            CancellationToken cancellationToken)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (reportHash == null) throw new ArgumentNullException(nameof(reportHash));

            IEnumerable<FileHashResult> computations = hasher.ComputeHashes(files, cancellationToken);

            //just in case the op is cancelled right away-ish. you don't want to even start the enumeration in that case
            if (cancellationToken.IsCancellationRequested) return;

            try
            {
                foreach (var result in computations)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    reportHash(result);

                    //so it doesn't go back to the enumerator, which would result in launching decoding the next file
                    if (cancellationToken.IsCancellationRequested) return;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}