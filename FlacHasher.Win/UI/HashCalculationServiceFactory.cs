using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static Andy.FlacHash.Win.CancellableBackgroundOperationRunner;

namespace Andy.FlacHash.Win.UI
{
    public class HashCalculationServiceFactory
    {
        private readonly IReportingMultipleFileHasher hasher;

        public HashCalculationServiceFactory(
            IReportingMultipleFileHasher hasher)
        {
            this.hasher = hasher;
        }

        public NonBlockingHashCalculationService Build(
            Control uiUpdateContext,
            CompletionHandler reportCompletion,
            Action<Exception> reportFailure,
            StateChangeHandler stateChanged)
        {
            return new NonBlockingHashCalculationService(
                new CancellableBackgroundOperationRunner(uiUpdateContext),
                hasher,
                reportCompletion, reportFailure, stateChanged);
        }
    }
}