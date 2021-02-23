using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    /// <summary>
    /// Starts the operation of hash calculation in a new thread with progress reported on a UI thead
    /// </summary>
    public class HashCalcOnSeparateThreadService
    {
        private readonly IReportingMultipleFileHasher hasher;
        private readonly ActionOnNonUiThreadRunner nonUiActionRunner;
        private readonly Control progressReportingContext;

        /// <param name="progressReportingContext">A control that is used as a context for UI updates from an operation running on a separate thread</param>
        public HashCalcOnSeparateThreadService(IReportingMultipleFileHasher hasher,
            ActionOnNonUiThreadRunner nonUiActionRunner,
            Control progressReportingContext)
        {
            this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.nonUiActionRunner = nonUiActionRunner ?? throw new ArgumentNullException(nameof(nonUiActionRunner));
            this.progressReportingContext = progressReportingContext ?? throw new ArgumentNullException(nameof(progressReportingContext));
        }

        public Task StartHashCalculation(IEnumerable<FileInfo> sourceFiles,
            CancellationToken cancellationToken,
            Action reportCompletionInContext,
            Action<FileHashResult> reportHashInContext)
        {
            return nonUiActionRunner.Start(
                reportProgressOnUi => hasher.ComputeHashes(sourceFiles, reportProgressOnUi, cancellationToken),
                progressReportingContext,
                reportHashInContext,
                reportCompletionInContext);
        }
    }
}