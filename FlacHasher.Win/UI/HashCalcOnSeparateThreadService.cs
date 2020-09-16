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
        private readonly IReportingMultipleFileHasher hasher;
        private readonly ActionOnNonUiThreadRunner nonUiActionRunner;

        /// <summary>
        /// A control that is used as a context for UI updates from another thread
        /// </summary>
        public Control UiUpdateContext { get; set; }

        /// <summary>
        /// An event that is fired for each calculated file hash
        /// </summary>
        public event Action<FileHashResult> HashCalculated;

        public HashCalcOnSeparateThreadService(IReportingMultipleFileHasher hasher,
            ActionOnNonUiThreadRunner nonUiActionRunner)
        {
            this.hasher = hasher;
            this.nonUiActionRunner = nonUiActionRunner;
        }

        public void CalculateHashes(IEnumerable<FileInfo> sourceFiles,
            CancellationToken cancellationToken,
            Action finishedCallback)
        {
            if (UiUpdateContext == null) throw new InvalidOperationException($"{nameof(UiUpdateContext)} is not set");
            if (HashCalculated == null) throw new InvalidOperationException($"{nameof(HashCalculated)} event handler is not set");

            if (finishedCallback == null) throw new ArgumentNullException(nameof(finishedCallback));

            nonUiActionRunner.Run(
                reportProgress => hasher.ComputeHashes(sourceFiles, reportProgress, cancellationToken),
                HashCalculated,
                finishedCallback,
                UiUpdateContext);
        }
    }
}