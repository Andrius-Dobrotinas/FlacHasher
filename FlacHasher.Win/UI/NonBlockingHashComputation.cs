using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static Andy.FlacHash.Win.CancellableBackgroundOperationRunner;

namespace Andy.FlacHash.Win.UI
{
    public class NonBlockingHashComputation
    {
        private readonly CancellableBackgroundOperationRunner operationRunner;
        private readonly IReportingMultipleFileHasher hasher;
        private readonly CompletionHandler reportCompletion;
        private readonly Action<Exception> reportFailure;
        private readonly StateChangeHandler stateChanged;

        public NonBlockingHashComputation(
            CancellableBackgroundOperationRunner operationRunner,
            IReportingMultipleFileHasher hasher,
            CompletionHandler reportCompletion, 
            Action<Exception> reportFailure, 
            StateChangeHandler stateChanged)
        {
            this.operationRunner = operationRunner ?? throw new ArgumentNullException(nameof(operationRunner));
            this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.reportCompletion = reportCompletion ?? throw new ArgumentNullException(nameof(reportCompletion));
            this.reportFailure = reportFailure ?? throw new ArgumentNullException(nameof(reportFailure));
            this.stateChanged = stateChanged ?? throw new ArgumentNullException(nameof(stateChanged));
        }

        public bool InProgress => operationRunner.InProgress;

        public Task Start(
            IEnumerable<FileInfo> sourceFiles,
            Action<FileHashResult> hashCalculated)
        {
            return operationRunner.Start(
                hashCalculated,
                reportCompletion,
                reportFailure,
                stateChanged,
                (report, cancel) => hasher.ComputeHashes(sourceFiles, report, cancel));
        }

        public void Cancel()
        {
            operationRunner.Cancel();
        }
    }
}