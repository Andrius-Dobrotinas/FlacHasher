using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Andy.FlacHash.Hashing;
using static Andy.FlacHash.Application.Win.CancellableBackgroundOperationRunner;

namespace Andy.FlacHash.Application.Win.UI
{
    public class HashComputationServiceFactory
    {
        public static NonBlockingHashComputation Build(
            IReportingMultiFileHasher hasher,
            Control uiUpdateContext,
            CompletionHandler reportCompletion,
            Action<Exception> reportFailure,
            StateChangeHandler stateTransitioned)
        {
            return new NonBlockingHashComputation(
                new CancellableBackgroundOperationRunner(uiUpdateContext),
                hasher,
                reportCompletion, reportFailure, stateTransitioned);
        }
    }
}