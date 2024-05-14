using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash
{
    public interface IReportingMultipleFileHasher
    {
        /// <summary>
        /// Calculates hashes for each file and reports the hash as soon as it's calculated
        /// </summary>
        void ComputeHashes(
            IEnumerable<FileInfo> files,
            Action<FileHashResult> reportHash,
            CancellationToken cancellationToken);
    }

    public class ReportingMultipleFileHasher : IReportingMultipleFileHasher
    {
        private readonly IMultipleFileHasher hasher;

        public ReportingMultipleFileHasher(IMultipleFileHasher hasher)
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

            IEnumerable<FileHashResult> computations = hasher.ComputeHashes(files);

            //just in case the op is cancelled right away-ish. you don't want to even start the enumeration in that case
            if (cancellationToken.IsCancellationRequested) return;

            foreach (var result in computations)
            {
                if (cancellationToken.IsCancellationRequested) return;

                reportHash(result);
            }
        }
    }
}