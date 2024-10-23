using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Andy.FlacHash.Hashing;
using static Andy.FlacHash.Application.Win.CancellableBackgroundOperationRunner;

namespace Andy.FlacHash.Application.Win.UI
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