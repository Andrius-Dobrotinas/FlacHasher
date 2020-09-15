using System;
using System.Collections.Generic;
using System.Threading;

namespace Andy.FlacHash.Win
{
    /// <summary>
    /// An action that honors a cancellation token and runs another action when it's done
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="finishedCallback">An action that's run when the main action is finished</param>
    public delegate void BeginCancellableAction(CancellationToken cancellationToken, Action finishedCallback);

    public class CancellableActionRunner
    {
        private CancellationTokenSource cancellationTokenSource;
        private bool inProgress = false;

        public bool InProgress => inProgress;

        private readonly Action<bool> reportCompletion;
        private readonly Action<bool> stateChanged;

        /// <param name="reportCompletion">Invoked when an action is completed</param>
        /// <param name="stateChanged">Invoked on transitions between in-progress and the opposite state</param>
        public CancellableActionRunner(Action<bool> reportCompletion, Action<bool> stateChanged)
        {
            this.reportCompletion = reportCompletion ?? throw new ArgumentNullException(nameof(reportCompletion));
            this.stateChanged = stateChanged ?? throw new ArgumentNullException(nameof(stateChanged));
        }

        private void ToggleActionState(bool inProgress)
        {
            this.inProgress = inProgress;
            stateChanged(inProgress);
        }

        private void OnFinished()
        {
            ToggleActionState(false);

            reportCompletion(cancellationTokenSource.IsCancellationRequested);

            cancellationTokenSource.Dispose();
        }

        /// <summary>
        /// Starts a new action
        /// </summary>
        public void Start(BeginCancellableAction beginCancellableAction)
        {
            if (inProgress)
                throw new InvalidOperationException("More than one action may not be run at a time");

            ToggleActionState(true);

            cancellationTokenSource = CancellableActionStarter.Start(beginCancellableAction, OnFinished);
        }

        /// <summary>
        /// Cancels a currently running action
        /// </summary>
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
    }
}