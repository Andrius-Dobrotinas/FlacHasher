using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static Andy.FlacHash.Win.CancellableBackgroundOperationRunner;

namespace Andy.FlacHash.Win.UI
{
    public class HashCalculationServiceFactory
    {
        private readonly IReportingMultiFileHasher hasher;

        public HashCalculationServiceFactory(
            IReportingMultiFileHasher hasher)
        {
            this.hasher = hasher;
        }

        public NonBlockingHashComputation Build(
            Control uiUpdateContext,
            CompletionHandler reportCompletion,
            Action<Exception> reportFailure,
            StateChangeHandler stateChanged)
        {
            return new NonBlockingHashComputation(
                new CancellableBackgroundOperationRunner(uiUpdateContext),
                hasher,
                reportCompletion, reportFailure, stateChanged);
        }
    }
}