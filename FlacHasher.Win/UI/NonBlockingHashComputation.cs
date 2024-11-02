using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Andy.FlacHash.Hashing;
using static Andy.FlacHash.Application.Win.CancellableBackgroundOperationRunner;

namespace Andy.FlacHash.Application.Win.UI
{
    public class NonBlockingHashComputation
    {
        private readonly CancellableBackgroundOperationRunner operationRunner;
        private readonly IReportingMultiFileHasher hasher;
        private readonly CompletionHandler reportCompletion;
        private readonly Action<Exception> reportFailure;
        private readonly StateChangeHandler stateTransitioned;

        public NonBlockingHashComputation(
            CancellableBackgroundOperationRunner operationRunner,
            IReportingMultiFileHasher hasher,
            CompletionHandler reportCompletion, 
            Action<Exception> reportFailure, 
            StateChangeHandler stateTransitioned)
        {
            this.operationRunner = operationRunner ?? throw new ArgumentNullException(nameof(operationRunner));
            this.hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.reportCompletion = reportCompletion ?? throw new ArgumentNullException(nameof(reportCompletion));
            this.reportFailure = reportFailure ?? throw new ArgumentNullException(nameof(reportFailure));
            this.stateTransitioned = stateTransitioned ?? throw new ArgumentNullException(nameof(stateTransitioned));
        }

        public bool InProgress => operationRunner.InProgress;

        public Task Start(
            IEnumerable<FileInfo> sourceFiles,
            Action<FileHashResult> hashComputed)
        {
            return operationRunner.Start(
                hashComputed,
                reportCompletion,
                reportFailure,
                stateTransitioned,
                (report, cancel) => hasher.ComputeHashes(sourceFiles, report, cancel));
        }

        public void Cancel()
        {
            operationRunner.Cancel();
        }
    }
}