using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public class HashCalculationServiceFactory
    {
        private readonly IReportingMultipleFileHasher hasher;
        private readonly ProgressReportingOperationRunner nonUiActionRunner;

        public HashCalculationServiceFactory(
            IReportingMultipleFileHasher hasher,
            ProgressReportingOperationRunner nonUiActionRunner)
        {
            this.hasher = hasher;
            this.nonUiActionRunner = nonUiActionRunner;
        }

        public NonBlockingHashCalculationService Build(
            Control uiUpdateContext,
            CancellableActionRunner.CompletionHandler reportCompletion,
            Action<Exception> reportFailure,
            CancellableActionRunner.StateChangeHandler stateChanged)
        {
            return new NonBlockingHashCalculationService(
                new HashCalcOnSeparateThreadService(hasher, nonUiActionRunner, uiUpdateContext),
                new CancellableActionRunner(reportCompletion, reportFailure, stateChanged));
        }
    }
}