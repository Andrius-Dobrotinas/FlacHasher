using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Andy.FlacHash.Win.UI
{
    public class NonBlockingHashCalculationService
    {
        public readonly HashCalcOnSeparateThreadService hashCalcOnSeparateThreadService;
        private readonly CancellableActionRunner cancellableActionRunner;

        public NonBlockingHashCalculationService(
            HashCalcOnSeparateThreadService hashCalcOnSeparateThreadService,
            CancellableActionRunner cancellableActionRunner)
        {
            this.hashCalcOnSeparateThreadService = hashCalcOnSeparateThreadService;
            this.cancellableActionRunner = cancellableActionRunner;
        }

        public bool InProgress => cancellableActionRunner.InProgress;

        public Task Start(
            IEnumerable<FileInfo> sourceFiles,
            Action<FileHashResult> hashCalculated)
        {
            return cancellableActionRunner.Start(
                (cancellationToken, finishedCallback, reportFailure) => hashCalcOnSeparateThreadService.StartHashCalculation(
                    sourceFiles,
                    cancellationToken,
                    finishedCallback,
                    hashCalculated,
                    reportFailure));
        }

        public void Cancel()
        {
            cancellableActionRunner.Cancel();
        }
    }
}