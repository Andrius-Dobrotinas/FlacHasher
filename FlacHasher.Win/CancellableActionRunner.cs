using System;
using System.Collections.Generic;
using System.Threading;

namespace Andy.FlacHash.Win
{
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
        public void Start(Action<CancellationToken> cancellableAction)
        {
            if (inProgress)
                throw new InvalidOperationException("More than one action may not be run at a time");

            cancellationTokenSource = new CancellationTokenSource();
            ToggleActionState(true);
            cancellableAction(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Cancel a currently running action
        /// </summary>
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Must be invoked when the action finishes
        /// </summary>
        public void OnFinished()
        {
            ToggleActionState(false);

            Finished(cancellationTokenSource.IsCancellationRequested);

            cancellationTokenSource.Dispose();
        }
    }
}