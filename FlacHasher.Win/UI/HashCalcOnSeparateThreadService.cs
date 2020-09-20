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
        private readonly Control uiUpdateContext;

        /// <param name="uiUpdateContext">A control that is used as a context for UI updates from another thread</param>
        public HashCalcOnSeparateThreadService(IReportingMultipleFileHasher hasher,
            ActionOnNonUiThreadRunner nonUiActionRunner,
            Control uiUpdateContext)
        {
            this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.nonUiActionRunner = nonUiActionRunner ?? throw new ArgumentNullException(nameof(nonUiActionRunner));
            this.uiUpdateContext = uiUpdateContext ?? throw new ArgumentNullException(nameof(uiUpdateContext));
        }

        public void CalculateHashes(IEnumerable<FileInfo> sourceFiles,
            CancellationToken cancellationToken,
            Action finishedCallback,
            Action<FileHashResult> hashCalculated)
        {
            if (hashCalculated == null) throw new ArgumentNullException(nameof(hashCalculated));

            if (finishedCallback == null) throw new ArgumentNullException(nameof(finishedCallback));

            nonUiActionRunner.Run(
                reportProgressOnUi => hasher.ComputeHashes(sourceFiles, reportProgressOnUi, cancellationToken),
                hashCalculated,
                finishedCallback,
                uiUpdateContext);
        }
    }
}