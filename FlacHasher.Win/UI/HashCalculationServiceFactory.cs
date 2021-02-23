using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public class HashCalculationServiceFactory
    {
        private readonly IReportingMultipleFileHasher hasher;
        private readonly ActionOnNonUiThreadRunner nonUiActionRunner;

        public HashCalculationServiceFactory(
            IReportingMultipleFileHasher hasher,
            ActionOnNonUiThreadRunner nonUiActionRunner)
        {
            this.hasher = hasher;
            this.nonUiActionRunner = nonUiActionRunner;
        }

        public NonBlockingHashCalculationService Build(
            Control uiUpdateContext,
            CancellableActionRunner.CompletionHandler reportCompletion,
            CancellableActionRunner.StateChangeHandler stateChanged)
        {
            return new NonBlockingHashCalculationService(
                new HashCalcOnSeparateThreadService(hasher, nonUiActionRunner, uiUpdateContext),
                new CancellableActionRunner(reportCompletion, stateChanged));
        }
    }
}