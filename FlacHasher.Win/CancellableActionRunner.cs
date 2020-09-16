using System;
using System.Collections.Generic;
using System.Threading;

namespace Andy.FlacHash.Win
{
    public class CancellableActionRunner
    {
        public delegate void CompletionHandler(bool cancelled);
        public delegate void StateChangeHandler(bool inProgress);

        private readonly CompletionHandler reportCompletion;
        private readonly StateChangeHandler stateChanged;

        private CancellationTokenSource cancellationTokenSource;
        private bool inProgress = false;

        public bool InProgress => inProgress;

        /// <param name="reportCompletion">Invoked when an action is completed</param>
        /// <param name="stateChanged">Invoked on transitions between in-progress and the opposite state</param>
        public CancellableActionRunner(CompletionHandler reportCompletion, StateChangeHandler stateChanged)
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