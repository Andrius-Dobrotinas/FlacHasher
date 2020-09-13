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

        /// <summary>
        /// Invoked whe the action is completed
        /// </summary>
        public event Action<bool> Finished;

        /// <summary>
        /// Invoked when on transitions between In-progress and not-in-progress state
        /// </summary>
        public event Action<bool> StateChanged;

        private void ToggleActionState(bool inProgress)
        {
            this.inProgress = inProgress;
            StateChanged(inProgress);
        }

        /// <summary>
        /// Starts an action
        /// </summary>
        public void Start(BeginCancellableAction beginCancellableAction)
        {
            if (inProgress)
                throw new InvalidOperationException("More than one action may not be run at a time");

            cancellationTokenSource = new CancellationTokenSource();
            ToggleActionState(true);
            beginCancellableAction(cancellationTokenSource.Token, OnFinished);
        }

        /// <summary>
        /// Cancels a currently running action
        /// </summary>
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        private void OnFinished()
        {
            ToggleActionState(false);

            Finished(cancellationTokenSource.IsCancellationRequested);

            cancellationTokenSource.Dispose();
        }
    }
}