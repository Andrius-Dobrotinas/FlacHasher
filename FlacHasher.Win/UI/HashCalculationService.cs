using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Andy.FlacHash.Win.UI
{
    public class HashCalculationService
    {
        public readonly HashCalcOnSeparateThreadService hashCalcOnSeparateThreadService;
        private readonly CancellableActionRunner cancellableActionRunner;

        public HashCalculationService(
            HashCalcOnSeparateThreadService hashCalcOnSeparateThreadService,
            CancellableActionRunner cancellableActionRunner)
        {
            this.hashCalcOnSeparateThreadService = hashCalcOnSeparateThreadService;
            this.cancellableActionRunner = cancellableActionRunner;
        }

        public bool InProgress => cancellableActionRunner.InProgress;

        public void Start(
            IEnumerable<FileInfo> sourceFiles,
            Action<FileHashResult> hashCalculated)
        {
            cancellableActionRunner.Start(
                (cancellationToken, finishedCallback) => hashCalcOnSeparateThreadService.StartHashCalculation(
                    sourceFiles,
                    cancellationToken,
                    finishedCallback,
                    hashCalculated));
        }

        public void Cancel()
        {
            cancellableActionRunner.Cancel();
        }
    }
}