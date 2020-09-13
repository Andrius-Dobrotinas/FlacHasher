using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    /// <summary>
    /// Calculates hash on a new thread and returns results via an event fired on a UI thread
    /// </summary>
    public class HashCalcOnSeparateThreadService
    {
        private readonly IMultipleFileHasher hasher;
        private readonly ActionOnNonUiThreadRunner nonUiActionRunner;

        /// <summary>
        /// A control that is used as a context for UI updates from another thread
        /// </summary>
        public Control UiUpdateContext { get; set; }

        /// <summary>
        /// An event that is fired for each calculated file hash
        /// </summary>
        public event Action<FileHashResult> OnHashCalculated;

        public event Action OnFinished;

        public HashCalcOnSeparateThreadService(IMultipleFileHasher hasher,
            ActionOnNonUiThreadRunner nonUiActionRunner)
        {
            this.hasher = hasher;
            this.nonUiActionRunner = nonUiActionRunner;
        }

        public void CalculateHashes(IEnumerable<FileInfo> sourceFiles, CancellationToken cancellationToken)
        {
            if (UiUpdateContext == null) throw new ArgumentNullException(nameof(UiUpdateContext), "The value must be provided via the public property");

            if (OnHashCalculated == null) throw new ArgumentNullException(nameof(OnHashCalculated), "The value must be provided via the public property");

            if (OnFinished == null) throw new ArgumentNullException(nameof(OnFinished), "The value must be provided via the public property");

            nonUiActionRunner.Run(
                reportProgress => CalcHashesAndReportOnUIThread(sourceFiles, reportProgress, cancellationToken),
                OnHashCalculated,
                OnFinished,
                UiUpdateContext);
        }

        private void CalcHashesAndReportOnUIThread(
            IEnumerable<FileInfo> files,
            Action<FileHashResult> reportHash,
            CancellationToken cancellationToken)
        {
            IEnumerable<FileHashResult> results = hasher.ComputeHashes(files);

            //just in case the op is cancelled right away-ish. you don't want to even start the enumeration in that case
            if (cancellationToken.IsCancellationRequested) return;

            foreach (var result in results)
            {
                if (cancellationToken.IsCancellationRequested) return;

                reportHash(result);
            }
        }
    }
}